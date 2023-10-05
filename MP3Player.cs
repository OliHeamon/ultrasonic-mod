using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace MP3Player
{
	public class MP3Player : Mod
	{
        public const string Silence = "Assets/Sounds/Silence";

        public static readonly Color Cyan = new(130, 233, 229);

        public static readonly Color Pink = new(226, 114, 175);

        public static readonly Color MusicalBackgroundColor = new(34, 31, 38);

        public static readonly string CachePath = Path.Combine(Main.SavePath, "MP3PlayerCache");

        public static ModKeybind MP3PlayerBind { get; private set; }

        public static SpriteFont Font { get; private set; }

        public static MP3Player Instance { get; private set; }

        public static List<string> Bosses { get; private set; }

        public MP3Player()
        {
            Instance = this;
            Bosses = new();
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                Directory.CreateDirectory(CachePath);

                SetupFFmpeg(CachePath);

                MP3PlayerBind = KeybindLoader.RegisterKeybind(this, "OpenMusicPlayer", "K");

                Font = Assets.Request<SpriteFont>("Assets/Fonts/MP3-11", AssetRequestMode.ImmediateLoad).Value;

                MusicLoader.AddMusic(this, Silence);
            }
        }

        public override void PostSetupContent()
        {
            PopulateBossList();
        }

        private void SetupFFmpeg(string path)
        {
            string ffmpegPath = Path.Combine(path, "ffmpeg.exe");

            if (File.Exists(ffmpegPath))
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
            TmodFile file = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) as TmodFile;

            IDictionary<string, FileEntry> files = typeof(TmodFile).GetField("files", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(file) as IDictionary<string, FileEntry>;

            FileEntry ffmpeg = files.First(f => f.Key == "ffmpeg.exe").Value;

            byte[] bytes = file.GetBytes(ffmpeg);

            File.WriteAllBytes(ffmpegPath, bytes);
        }

        private void PopulateBossList()
        {
            for (int i = 0; i < NPCLoader.NPCCount; i++)
            {
                NPC npc = new();
                npc.SetDefaults(i);

                if (npc.boss)
                {
                    // For modded bosses the type name needs to be stored, but for vanilla bosses they don't have an associated class so the numerical ID must be stored.
                    if (npc.ModNPC is not null)
                    {
                        Bosses.Add(npc.ModNPC.GetType().FullName);
                    }
                    else
                    {
                        Bosses.Add(i.ToString());
                    }
                }
            }

            // Attempts to remove duplicate boss names caused by worm segments.
            List<string> removeBuffer = new();

            for (int i = 0; i < Bosses.Count; i++)
            {
                string boss = Bosses[i];

                if (boss.EndsWith("Head"))
                {
                    // Remove last 4 characters.
                    string prefixName = boss[..^4];
                    
                    foreach (string checkBoss in Bosses)
                    {
                        if (checkBoss.Contains(prefixName) && !checkBoss.Contains("Head"))
                        {
                            removeBuffer.Add(checkBoss);
                        }
                    }
                }
            }

            foreach (string typeName in removeBuffer)
            {
                Bosses.Remove(typeName);
            }
        }
    }
}