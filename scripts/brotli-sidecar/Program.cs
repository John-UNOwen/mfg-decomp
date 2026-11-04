using System;
using System.IO;
using System.IO.Compression;

// Brotli sidecar generator: writes <file>.br next to each file under a root tree so webfront
// can serve Content-Encoding: br. Files with magic bytes that are already compressed (png/jpg/
// webm/ogg/mp3) are skipped. WASM is NOT skipped: it compresses ~7:1 with brotli (134MB -> 18MB
// measured on the 2026-11-04 lane build); the old skip assumed wasm was pre-compressed and cost
// every client a 134MB transfer.
internal static class Sidecar
{
    private static int Main(string[] argv)
    {
        string root = Path.GetFullPath(argv.Length > 0 ? argv[0] : ".");
        int quality = argv.Length > 1 ? int.Parse(argv[1]) : 6;
        long made = 0, skipped = 0, origBytes = 0, brBytes = 0;
        foreach (string f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (f.EndsWith(".br", StringComparison.OrdinalIgnoreCase)) continue;
            byte[] head = new byte[4];
            using (var fs = File.OpenRead(f)) { if (fs.Read(head, 0, 4) < 4) continue; }
            if (head[0] == 0x89 && head[1] == 0x50) continue;              // png
            if (head[0] == 0xFF && head[1] == 0xD8) continue;              // jpg
            if (head[0] == 0x1A && head[1] == 0x45 && head[2] == 0xDF && head[3] == 0xA3) continue; // webm/mkv
            if (head[0] == 0x4F && head[1] == 0x67 && head[2] == 0x67 && head[3] == 0x53) continue; // ogg
            if (head[0] == 0x49 && head[1] == 0x44 && head[2] == 0x33) continue; // mp3 id3
            byte[] data = File.ReadAllBytes(f);
            if (data.Length < 1024) continue;
            string dst = f + ".br";
            CompressionLevel lvl = quality >= 9 ? CompressionLevel.SmallestSize : CompressionLevel.Optimal;
            using (var fs = File.Create(dst))
            using (var br = new BrotliStream(fs, lvl, false))
                br.Write(data, 0, data.Length);
            made++; origBytes += data.Length; brBytes += new FileInfo(dst).Length;
        }
        Console.WriteLine($"sidecar: made={made} skipped_or_tiny={skipped} orig={origBytes / 1048576}MB br={brBytes / 1048576}MB ratio={(origBytes == 0 ? 0 : Math.Round(100.0 * brBytes / origBytes))}%");
        return 0;
    }
}
