using Microsoft.Xna.Framework.Graphics;
using MP3Player.Common.UI.Abstract;
using MP3Player.Common.UI.Themes;
using MP3Player.Localization;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using MP3Player.Common.UI;

namespace MP3Player.Content.UI.MP3PlayerUI
{
    public class IconButton : SmartUIElement
    {
        private readonly string icon;

        private readonly string tooltip;

        public IconButton(string icon, string tooltip)
        {
            this.icon = icon;
            this.tooltip = tooltip;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            UIHelper.DrawBox(spriteBatch, drawBox, Color.Gray);

            Color iconColor = Color.White;

            Texture2D iconTexture = ThemeSystem.GetIcon(icon);

            Rectangle sourceRectangle = new(Main.mouseLeft && IsMouseHovering ? iconTexture.Width / 2 : 0, 0, iconTexture.Width / 2, iconTexture.Height);

            spriteBatch.Draw(iconTexture, new Vector2(drawBox.X, drawBox.Y), sourceRectangle, iconColor);

            if (IsMouseHovering)
            {
                Main.instance.MouseText(LocalizationHelper.GetGUIText(tooltip));
            }

            base.Draw(spriteBatch);
        }

        public override void SafeMouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(new SoundStyle("MP3Player/Assets/Sounds/UI/ButtonPress"));
        }
    }
}
