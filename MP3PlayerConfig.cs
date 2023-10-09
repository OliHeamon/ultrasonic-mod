using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MP3Player
{
    public class MP3PlayerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool SendNowPlayingMessages { get; set; }
    }
}
