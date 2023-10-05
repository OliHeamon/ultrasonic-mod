// Credit to Scalie for ThemeBoxProvider - https://github.com/ScalarVector1/DragonLens/blob/407a54e45d7a4828f660b46988feaf86092249b3/Core/Systems/ThemeSystem/ThemeBoxProvider.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Localization;

namespace MP3Player.Common.UI.Themes.Providers
{
    /// <summary>
	/// A class which provides the methods used to render boxes for the GUI.
	/// </summary>
	public abstract class ThemeBoxProvider
    {
        /// <summary>
        /// The key for the name of this box provider
        /// </summary>
        public abstract string NameKey { get; }

        public string Name => LocalizationHelper.GetText($"{NameKey}Boxes.Name");

        public string Description => LocalizationHelper.GetText($"{NameKey}Boxes.Description");

        /// <summary>
        /// Draws a simple box. Used for most buttons and smaller backgrounds.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        public abstract void DrawBox(SpriteBatch spriteBatch, Rectangle target, Color color);

        /// <summary>
        /// Draws a simple box with a given texture. Used for most buttons and smaller backgrounds.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        public abstract void DrawBoxWith(SpriteBatch spriteBatch, Texture2D texture, Rectangle target, Color color);

        /// <summary>
        /// Draws a smaller box, used by tiny things like slider ticks.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        public abstract void DrawBoxSmall(SpriteBatch spriteBatch, Rectangle target, Color color);

        /// <summary>
        /// Draws a 'fancy' box. Used by things like popout UI backgrounds.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        public abstract void DrawBoxFancy(SpriteBatch spriteBatch, Rectangle target, Color color);

        /// <summary>
        /// Draws the outline of a box. Used for things like placement previews.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        public abstract void DrawOutline(SpriteBatch spriteBatch, Rectangle target, Color color);
    }
}
