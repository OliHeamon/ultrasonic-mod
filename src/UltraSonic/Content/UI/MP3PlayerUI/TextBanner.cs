using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MP3Player.Content.UI.MP3PlayerUI
{
    public class TextBanner
    {
        private const int TimerCooldown = 180;

        private readonly string text;

        private readonly SpriteFont font;

        private readonly bool scrollingRequired;

        private readonly float maximumOffset;

        private float offset;

        private float scrollTimer;

        private bool reversing;

        public Rectangle Rectangle { get; private set; }

        public TextBanner(string text, Rectangle rectangle, SpriteFont font)
        {
            this.text = text;
            Rectangle = rectangle;
            this.font = font;

            float textWidth = font.MeasureString(text).X;

            scrollingRequired = textWidth > rectangle.Width - 8;

            maximumOffset = rectangle.Width - 8 - textWidth;
        }

        public void UpdateScrolling()
        {
            if (!reversing)
            {
                if (scrollTimer < TimerCooldown)
                {
                    scrollTimer++;
                    return;
                }

                if (offset > maximumOffset)
                {
                    offset -= 1;
                }

                if (offset <= maximumOffset)
                {
                    offset = maximumOffset;

                    reversing = true;
                }
            }
            else
            {
                if (scrollTimer > 0)
                {
                    scrollTimer--;
                    return;
                }

                offset = 0;
                reversing = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            if (!scrollingRequired)
            {
                spriteBatch.DrawString(font, text, position, color);
            }
            else
            {
                RasterizerState state = new()
                {
                    ScissorTestEnable = true,
                    CullMode = CullMode.None
                };

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, state,
                    null, Main.UIScaleMatrix);

                float xScale = Main.UIScaleMatrix.M11;
                float yScale = Main.UIScaleMatrix.M22;

                Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(
                    (int)(Rectangle.X * xScale),
                    (int)(Rectangle.Y * yScale),
                    (int)(Rectangle.Width * xScale),
                    (int)(Rectangle.Height * yScale)
                );

                spriteBatch.DrawString(font, text, position + new Vector2(offset, 0), color);

                Main.instance.GraphicsDevice.ScissorRectangle = Main.instance.GraphicsDevice.Viewport.Bounds;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.UIScaleMatrix);
            }
        }
    }
}
