using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MP3Player
{
	public class MP3PlayerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(true)]
		public bool SendNowPlayingMessages { get; set; }

		[DefaultValue(typeof(Color), "130, 233, 229, 255")]
		public Color Cyan { get; set; }

		[DefaultValue(typeof(Color), "226, 114, 175, 255")]
		public Color Pink { get; set; }

		public override void OnChanged()
		{
			MP3Player.Cyan = Cyan;
			MP3Player.Pink = Pink;
		}
	}
}
