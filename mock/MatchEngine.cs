using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MockServer;

// Stateful gget/gpost match engine for the mock front server (docs/15 contract).
// Speaks the wire exactly as the recovered client parses it: cell-log windows
// (cell_sno start/count + cell_data_{seq} kind=.. pi0..pi3), must= chain parsing
// on gpost, and the NEXT_KYOKU_READY latch between kyokus.
//
// Rule scope v1 (deliberate): 4 seats, tonnan east 1-4, no chi/pon/kan/riichi -
// windows offer only discard / tsumo-win / ron. CPUs tsumogiri-draw and win when
// able. Settlement uses nominal mangan amounts with real tenpai-payment splits.
// The client renders whatever button set and score text the server sends, so
// these simplifications are invisible to it.
internal sealed class MatchEngine
{
    private sealed class Seat
    {
        public bool IsCpu;
        public List<int> Hand = new List<int>();   // Pai ids; drawn tile appended last (client convention)
        public List<int> River = new List<int>();
        public long Score = 30000;
    }

    private sealed class Cell
    {
        public readonly long Seq;
        public readonly int Kind;                  // RECEIVE_COMMAND_TYPE ordinal
        public readonly int PiMask;                // bits 0-3 -> pi0..pi3
        public readonly string Body;               // child element XML

        public Cell(long seq, int kind, int piMask, string body)
        {
            Seq = seq; Kind = kind; PiMask = piMask; Body = body;
        }
    }

    private enum OpenWindow { None, Tsumo, Sute }

    private sealed class Match
    {
        public long NextSeq;
        public bool KyokuStarted;                  // false = between kyokus, awaiting the start trigger
        public bool MatchActive = true;
        public int KyokuCompleted;                 // completed kyoku (ends the match at 4)
        public int Honba;
        public int DealerSeat;
        public int RenchanCount;

        public readonly Seat[] Seats = { new Seat(), new Seat { IsCpu = true }, new Seat { IsCpu = true }, new Seat { IsCpu = true } };
        public readonly List<Cell> Queue = new List<Cell>();

        // kyoku-local
        public List<int> DrawTiles = new List<int>();  // pop by index; DrawIndex advances per draw
        public int DrawIndex;
        public int DoraOpen = 1;                       // how many dora/ura pairs are revealed (no kan in v1)
        public List<int> Dora = new List<int>();
        public List<int> UraDora = new List<int>();
        public int LastDrawPai = -1;
        public int LastDrawSeat = -1;
        public bool LastDrawIsLast;

        public OpenWindow Window = OpenWindow.None;
        public int WindowSeat = -1;
        public long WindowSeq = -1;                  // seq of the open choices cell (answer binds to WindowSeq+1)
        public int PendingDiscardPai = -1;
        public int PendingDiscardSeat = -1;
        public int PendingCpuWinners;                // CPU seats that may ron PendingDiscardPai
    }

    private sealed class Session
    {
        public Match Match;                        // survives end_game until logout so results can be served
        public int MatchingPolls;                  // first gget after entry reports status 0
        public bool ReadySeen;                     // MahjongPlay armed ready=1 once
    }

    private readonly object gate = new object();
    private readonly Dictionary<string, Session> sessions = new Dictionary<string, Session>(StringComparer.OrdinalIgnoreCase);
    private readonly Random rng;

    public MatchEngine() : this((Random)null) { }

    // seeded ctor: the selftest uses fixed seeds so protocol regressions are reproducible
    public MatchEngine(int seed) : this(new Random(seed)) { }

    private MatchEngine(Random rng)
    {
        this.rng = rng ?? new Random();
    }

    // Returns response XML for a session-aware endpoint, or null to fall back to the static spec file.
    public string Handle(string endpoint, string body, Func<string, string> template)
    {
        string pcuid = FormValue(body, "pcuid");
        if (pcuid.Length == 0) pcuid = FormValue(body, "user_id");
        if (pcuid.Length == 0) return null;

        lock (gate)
        {
            switch (endpoint)
            {
                case "entry_game":
                {
                    sessions[pcuid] = new Session { Match = new Match() };
                    Program.Log("MATCH entry_game tid=1027 pindex=0 next_sno=0");
                    return template("entry_game");
                }
                case "gget":
                {
                    Session session;
                    if (!sessions.TryGetValue(pcuid, out session) || session.Match == null) return null;
                    return HandleGget(session, body);
                }
                case "gpost":
                {
                    Session session;
                    if (!sessions.TryGetValue(pcuid, out session) || session.Match == null) return null;
                    return HandleGpost(session, body);
                }
                case "gchat":
                {
                    Session session;
                    return sessions.TryGetValue(pcuid, out session) && session.Match != null ? template("gchat") : null;
                }
                case "end_game":
                case "kiken_game":
                {
                    Session session;
                    if (!sessions.TryGetValue(pcuid, out session) || session.Match == null) return null;
                    session.Match.MatchActive = false;
                    return Mgresult(session.Match);
                }
                case "logout":
                    sessions.Remove(pcuid);
                    return null;
                default:
                    return null;
            }
        }
    }

    // ---------------- gget ----------------

    private string HandleGget(Session session, string body)
    {
        var match = session.Match;
        session.MatchingPolls++;
        string mwait = MatchingPhase(session.MatchingPolls);

        // MahjongPlayFlow arms ready=1 once, then gates on all_ready; deal the first kyoku on that cue.
        bool ready = FormValue(body, "ready") == "1";
        if (ready && !session.ReadySeen)
        {
            session.ReadySeen = true;
            if (match.MatchActive && !match.KyokuStarted) StartKyoku(match);
        }

        var sb = new StringBuilder();
        sb.Append("<game><all_ready>1</all_ready>").Append(mwait);
        AppendWindow(match, sb, ParseMustCursor(body));
        sb.Append("</game>");
        return Res(sb.ToString());
    }

    // mwait: poll 1 keeps MatchingFlow searching, poll 2+ seats three CPU characters.
    private static string MatchingPhase(int poll)
    {
        var sb = new StringBuilder("<mwait>");
        sb.Append("<status>" + (poll == 1 ? 0 : 1) + "</status><pnum>1</pnum><cpu_num>3</cpu_num><pindex>0</pindex>");
        if (poll > 1)
        {
            sb.Append("<mend>");
            sb.Append("<player_0 ptype=\"0\"><zaseki>0</zaseki><client_states>")
              .Append("<state kind=\"player_game\"><data>e30=</data></state>")
              .Append("</client_states></player_0>");
            sb.Append("<player_1 ptype=\"3\"><cpu_level>3</cpu_level><zaseki>1</zaseki><cpu_name>OID_CHARACTER_02</cpu_name></player_1>");
            sb.Append("<player_2 ptype=\"3\"><cpu_level>4</cpu_level><zaseki>2</zaseki><cpu_name>OID_CHARACTER_05</cpu_name></player_2>");
            sb.Append("<player_3 ptype=\"3\"><cpu_level>5</cpu_level><zaseki>3</zaseki><cpu_name>OID_CHARACTER_08</cpu_name></player_3>");
            sb.Append("</mend>");
        }
        sb.Append("</mwait>");
        return sb.ToString();
    }

    // ---------------- gpost ----------------

    private string HandleGpost(Session session, string body)
    {
        var match = session.Match;

        // must = ver/app/tid/pindex/mid/seq/kind/tepai_pos/tepai_pos2/pai_id/tepai_id/tepai_id2/reach/tsumogiri/chi_naki
        string[] must = FormValue(body, "must").Split('/');
        long seq = must.Length > 5 ? ParseLong(must[5]) : -1;
        int kind = must.Length > 6 ? (int)ParseLong(must[6]) : -1;
        int paiId = must.Length > 9 ? (int)ParseLong(must[9]) : 0;
        bool tsumogiri = must.Length > 13 && ParseLong(must[13]) != 0;
        Program.Log("MATCH gpost kind=" + kind + " seq=" + seq + " pai=" + paiId + " window=" + match.Window);

        if (match.MatchActive)
        {
            switch (kind)
            {
                case 2: // SUTE_PAI answering the human draw window
                    if (match.Window == OpenWindow.Tsumo && match.WindowSeat == 0)
                    {
                        match.Window = OpenWindow.None;
                        DiscardFromHuman(match, paiId, tsumogiri);
                    }
                    else Program.Log("MATCH_ANOMALY SUTE_PAI without tsumo window");
                    break;

                case 3: // TSUMO_AGARI (human)
                    if (match.Window == OpenWindow.Tsumo && match.WindowSeat == 0)
                    {
                        match.Window = OpenWindow.None;
                        SettleTsumoAgari(match, 0);
                    }
                    else Program.Log("MATCH_ANOMALY TSUMO_AGARI without window");
                    break;

                case 4: // RON_AGARI (human)
                    if (match.Window == OpenWindow.Sute)
                    {
                        match.Window = OpenWindow.None;
                        SettleRon(match, humanWins: true);
                    }
                    else Program.Log("MATCH_ANOMALY RON_AGARI without sute window");
                    break;

                case 11: // NAKINASHI answering a ron offer
                    if (match.Window == OpenWindow.Sute)
                    {
                        match.Window = OpenWindow.None;
                        int discardSeat = match.PendingDiscardSeat;
                        if (match.PendingCpuWinners != 0) SettleRon(match, humanWins: false);
                        else CascadeFrom(match, (discardSeat + 1) % 4);
                    }
                    else Program.Log("MATCH_ANOMALY NAKINASHI without sute window");
                    break;

                case 13: // KIKEN (resign)
                    match.MatchActive = false;
                    match.Queue.Clear();
                    match.Window = OpenWindow.None;
                    Program.Log("MATCH resign");
                    break;

                case 15: // NEXT_KYOKU_READY: the only inter-kyoku trigger (CPU readiness is implicit)
                    if (!match.MatchActive) Program.Log("MATCH_ANOMALY NEXT_KYOKU_READY after match end");
                    else if (!match.KyokuStarted) StartKyoku(match);
                    else Program.Log("MATCH_ANOMALY NEXT_KYOKU_READY while kyoku running");
                    break;

                default:
                    Program.Log("MATCH_ANOMALY unhandled gpost kind=" + kind);
                    break;
            }
        }

        var sb = new StringBuilder("<game>");
        if (match.MatchActive) sb.Append(MatchingPhase(2)); // restate the roster the client already accepted
        AppendWindow(match, sb, cursor: seq);
        sb.Append("</game>");
        return Res(sb.ToString());
    }

    // ---------------- kyoku machinery (all recursion depth <= wall length) ----------------

    private void StartKyoku(Match match)
    {
        var wall = Mahjong.BuildWall136(rng);
        if (Program.TestTePai is { Length: 13 } testTe && match.KyokuCompleted == 0 && match.DealerSeat == 0)
        {
            // Test deal: the chosen seat takes the requested kinds (first free copy each);
            // the rest keeps its shuffled order so all the generic slicing below still
            // conserves every tile. The win kind is moved to that seat's first draw
            // (draw index == seat, since seat 0 draws first). Seat 0 proves the client's
            // agari window; a CPU seat proves the engine settles agari live.
            int testSeat = Math.Clamp(Program.TestSeat, 0, 3);
            var rest = new List<int>(wall);
            var hand = new List<int>();
            foreach (int kind in testTe)
            {
                int idx = rest.FindIndex(p => Mahjong.KindOfPai(p) == kind);
                hand.Add(rest[idx]);
                rest.RemoveAt(idx);
            }
            if (Program.TestWinKind >= 0)
            {
                int target = 39 + testSeat;                        // rest[target] == DrawTiles[testSeat]
                int win = rest.FindIndex(target, rest.Count - target, p => Mahjong.KindOfPai(p) == Program.TestWinKind);
                if (win > target) (rest[target], rest[win]) = (rest[win], rest[target]);
            }
            int at = 13 * testSeat;                                // hand lands in the seat's deal slot
            wall = new List<int>(rest.Take(at));
            wall.AddRange(hand.OrderBy(p => p));
            wall.AddRange(rest.Skip(at));
        }
        int pos = 0;
        foreach (var seat in match.Seats)
        {
            seat.Hand = wall.Skip(pos).Take(13).OrderBy(p => p).ToList();
            seat.River = new List<int>();
            pos += 13;
        }
        // draws from the front of the remaining wall (136 - 52 dealt - 14 dead wall = 70);
        // the tail slices become dora/ura/rinshan filler the client only displays
        match.DrawTiles = wall.Skip(52).Take(70).ToList();
        match.DrawIndex = 0;
        match.Dora = wall.Skip(122).Take(5).ToList();
        match.UraDora = wall.Skip(127).Take(5).ToList();
        var rinshan = wall.Skip(132).Take(4).ToList();

        match.KyokuStarted = true;
        match.Window = OpenWindow.None;
        match.LastDrawPai = -1;
        match.LastDrawSeat = -1;
        match.LastDrawIsLast = false;

        int dealer = match.DealerSeat;
        int kyokuField = Math.Min(match.KyokuCompleted, 3);
        var seatsXml = new StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            seatsXml.Append("<player_info" + i + "><jikaze>" + ((i - dealer + 4) % 4) + "</jikaze><tepai>" +
                            Mahjong.JoinPais(match.Seats[i].Hand) + "</tepai><score>" +
                            match.Seats[i].Score.ToString(CultureInfo.InvariantCulture) + "</score><rank>" +
                            RankOf(match.Seats, i) + "</rank></player_info" + i + ">");
        }

        string body =
            "<chicya>0</chicya><oya>" + dealer + "</oya><sai>1 1</sai><ba>0</ba><kyoku>" + kyokuField + "</kyoku>" +
            "<all_last>" + (kyokuField == 3 ? 1 : 0) + "</all_last><honba>" + match.Honba + "</honba>" +
            "<rencyan>" + match.RenchanCount + "</rencyan><kyoutaku>0</kyoutaku><nokori>" + match.DrawTiles.Count + "</nokori>" +
            "<dora_open>1</dora_open><dora>" + Mahjong.JoinPais(match.Dora) + "</dora><ura_dora>" + Mahjong.JoinPais(match.UraDora) + "</ura_dora>" +
            "<yama_cnt>" + match.DrawTiles.Count + "</yama_cnt><yama>" + Mahjong.JoinPais(match.DrawTiles) + "</yama>" +
            "<rinshan>" + Mahjong.JoinPais(rinshan) + "</rinshan>" +
            seatsXml;
        Append(match, 17, 0b1111, body);
        Program.Log("MATCH KYOKUSTART kyoku=" + kyokuField + " honba=" + match.Honba + " dealer=" + dealer);

        CascadeFrom(match, dealer);
    }

    // Runs draws from `seat` forward until the human must act, someone wins, or the wall dies.
    private void CascadeFrom(Match match, int seat)
    {
        if (!match.MatchActive || !match.KyokuStarted || match.Window != OpenWindow.None) return;
        if (match.DrawIndex >= match.DrawTiles.Count)
        {
            SettleRyuKyoku(match);
            return;
        }

        int pai = match.DrawTiles[match.DrawIndex++];
        int drawsLeft = match.DrawTiles.Count - match.DrawIndex;
        match.LastDrawPai = pai;
        match.LastDrawSeat = seat;
        match.LastDrawIsLast = drawsLeft == 0;
        match.Seats[seat].Hand.Add(pai);
        Append(match, 1, 0b1111, "<pindex>" + seat + "</pindex><pai>" + pai + "</pai>");

        bool winnable = Mahjong.IsWinPais(match.Seats[seat].Hand);
        if (seat == 0)
        {
            int select = 0x400;                                   // SUTE
            if (winnable) select |= 0x80;                         // TSUMOAGARI
            string body = "<select>" + select + "</select>";
            if (drawsLeft == 0) body += "<kuikae_pai>0</kuikae_pai>"; // no shibori enforcement needed: CPU ron is settled by the server
            Append(match, 15, 1 << seat, body);
            match.Window = OpenWindow.Tsumo;
            match.WindowSeat = 0;
            return;
        }

        Append(match, 15, 1 << seat, "<select>" + (winnable ? 0x480 : 0x400) + "</select>");
        if (winnable)
        {
            SettleTsumoAgari(match, seat);
            return;
        }

        // CPU tsumogiri, then claim check on that discard (v1: only the human may claim)
        int discard = TsumogiriDiscard(match, seat);
        if (CanRon(match, 0, discard))
        {
            match.Window = OpenWindow.Sute;
            match.WindowSeat = 0;
            Append(match, 16, 0b0001, SuteChoicesBody(0x81, discard, 0));
            return;
        }
        if (drawsLeft == 0)
        {
            SettleRyuKyoku(match);
            return;
        }
        CascadeFrom(match, (seat + 1) % 4);
    }

    // Removes the last tile (drawn) from `seat`'s hand, records the river, emits SUTEHAI.
    private int TsumogiriDiscard(Match match, int seat)
    {
        var hand = match.Seats[seat].Hand;
        int pai = hand[hand.Count - 1];
        hand.RemoveAt(hand.Count - 1);
        match.Seats[seat].River.Add(pai);
        Append(match, 2, 0b1111, "<pindex>" + seat + "</pindex><pai>" + pai + "</pai><stat>2</stat>"); // bit1 tsumogiri
        match.PendingDiscardPai = pai;
        match.PendingDiscardSeat = seat;
        return pai;
    }

    private void DiscardFromHuman(Match match, int pai, bool tsumogiri)
    {
        var hand = match.Seats[0].Hand;
        int index = tsumogiri ? hand.Count - 1 : hand.IndexOf(pai);
        if (index < 0) index = hand.Count - 1;                    // defensive: never desync the log
        int discard = hand[index];
        hand.RemoveAt(index);
        match.Seats[0].River.Add(discard);
        Append(match, 2, 0b1111, "<pindex>0</pindex><pai>" + discard + "</pai><stat>" + (tsumogiri ? 2 : 0) + "</stat>");
        match.PendingDiscardPai = discard;
        match.PendingDiscardSeat = 0;

        int cpuWinners = RonMask(match, discard);
        bool humanAlso = CanRon(match, 0, discard);               // own-discard self-ron is impossible; this is the *other* direction check kept for symmetry
        if (cpuWinners != 0)
        {
            match.PendingCpuWinners = cpuWinners;
            if (humanAlso)
            {
                // multi-ron: offer the human first; the answer (ron or pass) resolves the log afterwards
                match.Window = OpenWindow.Sute;
                match.WindowSeat = 0;
                Append(match, 16, 0b0001, SuteChoicesBody(0x81, discard, 0));
            }
            else SettleRon(match, humanWins: false);
            return;
        }

        if (match.LastDrawIsLast)
        {
            SettleRyuKyoku(match);
            return;
        }
        CascadeFrom(match, 1);
    }

    // ---------------- ron / tsumo / draw settlement ----------------

    // CPU seats that may ron `pai` (furiten honored). Discarding seat cannot ron its own tile.
    private static int RonMask(Match match, int pai)
    {
        int mask = 0;
        int discarder = match.PendingDiscardSeat;
        int[] available = null;
        for (int s = 1; s < 4; s++)
        {
            if (s == discarder) continue;
            var seat = match.Seats[s];
            if (seat.River.Contains(pai)) continue;
            if (seat.Hand.Count != 13) continue;
            if (available == null) available = AvailableCounts(match);
            if (Mahjong.Machi(seat.Hand, available).Contains(pai)) mask |= 1 << s;
        }
        return mask;
    }

    private static bool CanRon(Match match, int seat, int pai)
    {
        var s = match.Seats[seat];
        if (s.River.Contains(pai)) return false;                  // furiten
        if (s.Hand.Count != 13) return false;
        return Mahjong.Machi(s.Hand, AvailableCounts(match)).Contains(pai);
    }

    private static int[] AvailableCounts(Match match)
    {
        var counts = new int[Mahjong.KindCount];
        for (int k = 0; k < counts.Length; k++) counts[k] = 4;
        foreach (var seat in match.Seats)
        {
            foreach (int pai in seat.Hand) DecrementKind(counts, pai);
            foreach (int pai in seat.River) DecrementKind(counts, pai);
        }
        return counts;
    }

    private static void DecrementKind(int[] counts, int pai)
    {
        int kind = Mahjong.KindOfPai(pai);
        if (kind >= 0 && counts[kind] > 0) counts[kind]--;
    }

    private void SettleTsumoAgari(Match match, int winner)
    {
        var seat = match.Seats[winner];
        int dora = DoraCount(match, seat.Hand);
        int han = 1 + dora;

        // tsumo payments: dealer tsumo collects 8000 from each seat; otherwise the dealer pays 8000 and the two others 4000
        var deltas = new long[4];
        for (int s = 0; s < 4; s++)
        {
            if (s == winner) continue;
            long pay = (winner == match.DealerSeat || s == match.DealerSeat) ? 8000 : 4000;
            deltas[s] -= pay;
            deltas[winner] += pay;
        }
        ApplyDeltas(match, deltas);

        var body = new StringBuilder();
        body.Append("<pindex>").Append(winner).Append("</pindex>");
        body.Append("<yaku>").Append(YakuInner(seat.Hand, match.LastDrawPai, han, dora, tsumo: true, last: match.LastDrawIsLast)).Append("</yaku>");
        body.Append("<dora_open>1</dora_open><dora>").Append(Mahjong.JoinPais(match.Dora)).Append("</dora>")
            .Append("<ura_dora>").Append(Mahjong.JoinPais(match.UraDora)).Append("</ura_dora>");
        body.Append(CalcScores(match, deltas));
        Append(match, 3, 0b1111, body.ToString());
        Program.Log("MATCH AGARI tsumo seat=" + winner + " han=" + han + " pai=" + match.LastDrawPai);
        FinishKyoku(match, dealerRetain: winner == match.DealerSeat);
    }

    private void SettleRon(Match match, bool humanWins)
    {
        int pai = match.PendingDiscardPai;
        int discarder = match.PendingDiscardSeat;
        int mask = match.PendingCpuWinners;
        if (humanWins) mask |= 1;
        match.PendingCpuWinners = 0;

        var winners = new List<int>();
        for (int s = 0; s < 4; s++) if ((mask & (1 << s)) != 0) winners.Add(s);
        int primary = winners[0];

        var deltas = new long[4];
        foreach (int w in winners) deltas[w] += 12000;            // nominal mangan per winner
        deltas[discarder] -= 12000L * winners.Count;
        ApplyDeltas(match, deltas);

        var body = new StringBuilder();
        body.Append("<furikomi_pindex>").Append(discarder).Append("</furikomi_pindex>");
        var flags = new List<string>();
        for (int s = 0; s < 4; s++) flags.Add(((mask & (1 << s)) != 0) ? "1" : "0");
        body.Append("<ron_flg>" + string.Join(" ", flags) + "</ron_flg>");
        body.Append("<dora_open>1</dora_open><dora>" + Mahjong.JoinPais(match.Dora) + "</dora>")
            .Append("<ura_dora>" + Mahjong.JoinPais(match.UraDora) + "</ura_dora>");
        for (int s = 0; s < 4; s++)
        {
            bool won = (mask & (1 << s)) != 0;
            var seat = match.Seats[s];
            body.Append("<yaku" + s + ">");
            if (won)
            {
                int dora = DoraCount(match, seat.Hand);
                body.Append(YakuInner(seat.Hand, pai, 1 + dora, dora, tsumo: false, last: false));
            }
            else body.Append("<pai>0</pai><yaku_han>0</yaku_han><han_num>0</han_num><fu_num>0</fu_num><dora_num>0</dora_num>" +
                             "<yaku1>0</yaku1><yaku2>0</yaku2><tepai>0</tepai>" + EmptyNaki());
            body.Append("</yaku" + s + ">");
        }
        body.Append(CalcScores(match, deltas));
        Append(match, 4, 0b1111, body.ToString());
        Program.Log("MATCH AGARI ron mask=" + mask + " pai=" + pai + " from=" + discarder);
        FinishKyoku(match, dealerRetain: primary == match.DealerSeat);
    }

    private void SettleRyuKyoku(Match match)
    {
        var tenpai = new bool[4];
        var machi = new List<int>[4];
        var available = AvailableCounts(match);
        for (int s = 0; s < 4; s++)
        {
            // the seat that drew the final tile still holds 14; tenpai is judged on the 13
            // they keep after discarding it, so pull the drawn tile out before evaluating
            var hand = match.Seats[s].Hand;
            if (match.LastDrawIsLast && match.LastDrawSeat == s && hand.Count == 14)
            {
                var copy = new List<int>(hand);
                copy.Remove(match.LastDrawPai);
                machi[s] = Mahjong.Machi(copy, available);
            }
            else
            {
                machi[s] = Mahjong.Machi(hand, available);
            }
            tenpai[s] = machi[s].Count > 0;
        }

        var deltas = new long[4];
        int t = tenpai.Count(x => x);
        if (t > 0 && t < 4)
            for (int s = 0; s < 4; s++)
                deltas[s] = tenpai[s] ? 1000L * (4 - t) : -1000L * t;   // real 1000-per-pair tenpai payment split
        ApplyDeltas(match, deltas);

        var sb = new StringBuilder();
        for (int s = 0; s < 4; s++)
        {
            sb.Append("<ryukyoku_status" + s + "><end_stat>" + (tenpai[s] ? 1 : 0) + "</end_stat>")
              .Append("<machi_pai>" + (machi[s].Count > 0 ? Mahjong.JoinPais(machi[s]) : "0") + "</machi_pai>")
              .Append("</ryukyoku_status" + s + ">");
        }
        Append(match, 5, 0b1111, sb.ToString() + CalcScores(match, deltas));
        Program.Log("MATCH RYUKYOKU tenpai_count=" + t);
        FinishKyoku(match, dealerRetain: false, drawWithTenpai: t > 0);
    }

    private static void ApplyDeltas(Match match, long[] deltas)
    {
        for (int s = 0; s < 4; s++) match.Seats[s].Score += deltas[s];
    }

    private void FinishKyoku(Match match, bool dealerRetain, bool drawWithTenpai = false)
    {
        match.KyokuStarted = false;
        match.Window = OpenWindow.None;

        bool over;
        if (dealerRetain)
        {
            match.RenchanCount++;
            match.Honba++;
            over = match.RenchanCount >= 4 || match.KyokuCompleted >= 4;   // tonnan: renchan extends east-4
        }
        else
        {
            if (drawWithTenpai) match.Honba++; else match.Honba = 0;
            match.KyokuCompleted++;
            match.DealerSeat = (match.DealerSeat + 1) % 4;
            match.RenchanCount = 0;
            over = match.KyokuCompleted >= 4;
        }
        Append(match, 23, 0b1111, "<end_stat>" + (over ? 1 : 0) + "</end_stat>");
        if (over) match.MatchActive = false;
        Program.Log("MATCH KYOKUEND end_stat=" + (over ? 1 : 0) + " scores=[" +
                    string.Join(",", match.Seats.Select(x => x.Score.ToString(CultureInfo.InvariantCulture))) + "]");
    }

    // ---------------- cell + window emission ----------------

    private void Append(Match match, int kind, int piMask, string body)
    {
        match.Queue.Add(new Cell(match.NextSeq++, kind, piMask, body));
    }

    private static void AppendWindow(Match match, StringBuilder sb, long cursor)
    {
        sb.Append("<taikyoku><cell_info available=\"1\">");
        if (match.Queue.Count == 0)
        {
            sb.Append("<cell_sno start=\"" + cursor.ToString(CultureInfo.InvariantCulture) + "\" count=\"0\" />");
        }
        else
        {
            long start = match.Queue[0].Seq;
            sb.Append("<cell_sno start=\"" + start.ToString(CultureInfo.InvariantCulture) +
                      "\" count=\"" + match.Queue.Count.ToString(CultureInfo.InvariantCulture) + "\" />");
            foreach (var cell in match.Queue)
            {
                string seqText = cell.Seq.ToString(CultureInfo.InvariantCulture);
                sb.Append("<cell_data_" + seqText + " kind=\"" + cell.Kind + "\"" +
                          " pi0=\"" + (cell.PiMask & 1) + "\" pi1=\"" + ((cell.PiMask >> 1) & 1) + "\"" +
                          " pi2=\"" + ((cell.PiMask >> 2) & 1) + "\" pi3=\"" + ((cell.PiMask >> 3) & 1) + "\">")
                  .Append(cell.Body)
                  .Append("</cell_data_" + seqText + ">");
            }
            match.Queue.Clear();
        }
        sb.Append("</cell_info></taikyoku>");
    }

    // ---------------- wire helpers ----------------

    private static string SuteChoicesBody(int select, int discard, int addressed)
    {
        return "<select>" + select.ToString(CultureInfo.InvariantCulture) + "</select><naki>0</naki><pindex>" +
               addressed.ToString(CultureInfo.InvariantCulture) + "</pindex><sute_pai>" +
               discard.ToString(CultureInfo.InvariantCulture) + "</sute_pai>";
    }

    // YakuBit bit positions = MFG.Types.Yaku enum values: Haitei 43, Houtei 44, Pinfu 49, Menzen 52, Dora 58.
    // The bit set only drives the yaku-name line; the score text comes from calc_score.
    private static string YakuInner(List<int> hand, int winPai, int han, int dora, bool tsumo, bool last)
    {
        ulong bits = 1UL << (tsumo ? 52 : 49);
        if (tsumo && last) bits |= 1UL << 43;                     // haitei
        if (dora > 0) bits |= 1UL << 58;
        var display = hand.OrderBy(p => p).ToList();
        if (!tsumo) display.Add(winPai);                          // winner's hand plus the claimed tile
        display.Sort();
        return "<pai>" + winPai + "</pai><yaku_han>" + han + "</yaku_han><han_num>" + han + "</han_num><fu_num>30</fu_num>" +
               "<dora_num>" + dora + "</dora_num><yaku1>" + (bits & 0xFFFFFFFF).ToString(CultureInfo.InvariantCulture) + "</yaku1>" +
               "<yaku2>" + ((bits >> 32) & 0xFFFFFFFF).ToString(CultureInfo.InvariantCulture) + "</yaku2><tepai>" +
               Mahjong.JoinPais(display) + "</tepai>" + EmptyNaki();
    }

    private static string EmptyNaki()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            sb.Append("<naki" + i + "><type>0</type><kantype>0</kantype>");
            for (int j = 0; j < 4; j++) sb.Append("<pai" + j + "><pai_st>0</pai_st><pai>0</pai></pai" + j + ">");
            sb.Append("</naki" + i + ">");
        }
        return sb.ToString();
    }

    // calc_score blocks are read AFTER the seat scores were updated, so new_score is the seat's
    // current value and before_score is back-derived from the delta that produced it.
    private static string CalcScores(Match match, long[] deltas)
    {
        var sb = new StringBuilder();
        for (int s = 0; s < 4; s++)
        {
            long after = match.Seats[s].Score;
            long before = after - deltas[s];
            sb.Append("<calc_score" + s + "><before_score>" + before.ToString(CultureInfo.InvariantCulture) +
                      "</before_score><yaku_score>" + deltas[s].ToString(CultureInfo.InvariantCulture) +
                      "</yaku_score><kyotaku_score>0</kyotaku_score><tumifu_score>0</tumifu_score><new_score>" +
                      after.ToString(CultureInfo.InvariantCulture) + "</new_score><wherefore>0</wherefore></calc_score" + s + ">");
        }
        return sb.ToString();
    }

    private static int DoraCount(Match match, List<int> hand)
    {
        if (match.Dora.Count == 0) return 0;
        var counts = new int[Mahjong.KindCount];
        foreach (int pai in hand)
        {
            int kind = Mahjong.KindOfPai(pai);
            if (kind >= 0) counts[kind]++;
        }
        // only the first indicator is open in v1 (no kan to flip more)
        return Mahjong.CountDora(counts, match.Dora.Take(match.DoraOpen).ToList());
    }

    // Match standing. The client's Rank enum is 0-based (Rank1 = 0; ScorePanel indexes m_ranks by it),
    // so ranks here are 0..3. Ties break toward the earlier seat (dealer-relative order).
    private static int RankOf(Seat[] seats, int seat)
    {
        long mine = seats[seat].Score;
        int rank = 0;
        for (int i = 0; i < 4; i++)
            if (i != seat && (seats[i].Score > mine || (seats[i].Score == mine && i < seat))) rank++;
        return rank;
    }

    private static string Mgresult(Match match)
    {
        int[] uma = { 30, 20, 10, -60 };                         // tonnan uma, display only
        var sb = new StringBuilder("<mgresult>");
        sb.Append("<gmode>1</gmode><taku_class>1</taku_class><continue_state>0</continue_state><continue_fee>0</continue_fee>");
        sb.Append("<breakdown><exp><base_exp>120</base_exp><group_adjust_exp>0</group_adjust_exp><bonus_list /></exp>")
          .Append("<affinity_point><bonus_list /><affinity_growth_point>100</affinity_growth_point>")
          .Append("<actual_affinity_growth_point>100</actual_affinity_growth_point></affinity_point></breakdown>");
        for (int s = 0; s < 4; s++)
        {
            int rank = RankOf(match.Seats, s);      // 0-based, see RankOf
            sb.Append("<player_" + s + "><rank>" + rank + "</rank><score>" +
                      match.Seats[s].Score.ToString(CultureInfo.InvariantCulture) + "</score><uma>" +
                      uma[rank] + "</uma></player_" + s + ">");
        }
        sb.Append("</mgresult>");
        return Res(sb.ToString());
    }

    private static string Res(string inner)
    {
        return "<res><serv_st><code>0</code></serv_st><code>0</code>" + inner + "</res>";
    }

    // gget must = 2.0.2/pcuid/tid/pindex/mid/cursor ; gpost must slot 6 is the answering seq.
    private static long ParseMustCursor(string body)
    {
        string[] must = FormValue(body, "must").Split('/');
        return must.Length > 5 ? ParseLong(must[5]) : 0;
    }

    private static long ParseLong(string text)
    {
        long value;
        return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
    }

    internal static string FormValue(string body, string key)
    {
        foreach (string pair in body.Split('&'))
        {
            int eq = pair.IndexOf('=');
            if (eq <= 0) continue;
            if (string.Equals(pair.Substring(0, eq), key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(pair.Substring(eq + 1));
        }
        return "";
    }
}
