﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using MP3Player.Common.UI.Themes.Providers;

namespace MP3Player.Common.UI.Themes.Musical
{
    public class MusicalBoxProvider : ThemeBoxProvider
    {
        public override string NameKey => "MusicalBoxes";

        public override void DrawBox(SpriteBatch spriteBatch, Rectangle target, Color color)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MP3Player/Assets/UI/Themes/Musical/Box").Value;

            DrawBoxWith(spriteBatch, tex, target, color);
        }

        public override void DrawBoxWith(SpriteBatch spriteBatch, Texture2D tex, Rectangle target, Color color)
        {
            if (color == default)
                color = new Color(49, 84, 141) * 0.9f;

            var sourceCorner = new Rectangle(0, 0, 6, 6);
            var sourceEdge = new Rectangle(6, 0, 4, 6);
            var sourceCenter = new Rectangle(6, 6, 4, 4);

            Rectangle inner = target;
            inner.Inflate(-6, -6);

            spriteBatch.Draw(tex, inner, sourceCenter, color);

            spriteBatch.Draw(tex, new Rectangle(target.X + 6, target.Y, target.Width - 12, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X, target.Y - 6 + target.Height, target.Height - 12, 6), sourceEdge, color, -(float)System.Math.PI * 0.5f, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X - 6 + target.Width, target.Y + target.Height, target.Width - 12, 6), sourceEdge, color, (float)System.Math.PI, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 6, target.Height - 12, 6), sourceEdge, color, (float)System.Math.PI * 0.5f, Vector2.Zero, 0, 0);

            spriteBatch.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)System.Math.PI * 0.5f, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)System.Math.PI, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)System.Math.PI * 1.5f, Vector2.Zero, 0, 0);
        }

        public override void DrawBoxFancy(SpriteBatch spriteBatch, Rectangle target, Color color)
        {
            DrawBox(spriteBatch, target, color);
        }

        public override void DrawBoxSmall(SpriteBatch spriteBatch, Rectangle target, Color color)
        {
            DrawBox(spriteBatch, target, color);
        }

        public override void DrawOutline(SpriteBatch spriteBatch, Rectangle target, Color color)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MP3Player/Assets/UI/Themes/Musical/Box").Value;

            if (color == default)
                color = new Color(49, 84, 141) * 0.9f;

            var sourceCorner = new Rectangle(0, 0, 6, 6);
            var sourceEdge = new Rectangle(6, 0, 4, 6);
            var sourceCenter = new Rectangle(6, 6, 4, 4);

            spriteBatch.Draw(tex, new Rectangle(target.X + 6, target.Y, target.Width - 12, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X, target.Y - 6 + target.Height, target.Height - 12, 6), sourceEdge, color, -(float)System.Math.PI * 0.5f, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X - 6 + target.Width, target.Y + target.Height, target.Width - 12, 6), sourceEdge, color, (float)System.Math.PI, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 6, target.Height - 12, 6), sourceEdge, color, (float)System.Math.PI * 0.5f, Vector2.Zero, 0, 0);

            spriteBatch.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)System.Math.PI * 0.5f, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)System.Math.PI, Vector2.Zero, 0, 0);
            spriteBatch.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)System.Math.PI * 1.5f, Vector2.Zero, 0, 0);
        }
    }
}
