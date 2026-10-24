using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MockServer;

// Minimal server-side mahjong logic for the mock table engine.
// Wire tiles use the client's MFG.Types.Pai convention (docs/15 section 8):
// man 1-9, sou 11-19, pin 21-29, honors 31-37 (J1..J7), red fives = plain + 64.
// HaiID kinds 0-33 are internal only: M1=0..M9=8, S1=9..S17, P1=18..P26, J1=27..J7=33.
internal static class Mahjong
{
    public const int KindCount = 34;

    public static int KindOfPai(int pai)
    {
        if (pai >= 64) pai -= 64; // red variant
        if (pai < 1 || pai > 37) return -1;
        int suit = pai / 10;   // 0 man, 1 sou, 2 pin, 3 honors
        int num = pai % 10;    // 1-9, honors 1-7
        if (suit > 3 || num < 1 || (suit < 3 && num > 9)) return -1;
        return suit * 9 + num - 1;
    }

    public static int PaiOfKind(int kind)
    {
        int suit = kind / 9;
        int num = kind % 9 + 1;
        return suit * 10 + num;
    }

    // kind of the tile that the dora indicator `indicator` points at (wrap inside the suit).
    public static int DoraOfIndicator(int indicatorKind)
    {
        int suit = indicatorKind / 9;
        int idx = indicatorKind % 9;
        return suit * 9 + (idx + 1) % 9;
    }

    public static bool IsTerminalOrHonor(int kind) => kind % 9 == 0 || kind % 9 == 8 || kind >= 27;

    // ---- shanten (standard form + chiitoitsu; no kokushu, which cannot occur here) ----

    public static int Shanten(List<int> handPais)
    {
        var counts = new int[KindCount];
        foreach (int pai in handPais)
        {
            int kind = KindOfPai(pai);
            if (kind >= 0) counts[kind]++;
        }
        int best = 8;
        RecurseShanten(counts, 0, 0, 0, false, ref best);

        int distinctPairs = 0;
        bool odd = false;
        for (int k = 0; k < KindCount; k++)
        {
            if (counts[k] >= 2) distinctPairs++;
            else if (counts[k] == 1) odd = true;
        }
        int chiitoi = 6 - distinctPairs + (distinctPairs < (handPais.Count + 1) / 2 && !odd ? 1 : 0);
        return Math.Min(best, Math.Max(chiitoi, -1));
    }

    // DFS assigning every tile to meld / partial meld / pair / discard;
    // shanten = 8 - 2*melds - partials - pair, with melds+partials capped at 4.
    private static void RecurseShanten(int[] counts, int idx, int melds, int partials, bool pair, ref int best)
    {
        while (idx < KindCount && counts[idx] == 0) idx++;

        int current = 8 - 2 * melds - partials - (pair ? 1 : 0);
        if (current < best) best = current;
        if (melds + partials >= 4 && pair) return; // blocks full and the pair exists; nothing else to gain

        if (idx >= KindCount) return;

        // try using counts[idx] as: kotsu, run, pair, kotsu-partial, two-sided run partial, penchan, or junk
        if (melds < 4 && counts[idx] >= 3)
        {
            counts[idx] -= 3;
            RecurseShanten(counts, idx, melds + 1, partials, pair, ref best);
            counts[idx] += 3;
        }
        if (melds < 4 && idx < 27 && idx % 9 <= 6 && counts[idx + 1] > 0 && counts[idx + 2] > 0)
        {
            counts[idx]--; counts[idx + 1]--; counts[idx + 2]--;
            RecurseShanten(counts, idx, melds + 1, partials, pair, ref best);
            counts[idx]++; counts[idx + 1]++; counts[idx + 2]++;
        }
        if (!pair && counts[idx] >= 2)
        {
            counts[idx] -= 2;
            RecurseShanten(counts, idx, melds, partials, true, ref best);
            counts[idx] += 2;
        }
        if (melds + partials < 4 && counts[idx] >= 2)
        {
            counts[idx] -= 2;
            RecurseShanten(counts, idx, melds, partials + 1, pair, ref best);
            counts[idx] += 2;
        }
        if (melds + partials < 4 && idx < 27 && idx % 9 <= 6 && counts[idx + 1] > 0)
        {
            counts[idx]--; counts[idx + 1]--;
            RecurseShanten(counts, idx, melds, partials + 1, pair, ref best);
            counts[idx]++; counts[idx + 1]++;
        }
        if (melds + partials < 4 && idx < 27 && idx % 9 <= 5 && counts[idx + 2] > 0)
        {
            counts[idx]--; counts[idx + 2]--;
            RecurseShanten(counts, idx, melds, partials + 1, pair, ref best);
            counts[idx]++; counts[idx + 2]++;
        }
        // junk: the remaining copies of this kind are dead tiles
        int saved = counts[idx];
        counts[idx] = 0;
        RecurseShanten(counts, idx + 1, melds, partials, pair, ref best);
        counts[idx] = saved;
    }

    public static List<int> BuildWall136(Random rng)
    {
        var wall = new List<int>(136);
        for (int kind = 0; kind < KindCount; kind++)
        {
            int plain = PaiOfKind(kind);
            bool redFive = kind == 4 || kind == 13 || kind == 22;
            for (int i = 0; i < (redFive ? 3 : 4); i++) wall.Add(plain);
            if (redFive) wall.Add(plain + 64);
        }
        for (int i = wall.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (wall[i], wall[j]) = (wall[j], wall[i]);
        }
        return wall;
    }

    // counts[kind] -> can the 3n+2 part decompose entirely into melds (no pair left)?
    private static bool AllMelds(int[] counts)
    {
        int first = -1;
        for (int k = 0; k < KindCount; k++)
            if (counts[k] > 0) { first = k; break; }
        if (first < 0) return true;

        if (counts[first] >= 3)
        {
            counts[first] -= 3;
            bool ok = AllMelds(counts);
            counts[first] += 3;
            if (ok) return true;
        }
        if (first < 27 && first % 9 <= 6 && counts[first + 1] > 0 && counts[first + 2] > 0)
        {
            counts[first]--; counts[first + 1]--; counts[first + 2]--;
            bool ok = AllMelds(counts);
            counts[first]++; counts[first + 1]++; counts[first + 2]++;
            if (ok) return true;
        }
        return false;
    }

    // 14 concealed tiles (no melds yet, v1 rule set): standard 4 melds + pair, or seven pairs.
    public static bool IsWin(int[] counts)
    {
        int total = 0;
        for (int k = 0; k < KindCount; k++) total += counts[k];
        if (total != 14) return false;

        for (int pair = 0; pair < KindCount; pair++)
        {
            if (counts[pair] < 2) continue;
            counts[pair] -= 2;
            bool ok = AllMelds(counts);
            counts[pair] += 2;
            if (ok) return true;
        }

        // chiitoitsu: seven different kinds with exactly 2 each
        bool pairsOnly = true;
        int pairs = 0;
        for (int k = 0; k < KindCount; k++)
        {
            if (counts[k] == 2) pairs++;
            else if (counts[k] != 0) { pairsOnly = false; break; }
        }
        return pairsOnly && pairs == 7;
    }

    public static bool IsWinPais(List<int> pais)
    {
        var counts = new int[KindCount];
        foreach (int pai in pais)
        {
            int kind = KindOfPai(pai);
            if (kind < 0) return false;
            counts[kind]++;
        }
        return IsWin(counts);
    }

    // Waiting tiles for a 13-tile concealed hand; returns Pai ids. `available` optionally
    // filters kinds with no tile left outside the hand (null = no filter).
    public static List<int> Machi(List<int> handPais, int[] available)
    {
        var counts = new int[KindCount];
        foreach (int pai in handPais)
        {
            int kind = KindOfPai(pai);
            if (kind >= 0) counts[kind]++;
        }
        var result = new List<int>();
        for (int k = 0; k < KindCount; k++)
        {
            if (counts[k] >= 4) continue;
            if (available != null && available[k] <= 0) continue;
            counts[k]++;
            bool win = IsWin(counts);
            counts[k]--;
            if (win) result.Add(PaiOfKind(k));
        }
        return result;
    }

    // Count dora (kind, not red bonus) for a concealed hand. Indicators are given as
    // wire Pai ids (what the wire sends) and mapped to their target kinds here.
    public static int CountDora(int[] handCounts, List<int> doraIndicatorPais)
    {
        int sum = 0;
        foreach (int indicator in doraIndicatorPais)
        {
            int kind = KindOfPai(indicator);
            if (kind < 0) continue;
            sum += handCounts[DoraOfIndicator(kind)];
        }
        return sum;
    }

    public static string JoinPais(IEnumerable<int> pais)
    {
        return string.Join(" ", pais.Select(p => p.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }
}
