// Credit to Scalie for ThemeIconProvider - https://github.com/ScalarVector1/DragonLens/blob/407a54e45d7a4828f660b46988feaf86092249b3/Core/Systems/ThemeSystem/ThemeIconProvider.cs

using Microsoft.Xna.Framework.Graphics;
using MP3Player.Localization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.ModLoader;

namespace MP3Player.Common.UI.Themes.Providers
{
    public abstract class ThemeIconProvider
    {
        private readonly Dictionary<string, Texture2D> icons;

        private static readonly List<string> defaultKeysInner = new()
        {
            "Add",
            "Stop",
            "PreviousSong",
            "Rewind",
            "PausePlay",
            "Forward",
            "NextSong",
            "VolumeKnob",
            "VolumeIndicator",
            "Menu",
            "Shuffle",
            "SmallShuffle",
            "SmallLoop",
            "SmallNextSong",
            "Background",
            "NowPlayingText",
            "NowPlayingBox"
        };

        /// <summary>
        /// A list of the default keys that need to be filled out by an IconProvider, provided for convenience for iteration.
        /// </summary>
        public ReadOnlyCollection<string> defaultKeys = new(defaultKeysInner);

        /// <summary>
        /// The name of this icon provider
        /// </summary>
        public abstract string NameKey { get; }

        public string Name => LocalizationHelper.GetText($"{NameKey}Icons.Name");

        public string Description => LocalizationHelper.GetText($"{NameKey}Icons.Description");

        public ThemeIconProvider()
        {
            icons = new Dictionary<string, Texture2D>();
            PopulateIcons(icons);
        }

        public abstract void PopulateIcons(Dictionary<string, Texture2D> icons);

        public Texture2D GetIcon(string key)
        {
            if (icons.ContainsKey(key))
                return icons[key];
            else
                return ModContent.Request<Texture2D>("MP3Player/Assets/UI/Themes/NoBox").Value;
        }
    }
}
