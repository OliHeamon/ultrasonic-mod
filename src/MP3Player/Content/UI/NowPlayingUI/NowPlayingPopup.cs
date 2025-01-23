using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Common.UI.Abstract;
using MP3Player.Common.UI.Themes;
using MP3Player.Content.UI.MP3PlayerUI;
using MP3Player.Core.UI;
using System;
using Terraria;

namespace MP3Player.Content.UI.NowPlayingUI
{
    public class NowPlayingPopup : SmartUIElement
    {
        public bool IsCompleted { get; private set; }

        private readonly string name;

        private readonly string author;

        private readonly int popupDuration;

        private readonly int maxOffset;

        private readonly int lineHeight;

        private readonly int boxHeight;

        private readonly TextBanner[] banners;

        private readonly Rectangle[] bannerRectangles;

        private State drawState;

        private int xOffset;

        private int waitTimer;

        private float textFade;

        public NowPlayingPopup(string name, string author, bool startAlreadyOpen = false)
        {
            this.name = name;
            this.author = author;

            popupDuration = Math.Max((int)(name.Length * 0.2f * 60), 5 * 60);

            Texture2D box = ThemeSystem.GetIcon("NowPlayingBox");

            boxHeight = box.Height;

            xOffset = maxOffset = -box.Width - 32;

            if (startAlreadyOpen)
            {
                xOffset = 0;
                drawState = State.Waiting;
            }

            lineHeight = (int)MP3Player.Font.MeasureString("A").Y;

            banners = new TextBanner[2];
            bannerRectangles = new Rectangle[2];
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            MP3PlayerMainPanel panel = MP3PlayerUILoader.GetUIState<MP3PlayerState>().MainPanel;

            if (panel.ActiveSong == null)
            {
                drawState = State.Closing;
            }

            switch (drawState)
            {
                case State.Opening:

                    xOffset += 20;

                    if (xOffset >= 0)
                    {
                        xOffset = 0;

                        drawState = State.Waiting;
                    }

                    break;
                case State.Waiting:

                    waitTimer++;
                    

                    if (waitTimer > popupDuration)
                    {
                        drawState = State.Closing;
                    }

                    textFade += 0.02f;

                    if (textFade >= 1)
                    {
                        textFade = 1;
                    }

                    break;
                case State.Closing:

                    xOffset -= 20;

                    if (xOffset <= maxOffset)
                    {
                        xOffset = maxOffset;

                        IsCompleted = true;
                    }

                    break;
            }

            if (banners[0] == null)
            {
                Vector2 drawPosition = new(32, Main.screenHeight - boxHeight - 32);

                int drawY = (int)drawPosition.Y + boxHeight - 48;

                // -maxOffset is just box width + 32
                int rectWidth = -maxOffset - 64;

                Rectangle nameRectangle = new((int)drawPosition.X + 16, drawY, rectWidth, lineHeight);

                banners[0] = new TextBanner(name, nameRectangle, MP3Player.Font);
                bannerRectangles[0] = nameRectangle;

                nameRectangle.Y += lineHeight;

                banners[1] = new TextBanner(author, nameRectangle, MP3Player.Font);
                bannerRectangles[1] = nameRectangle;
            }

            for (int i = 0; i < banners.Length; i++)
            {
                banners[i]?.UpdateScrolling();
            }

            base.SafeUpdate(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D title = ThemeSystem.GetIcon("NowPlayingText");
            Texture2D box = ThemeSystem.GetIcon("NowPlayingBox");

            Vector2 drawPosition = new(32 + xOffset, Main.screenHeight - box.Height - 32);

            float progress = 1 - ((float)xOffset / maxOffset);

            spriteBatch.Draw(box, drawPosition, Color.White * 0.9f * progress);
            spriteBatch.Draw(title, drawPosition, Color.White * progress);

            if (banners[0] != null && drawState == State.Waiting)
            {
                for (int i = 0; i < banners.Length; i++)
                {
                    Vector2 position = bannerRectangles[i].TopLeft() + new Vector2(xOffset, 0);

                    banners[i].Draw(spriteBatch, position + new Vector2(2, 0), MP3Player.Pink * textFade);
                    banners[i].Draw(spriteBatch, position, MP3Player.Cyan * textFade);
                }
            }

            base.Draw(spriteBatch);
        }

        private enum State
        {
            Opening,
            Waiting,
            Closing
        }
    }
}
