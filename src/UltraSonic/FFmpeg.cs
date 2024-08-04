using System.Diagnostics;
using System.IO;
using Terraria.ModLoader;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.Compression;
using static Terraria.ModLoader.Core.TmodFile;
using Terraria.ModLoader.UI;

namespace MP3Player;

public static class FFmpeg
{
    private static string platformBinary;

    public static void Initialise(Mod mod)
    {
        platformBinary = GetPlatformBinary();

        string path = MP3Player.CachePath;

        RemoveLegacyFFmpeg(path);

        string binaryPath = Path.Combine(path, platformBinary);

        if (File.Exists(binaryPath))
        {
            return;
        }

		Interface.loadMods.SetLoadStage("MP3Player: Installing ffmpeg...");

        FileEntry ffmpeg = mod.File.files.First(f => f.Key == $"FFmpeg/{platformBinary}.gz").Value;

        ExtractGZipFile(mod.File.GetStream(ffmpeg), binaryPath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
			mod.Logger.Info($"[Linux Compatibility] Making ffmpeg executable. Location: {binaryPath}.");

			File.SetUnixFileMode(binaryPath, UnixFileMode.UserExecute);
        }
    }

    // In older versions of the mod it was just ffmpeg.exe so this needs to be removed from the filesystem.
    private static void RemoveLegacyFFmpeg(string path)
    {
        string legacyFFmpeg = Path.Combine(path, "ffmpeg.exe");

        if (File.Exists(legacyFFmpeg))
        {
            File.Delete(legacyFFmpeg);
        }
    }

    private static string GetPlatformBinary()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"ffmpeg64W.exe";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"ffmpeg64L";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "ffmpeg64M";
        }

        return "How are you using tModLoader on FreeBSD?";
    }

    private static void ExtractGZipFile(Stream stream, string path)
    {
        using (FileStream decompressedFileStream = File.Create(path))
        {
            using (GZipStream decompressionStream = new(stream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedFileStream);
            }
        }

        stream.Close();
    }

    public static Process Run(string path, string args)
    {
        ProcessStartInfo info = new()
        {
            FileName = Path.Combine(path, platformBinary),
            Arguments = args,
            CreateNoWindow = true
        };

        return Process.Start(info);
    }
}
