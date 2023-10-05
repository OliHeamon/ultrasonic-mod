using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Content.UI.MP3PlayerUI.Selection;
using MP3Player.Localization;
using System.IO;
using Terraria.GameContent;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public class ListConditionFinalizationWidget : MP3Widget
    {
        private readonly ListCondition condition;

        private readonly int lineHeight;

        private readonly TextBanner[] banners;

        public ListConditionFinalizationWidget(MP3PlayerMainPanel panel, ListCondition condition) : base(panel)
        {
            this.condition = condition;

            banners = new TextBanner[2];

            lineHeight = (int)MP3Player.Font.MeasureString("A").Y;
        }

        public override void AcceptInput(ButtonType type)
        {
            switch (type)
            {
                // Sets choice to null and returns to the previous page in the event the choice is not wanted.
                case ButtonType.Previous:
                    condition.Choice = null;
                    Panel.ChangeWidget(new ConditionListWidget(Panel, condition), true);
                    break;
                case ButtonType.Next:
                    condition.ApplyChoice();
                    Panel.ChangeWidget(new SongSelectionWidget(Panel), true);
                    break;
            }
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            if (banners[0] == null)
            {
                string infoFile = Path.Combine(MP3Player.CachePath, $"{condition.Uuid}.txt");

                string songName = File.ReadAllLines(infoFile)[0];

                string choice = condition.GetDataDisplay(condition.Choice);

                Rectangle drawBox = GetDimensions().ToRectangle();

                Rectangle screenRectangle = new(drawBox.X + 4, drawBox.Y + (lineHeight * 2) + 4, drawBox.Width - 8, lineHeight);

                banners[0] = new TextBanner(songName, screenRectangle, MP3Player.Font);

                screenRectangle.Y += lineHeight * 2;

                banners[1] = new TextBanner(choice, screenRectangle, MP3Player.Font);
            }

            for (int i = 0; i < banners.Length; i++)
            {
                banners[i].UpdateScrolling();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            DrawTitle(spriteBatch, drawBox);
            DrawWarning(spriteBatch, drawBox);
            DrawButtonIndicators(spriteBatch, drawBox);

            base.Draw(spriteBatch);
        }

        private void DrawTitle(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            string title = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionConfirmChoice");

            spriteBatch.DrawString(MP3Player.Font, title, new(drawBox.X + 4, drawBox.Y + 4), MP3Player.Cyan);
        }

        private void DrawWarning(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            int centerX = drawBox.X + (drawBox.Width / 2);

            int y = drawBox.Y + lineHeight + 4;

            if (banners[0] != null)
            {
                string info = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionThisWillPlay");

                int halfWidth = (int)MP3Player.Font.MeasureString(info).X / 2;

                spriteBatch.DrawString(MP3Player.Font, info, new(centerX - halfWidth, y), MP3Player.Pink);

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, banners[0].Rectangle, MP3Player.Pink);
                banners[0].Draw(spriteBatch, new(drawBox.X + 4, banners[0].Rectangle.Top), MP3Player.Cyan);

                y += lineHeight * 2;

                info = LocalizationHelper.GetGUIText(condition.Info);

                halfWidth = (int)MP3Player.Font.MeasureString(info).X / 2;

                spriteBatch.DrawString(MP3Player.Font, info, new(centerX - halfWidth, y), MP3Player.Pink);

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, banners[1].Rectangle, MP3Player.Pink);
                banners[1].Draw(spriteBatch, new(drawBox.X + 4, banners[1].Rectangle.Top), MP3Player.Cyan);
            }
        }

        private void DrawButtonIndicators(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Rectangle backVerticalLine = new(drawBox.X + 22, drawBox.Y + drawBox.Height - 12, 4, 12);
            Rectangle backHorizontalLine = new(drawBox.X + 22, drawBox.Y + drawBox.Height - 16, 16, 4);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, backVerticalLine, MP3Player.Cyan);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, backHorizontalLine, MP3Player.Cyan);

            string backText = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionBack");

            spriteBatch.DrawString(MP3Player.Font, backText, new(drawBox.X + 44, drawBox.Y + drawBox.Height - 24), MP3Player.Cyan);

            Rectangle conditionVerticalLine = new(drawBox.X + drawBox.Width - 26, drawBox.Y + drawBox.Height - 12, 4, 12);
            Rectangle conditionHorizontalLine = new(drawBox.X + drawBox.Width - 38, drawBox.Y + drawBox.Height - 16, 16, 4);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, conditionVerticalLine, MP3Player.Cyan);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, conditionHorizontalLine, MP3Player.Cyan);

            string conditionText = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionProceed");

            int width = (int)MP3Player.Font.MeasureString(conditionText).X;

            spriteBatch.DrawString(MP3Player.Font, conditionText, new(conditionHorizontalLine.X - width - 4, drawBox.Y + drawBox.Height - 24), MP3Player.Cyan);
        }
    }
}
