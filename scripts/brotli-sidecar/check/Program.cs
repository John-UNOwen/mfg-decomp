using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

// Verify a .br sidecar decompresses byte-identical to its original: checkbr <file.br> <original>
internal static class CheckBr
{
    private static int Main(string[] a)
    {
        if (a.Length < 2) { Console.Error.WriteLine("usage: checkbr <file.br> <original>"); return 2; }
        using var fs = File.OpenRead(a[0]);
        using var bs = new BrotliStream(fs, CompressionMode.Decompress);
        using var ms = new MemoryStream();
        bs.CopyTo(ms);
        byte[] d = ms.ToArray(), o = File.ReadAllBytes(a[1]);
        if (d.Length != o.Length) { Console.WriteLine($"LEN MISMATCH {d.Length} vs {o.Length}"); return 1; }
        byte[] h1 = SHA256.HashData(d), h2 = SHA256.HashData(o);
        for (int i = 0; i < 32; i++) if (h1[i] != h2[i]) { Console.WriteLine("HASH MISMATCH"); return 1; }
        Console.WriteLine($"DECODE EXACT {o.Length} bytes sha256={Convert.ToHexString(h1).Substring(0, 16)}");
        return 0;
    }
}
