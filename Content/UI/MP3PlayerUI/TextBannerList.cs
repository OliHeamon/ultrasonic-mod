using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Localization;
using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace MP3Player.Content.UI.MP3PlayerUI
{
    public class TextBannerList
    {
        private const int Offset = 24;

        private readonly TextBanner[] banners;

        private readonly int lineHeight;

        private List<string> elements;

        public int SelectedIndex { get; private set; }

        public TextBannerList()
        {
            banners = new TextBanner[5];

            lineHeight = (int)MP3Player.Font.MeasureString("A").Y;
        }

        public void SetElements(List<string> elements, Rectangle drawBox)
        {
            this.elements = elements;

            IncrementIndex(0);

            UpdateBanners(drawBox);
        }

        public void UpdateBanners(Rectangle drawBox)
        {
            for (int i = 0; i < banners.Length; i++)
            {
                banners[i] = null;
            }

            for (int i = -2; i < 3; i++)
            {
                if (elements.Count == 0)
                {
                    return;
                }

                int index = SelectedIndex + i;

                Rectangle textRectangle = new(drawBox.X + Offset, drawBox.Y + 4 + lineHeight + (i + 2) * lineHeight, drawBox.Width - Offset, lineHeight);

                if (InRange(index))
                {
                    int bannerIndex = i + 2;

                    string bannerText = elements[index];

                    banners[bannerIndex] = new(bannerText, textRectangle, MP3Player.Font);
                }
                else
                {
                    int bannerIndex = i + 2;

                    string emptyText = "--------";

                    banners[bannerIndex] = new(emptyText, textRectangle, MP3Player.Font);
                }
            }
        }

        private bool InRange(int index) => index >= 0 && index < elements.Count;

        public void DrawSongList(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            int y = drawBox.Y + 4 + lineHeight;

            int listY = y;

            if (elements.Count == 0)
            {
                Vector2 centerPoint = new(drawBox.X + drawBox.Width / 2, drawBox.Y + drawBox.Height / 2);

                string noSongs = LocalizationHelper.GetGUIText("MP3PlayerMenu.NoSongs");

                Vector2 drawPoint = centerPoint - MP3Player.Font.MeasureString(noSongs) / 2;

                spriteBatch.DrawString(MP3Player.Font, noSongs, drawPoint, MP3Player.Pink);
            }
            else
            {
                for (int i = 0; i < banners.Length; i++)
                {
                    TextBanner banner = banners[i];

                    if (i == 2)
                    {
                        spriteBatch.Draw(TextureAssets.MagicPixel.Value, banner.Rectangle, MP3Player.Pink);
                    }

                    banner?.Draw(spriteBatch, new(drawBox.X + Offset, listY), i == 2 ? MP3Player.Cyan : MP3Player.Pink);

                    listY += lineHeight;
                }

                Rectangle verticalLine = new(drawBox.X + 8, y + 2, 4, lineHeight * banners.Length - 8);
                Rectangle horizontalLine = new(drawBox.X + 8, 0, 10, 4);

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, verticalLine, MP3Player.Pink);

                for (int i = 0; i < banners.Length; i++)
                {
                    horizontalLine.Y = y + lineHeight * i + lineHeight / 2;

                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, horizontalLine, MP3Player.Pink);
                }
            }
        }

        public void Update()
        {
            foreach (TextBanner banner in banners)
            {
                banner?.UpdateScrolling();
            }
        }

        public void IncrementIndex(int increment)
        {
            if (elements.Count > 0)
            {
                SelectedIndex += increment;

                if (SelectedIndex < 0)
                {
                    SelectedIndex = elements.Count - 1;
                }

                if (SelectedIndex > elements.Count - 1)
                {
                    SelectedIndex = 0;
                }
            }
        }
    }
}
