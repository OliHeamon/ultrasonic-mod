using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Terraria.ModLoader.Core;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.Compression;
using static Terraria.ModLoader.Core.TmodFile;

namespace MP3Player
{
    public static class FFmpeg
    {
        public static string PlatformBinary => platformBinary;

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

            // Isn't it insane the amount of reflection you have to do to set the mod loading message?
            Assembly assembly = Assembly.GetAssembly(typeof(Mod));

            object loadModsValue = assembly.GetType("Terraria.ModLoader.UI.Interface")
                .GetField("loadMods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            Type uiLoadModsType = assembly.GetType("Terraria.ModLoader.UI.UILoadMods");

            MethodInfo loadStageMethod = uiLoadModsType.GetMethod("SetLoadStage", BindingFlags.Instance | BindingFlags.Public);

            loadStageMethod.Invoke(loadModsValue, new object[] { "MP3Player: Installing ffmpeg...", -1 });

            // Get tmod file entries to copy the included ffmpeg.exe file to the cache folder.
            TmodFile file = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mod) as TmodFile;

            IDictionary<string, FileEntry> files = typeof(TmodFile).GetField("files", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(file) as IDictionary<string, FileEntry>;

            FileEntry ffmpeg = files.First(f => f.Key == $"FFmpeg/{platformBinary}.gz").Value;

            ExtractGZipFile(file.GetStream(ffmpeg), binaryPath);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                int result = Chmod(binaryPath);

                mod.Logger.Info($"[Linux Compatibility] Attempted chmod 777 on file {binaryPath}. Result: {result}");
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
            string architecture = Environment.Is64BitOperatingSystem ? "64" : "32";

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"ffmpeg{architecture}W.exe";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"ffmpeg{architecture}L";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "ffmpeg64M";
            }

            return "How the fuck are you using tModLoader on FreeBSD?";
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

        // This mess is unfortunately necessary because tML isn't on .NET 7 so there's no good API for modifying Linux file permissions.
        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

        private const int S_IRUSR = 0x100;
        private const int S_IWUSR = 0x80;
        private const int S_IXUSR = 0x40;

        private const int S_IRGRP = 0x20;
        private const int S_IWGRP = 0x10;
        private const int S_IXGRP = 0x8;

        private const int S_IROTH = 0x4;
        private const int S_IWOTH = 0x2;
        private const int S_IXOTH = 0x1;

        private static int Chmod(string path)
        {
            // Shotgun all the permissions.
            int _777 = S_IRUSR | S_IWUSR | S_IXUSR
                | S_IRGRP | S_IWGRP | S_IXGRP
                | S_IROTH | S_IWOTH | S_IXOTH;

            return chmod(path, _777);
        }
    }
}
