using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Localization;
using System.Collections.Generic;
using Terraria.GameContent;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public class ConditionListWidget : MP3Widget, ITextBannerListWidget
    {
        private readonly ListCondition condition;

        private readonly List<string> choices;

        private readonly List<string> displayNames;

        private readonly TextBannerList bannerList;

        public ConditionListWidget(MP3PlayerMainPanel panel, ListCondition condition) : base(panel)
        {
            this.condition = condition;

            bannerList = new TextBannerList();

            choices = condition.BackingList();

            displayNames = new();

            foreach (string item in choices)
            {
                displayNames.Add(condition.GetDataDisplay(item));
            }

            bannerList.SetElements(displayNames, GetDimensions().ToRectangle());
        }

        public override void AcceptInput(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Rewind:
                    bannerList.IncrementIndex(-1);
                    UpdateBanners();
                    break;
                case ButtonType.FastForward:
                    bannerList.IncrementIndex(1);
                    UpdateBanners();
                    break;
                case ButtonType.Previous:
                    Panel.ChangeWidget(new ConditionSelectionWidget(Panel, condition.Uuid), true);
                    break;
                case ButtonType.Next:
                    string choice = choices[bannerList.SelectedIndex];

                    condition.Choice = choice;

                    Panel.ChangeWidget(new ListConditionFinalizationWidget(Panel, condition), true);
                    break;
            }
        }

        public void UpdateBanners() => bannerList.UpdateBanners(GetDimensions().ToRectangle());

        public override void SafeUpdate(GameTime gameTime)
        {
            bannerList?.Update();

            base.SafeUpdate(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            DrawTitle(spriteBatch, drawBox);
            bannerList.DrawSongList(spriteBatch, drawBox);
            DrawButtonIndicators(spriteBatch, drawBox);

            base.Draw(spriteBatch);
        }

        private void DrawTitle(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            string title = LocalizationHelper.GetGUIText(condition is BossCondition ?
                "MP3PlayerMenu.ConditionBossSelect" :
                "MP3PlayerMenu.ConditionBiomeSelect");

            spriteBatch.DrawString(MP3Player.Font, title, new(drawBox.X + 4, drawBox.Y + 4), MP3Player.Cyan);
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
