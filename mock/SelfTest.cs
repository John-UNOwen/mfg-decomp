using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace MockServer;

// In-process scripted client that drives MatchEngine exactly the way the game does
// (entry_game -> mwait polls -> ready -> gget/gpost loop -> end_game) and checks the
// protocol invariants the recovered client depends on. No sockets, no game needed.
//
// Human seat policy: accept tsumo/ron when offered, otherwise discard the tile that
// keeps the most winning tiles (greedy tenpai racing), so ron offers, tsumo wins, and
// exhaustive draws all get exercised across the seeded matches.
internal static class SelfTest
{
    private static int failures;

    public static int Run()
    {
        CheckMahjongPrimitives();

        int passed = 0, tsumoMatches = 0, ronMatches = 0, drawMatches = 0;
        for (int m = 0; m < 12; m++)
        {
            var outcome = new Driver(1000 + m).PlayMatch();
            if (outcome.Ok) passed++;
            if (outcome.Tsumo > 0) tsumoMatches++;
            if (outcome.Ron > 0) ronMatches++;
            if (outcome.Draws > 0) drawMatches++;
        }
        Console.WriteLine("selftest: matches_ok=" + passed + "/12 tsumo=" + tsumoMatches + " ron=" + ronMatches + " ryuukyoku=" + drawMatches);
        if (tsumoMatches == 0) Console.WriteLine("selftest: WARN no tsumo exercised");
        if (ronMatches == 0) Console.WriteLine("selftest: WARN no ron exercised");
        if (drawMatches == 0) Console.WriteLine("selftest: WARN no exhaustive draw exercised");
        Console.WriteLine(failures == 0 ? "SELFTEST PASS" : "SELFTEST FAIL");
        return failures == 0 ? 0 : 1;
    }

    private static void Expect(bool condition, string label)
    {
        if (!condition) { failures++; Console.WriteLine("SELFTEST RULE FAIL: " + label); }
    }

    private static void CheckMahjongPrimitives()
    {
        // 123m 456m 789m 111s + 22p pair
        Expect(Mahjong.IsWinPais(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 11, 11, 21, 21 }), "standard form wins");
        var tenpaiHand = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 11, 11, 21 };
        Expect(Mahjong.Machi(tenpaiHand, null).Contains(21), "machi finds the pair wait");
        Expect(!Mahjong.IsWinPais(tenpaiHand), "13 tiles never wins");
        Expect(Mahjong.IsWinPais(new List<int> { 1, 1, 11, 11, 21, 21, 31, 31, 32, 32, 33, 33, 34, 34 }), "seven pairs wins");
        Expect(Mahjong.KindOfPai(69) == 4 && Mahjong.KindOfPai(5) == 4, "red five kind mapping");
        var counts = new int[Mahjong.KindCount];
        counts[Mahjong.KindOfPai(2)] = 2;
        Expect(Mahjong.CountDora(counts, new List<int> { 1 }) == 2, "dora follows the indicator");
        Expect(Mahjong.Shanten(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 11, 11, 21, 21 }) == -1, "agari hand is shanten -1");
        Expect(Mahjong.Shanten(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 11, 11, 21 }) == 0, "tenpai is shanten 0");
        Expect(Mahjong.Shanten(new List<int> { 1, 5, 9, 13, 17, 21, 25, 29, 31, 32, 33, 34, 35 }) >= 4, "scattered 13 is high shanten");
        Expect(Mahjong.Shanten(new List<int> { 1, 1, 11, 11, 21, 21, 31, 31, 32, 32, 33, 33, 34 }) == 0, "six pairs + single is chiitoi tenpai");
    }

    private sealed class Outcome
    {
        public bool Ok;
        public int Tsumo, Ron, Draws;
    }

    private sealed class Driver
    {
        private readonly MatchEngine engine;
        private readonly string pcuid;
        private long cursor;
        private List<int> hand = new List<int>(); // seat 0 mirror, rebuilt from the cell stream
        private int lastDrawn = -1;
        private Outcome outcome = new Outcome();

        public Driver(int seed)
        {
            engine = new MatchEngine(seed);
            pcuid = "selftest" + seed.ToString(CultureInfo.InvariantCulture);
        }

        private static string Template(string spec)
        {
            return "<res><serv_st><code>0</code></serv_st><code>0</code><entry><next_sno>0</next_sno></entry></res>";
        }

        public Outcome PlayMatch()
        {
            engine.Handle("entry_game", "user_id=" + pcuid, Template);

            int polls = 0;
            bool readySent = false;
            while (polls++ < 6000)
            {
                string gget = "user_id=" + pcuid + "&pcuid=" + pcuid + "&gserv_id=1&ready=" +
                              (readySent ? "1" : "0") + "&must=2.0.2/" + pcuid + "/1027/0/1000001/" +
                              cursor.ToString(CultureInfo.InvariantCulture);
                readySent = true;
                XDocument doc = Parse(engine.Handle("gget", gget, Template));
                if (doc == null) return outcome;

                // act on this doc, then on every follow-up gpost response it triggers,
                // exactly like the game consuming its command buffer
                while (doc != null)
                {
                    if (MatchOver(doc)) return Finish();
                    string answer = Decide(doc);
                    if (answer == null) break;
                    doc = Parse(answer);
                }
            }
            Console.WriteLine("selftest: " + pcuid + " request cap reached");
            return outcome;
        }

        private XDocument Parse(string xml)
        {
            if (xml == null) { failures++; Console.WriteLine("SELFTEST FAIL: engine returned null mid-match"); return null; }
            var doc = XDocument.Parse(xml);
            var sno = doc.Descendants("cell_sno").FirstOrDefault();
            if (sno == null) { failures++; Console.WriteLine("SELFTEST FAIL: response without cell_sno"); return null; }
            long start = long.Parse((string)sno.Attribute("start"), CultureInfo.InvariantCulture);
            int count = int.Parse((string)sno.Attribute("count"), CultureInfo.InvariantCulture);
            var cells = doc.Descendants().Where(e => e.Name.LocalName.StartsWith("cell_data_")).ToList();
            if (cells.Count != count) { failures++; Console.WriteLine("SELFTEST FAIL: count=" + count + " nodes=" + cells.Count); return null; }
            if (start != cursor) { failures++; Console.WriteLine("SELFTEST FAIL: start=" + start + " cursor=" + cursor); return null; }
            long expected = start;
            foreach (var cell in cells.OrderBy(e => long.Parse(e.Name.LocalName.Substring(10), CultureInfo.InvariantCulture)))
            {
                long seq = long.Parse(cell.Name.LocalName.Substring(10), CultureInfo.InvariantCulture);
                if (seq != expected) { failures++; Console.WriteLine("SELFTEST FAIL: seq gap " + expected + "->" + seq); return null; }
                if (cell.Attribute("pi0") == null) { failures++; Console.WriteLine("SELFTEST FAIL: cell missing pi attrs"); return null; }
                expected++;
            }
            cursor = start + count;
            foreach (var cell in cells)
            {
                int kind = int.Parse((string)cell.Attribute("kind"), CultureInfo.InvariantCulture);
                switch (kind)
                {
                    case 3: outcome.Tsumo++; break;
                    case 4: outcome.Ron++; break;
                    case 5: outcome.Draws++; break;
                }
            }
            return doc;
        }

        private bool MatchOver(XDocument doc)
        {
            foreach (var cell in doc.Descendants().Where(e => e.Name.LocalName.StartsWith("cell_data_") && (string)e.Attribute("kind") == "23"))
                if ((string)cell.Element("end_stat") == "1") return true;
            return false;
        }

        private Outcome Finish()
        {
            string body = "user_id=" + pcuid + "&pcuid=" + pcuid + "&tid=1027&pindex=0&mid=1000001&version=2.0.2";
            var doc = XDocument.Parse(engine.Handle("end_game", body, Template));
            var players = doc.Descendants("mgresult").Elements().Where(e => e.Name.LocalName.StartsWith("player_")).ToList();
            Expect(players.Count == 4, "mgresult lists all four seats");
            long sum = players.Select(e => long.Parse(e.Element("score").Value, CultureInfo.InvariantCulture)).Sum();
            Expect(sum == 120000, "final scores conserve 120000 (got " + sum + ")");
            outcome.Ok = true;
            Console.WriteLine("selftest: match ok scores=[" + string.Join(",", players.Select(p => p.Element("score").Value)) + "]");
            return outcome;
        }

        // Consume the window in `doc`: mirror seat 0's hand from the log, and answer the
        // last human-addressed window if there is one (the game answers the newest window only).
        private string Decide(XDocument doc)
        {
            XElement openWindow = null;   // TSUMOCHOICES or SUTECHOICES addressed to seat 0
            long openSeq = -1;
            foreach (var cell in doc.Descendants().Where(e => e.Name.LocalName.StartsWith("cell_data_"))
                .OrderBy(e => long.Parse(e.Name.LocalName.Substring(10), CultureInfo.InvariantCulture)))
            {
                long seq = long.Parse(cell.Name.LocalName.Substring(10), CultureInfo.InvariantCulture);
                int kind = int.Parse((string)cell.Attribute("kind"), CultureInfo.InvariantCulture);
                bool forMe = (string)cell.Attribute("pi0") == "1";
                switch (kind)
                {
                    case 17: // KYOKUSTART: mirror the deal for seat 0
                        hand = ParsePais(cell.Descendants("player_info0").First().Element("tepai").Value).ToList();
                        lastDrawn = -1;
                        break;
                    case 1: // TSUMO
                        if (forMe)
                        {
                            lastDrawn = int.Parse(cell.Element("pai").Value, CultureInfo.InvariantCulture);
                            hand.Add(lastDrawn);
                        }
                        break;
                    case 2: // SUTEHAI
                        if (forMe)
                        {
                            int pai = int.Parse(cell.Element("pai").Value, CultureInfo.InvariantCulture);
                            hand.Remove(pai);
                            lastDrawn = -1;
                        }
                        break;
                    case 15:
                        if (forMe) { openWindow = cell; openSeq = seq; }
                        break;
                    case 16:
                        if (forMe) { openWindow = cell; openSeq = seq; }
                        break;
                    case 23: // KYOKUEND(0): latch the next kyoku
                        if ((string)cell.Element("end_stat") == "0") return Gpost(seq + 1, 15, 0, false);
                        return null;
                }
            }

            if (openWindow == null) return null;
            int select = int.Parse(openWindow.Element("select").Value, CultureInfo.InvariantCulture);
            int kind2 = int.Parse((string)openWindow.Attribute("kind"), CultureInfo.InvariantCulture);

            if (kind2 == 15) // own draw window: win or discard
            {
                if ((select & 0x80) != 0) return Gpost(openSeq + 1, 3, lastDrawn, false); // TSUMO_AGARI
                int discard = GreedyDiscard();
                bool tsumogiri = lastDrawn >= 0 && discard == lastDrawn;
                return Gpost(openSeq + 1, 2, discard, tsumogiri);
            }

            // ron offer (v1: the engine only opens sute windows for ron)
            if ((select & 0x80) != 0) return Gpost(openSeq + 1, 4, int.Parse(openWindow.Element("sute_pai").Value, CultureInfo.InvariantCulture), false);
            return Gpost(openSeq + 1, 11, 0, false);
        }

        // Discard policy: play the tile that keeps the lowest shanten (real rule-library
        // grading), tie-broken toward discarding isolated and terminal tiles first and
        // finally toward the drawn tile, which is how the cabinet's own bot behaves.
        private int GreedyDiscard()
        {
            if (hand.Count != 14) return lastDrawn >= 0 ? lastDrawn : 0;
            int bestShanten = 99, bestTie = 99, bestPai = hand[hand.Count - 1];
            for (int i = 0; i < hand.Count; i++)
            {
                var trial = new List<int>(hand);
                int candidate = trial[i];
                trial.RemoveAt(i);
                int shanten = Mahjong.Shanten(trial);
                int kind = Mahjong.KindOfPai(candidate);
                int same = hand.Count(p => Mahjong.KindOfPai(p) == kind);
                int tie = same * 2 + (Mahjong.IsTerminalOrHonor(kind) ? 0 : 1);
                bool prefer = shanten < bestShanten ||
                              (shanten == bestShanten && tie < bestTie) ||
                              (shanten == bestShanten && tie == bestTie && candidate == lastDrawn && i == hand.Count - 1);
                if (prefer) { bestShanten = shanten; bestTie = tie; bestPai = candidate; }
            }
            return bestPai;
        }

        private static IEnumerable<int> ParsePais(string text)
        {
            foreach (string part in text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                yield return int.Parse(part, CultureInfo.InvariantCulture);
        }

        private string Gpost(long seq, int kind, int pai, bool tsumogiri)
        {
            string must = "2.0.2/" + pcuid + "/1027/0/1000001/" +
                          seq.ToString(CultureInfo.InvariantCulture) + "/" + kind + "/13/0/" +
                          pai.ToString(CultureInfo.InvariantCulture) + "/0/0/0/" + (tsumogiri ? "1" : "0") + "/0";
            string body = "user_id=" + pcuid + "&pcuid=" + pcuid + "&gserv_id=1&tid=1027&mid=1000001&pindex=0&must=" + must + "&app_param=0/0";
            return engine.Handle("gpost", body, Template);
        }
    }
}
