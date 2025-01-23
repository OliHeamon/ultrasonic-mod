using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Localization;
using System;
using Terraria.GameContent;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public class ConditionSelectionWidget : MP3Widget
    {
        private readonly string uuid;

        private Condition condition;

        private readonly int conditionMaxIndex;

        public ConditionSelectionWidget(MP3PlayerMainPanel panel, string uuid) : base(panel)
        {
            this.uuid = uuid;

            conditionMaxIndex = Enum.GetValues<Condition>().Length - 1;
        }

        public override void AcceptInput(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Previous:

                    condition--;

                    if (condition < 0)
                    {
                        condition = (Condition)conditionMaxIndex;
                    }

                    break;
                case ButtonType.Next:

                    condition++;

                    if ((int)condition > conditionMaxIndex)
                    {
                        condition = 0;
                    }

                    break;
                case ButtonType.PausePlay:
                    switch (condition)
                    {
                        case Condition.ConditionLastAlive:
                            Panel.ChangeWidget(new LastAliveFinalizationWidget(Panel, uuid), true);
                            break;
                        case Condition.ConditionBoss:
                            Panel.ChangeWidget(new ConditionListWidget(Panel, new BossCondition(uuid)), true);
                            break;
                        case Condition.ConditionBiome:
                            Panel.ChangeWidget(new ConditionListWidget(Panel, new BiomeCondition(uuid)), true);
                            break;
                        case Condition.ConditionWhenDead:
                            Panel.ChangeWidget(new OnDeathFinalizationWidget(Panel, uuid), true);
                            break;
                        case Condition.ConditionCheckOthers:
                            Panel.ChangeWidget(new ConditionViewerWidget(Panel, uuid), true);
                            break;
                    }

                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            DrawTitle(spriteBatch, drawBox);
            DrawConditionInfo(spriteBatch, drawBox);
            DrawButtonIndicators(spriteBatch, drawBox);

            base.Draw(spriteBatch);
        }

        private void DrawTitle(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            string title = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionMenu");

            spriteBatch.DrawString(MP3Player.Font, title, new(drawBox.X + 4, drawBox.Y + 4), MP3Player.Cyan);
        }

        private void DrawConditionInfo(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Vector2 centerPoint = new(drawBox.X + drawBox.Width / 2, drawBox.Y + drawBox.Height / 2);

            string conditionText = LocalizationHelper.GetGUIText($"MP3PlayerMenu.{condition}");

            Vector2 drawPoint = centerPoint - MP3Player.Font.MeasureString(conditionText) / 2;

            spriteBatch.DrawString(MP3Player.Font, conditionText, drawPoint, MP3Player.Pink);
        }

        private void DrawButtonIndicators(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Rectangle prevVerticalLine = new(drawBox.X + 22, drawBox.Y + drawBox.Height - 12, 4, 12);
            Rectangle prevHorizontalLine = new(drawBox.X + 22, drawBox.Y + drawBox.Height - 16, 16, 4);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, prevVerticalLine, MP3Player.Cyan);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, prevHorizontalLine, MP3Player.Cyan);

            string prevText = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionPrevious");

            spriteBatch.DrawString(MP3Player.Font, prevText, new(drawBox.X + 44, drawBox.Y + drawBox.Height - 24), MP3Player.Cyan);

            Rectangle nextVerticalLine = new(drawBox.X + drawBox.Width - 26, drawBox.Y + drawBox.Height - 12, 4, 12);
            Rectangle nextHorizontalLine = new(drawBox.X + drawBox.Width - 38, drawBox.Y + drawBox.Height - 16, 16, 4);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, nextVerticalLine, MP3Player.Cyan);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, nextHorizontalLine, MP3Player.Cyan);

            string nextText = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionNext");

            int width = (int)MP3Player.Font.MeasureString(nextText).X;

            spriteBatch.DrawString(MP3Player.Font, nextText, new(nextHorizontalLine.X - width - 4, drawBox.Y + drawBox.Height - 24), MP3Player.Cyan);

            Rectangle selectVerticalLine = new(drawBox.X + drawBox.Width / 2 - 2, drawBox.Y + drawBox.Height - 12, 4, 12);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, selectVerticalLine, MP3Player.Cyan);

            string selectText = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionSelect");

            Vector2 size = MP3Player.Font.MeasureString(selectText);

            spriteBatch.DrawString(MP3Player.Font, selectText, new(drawBox.X + drawBox.Width / 2 - size.X / 2, selectVerticalLine.Y - size.Y), MP3Player.Cyan);
        }
    }
}
