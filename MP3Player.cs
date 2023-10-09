using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace MP3Player
{
    public class MP3Player : Mod
    {
        public const string Silence = "Assets/Sounds/Silence";

        public static Color Cyan;

        public static Color Pink;

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

                FFmpeg.Initialise(this);

                MP3PlayerBind = KeybindLoader.RegisterKeybind(this, "OpenMusicPlayer", "K");

                Font = Assets.Request<SpriteFont>("Assets/Fonts/MP3-11", AssetRequestMode.ImmediateLoad).Value;

                MusicLoader.AddMusic(this, Silence);
            }
        }

        public override void PostSetupContent()
        {
            PopulateBossList();

            MP3PlayerConfig config = ModContent.GetInstance<MP3PlayerConfig>();

            Cyan = config.Cyan;
            Pink = config.Pink;
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