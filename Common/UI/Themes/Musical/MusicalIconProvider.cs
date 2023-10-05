using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ModLoader;
using MP3Player.Common.UI.Themes.Providers;

namespace MP3Player.Common.UI.Themes.Musical
{
    public class MusicalIconProvider : ThemeIconProvider
    {
        public override string NameKey => "MusicalIcons";

        public override void PopulateIcons(Dictionary<string, Texture2D> icons)
        {
            foreach (string key in defaultKeys)
            {
                icons.Add(key, ModContent.Request<Texture2D>($"MP3Player/Assets/UI/Themes/Musical/Icons/{key}", AssetRequestMode.ImmediateLoad).Value);
            }
        }
    }
}
