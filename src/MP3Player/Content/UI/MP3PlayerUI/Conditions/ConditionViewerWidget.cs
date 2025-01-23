using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Content.IO;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Content.UI.MP3PlayerUI.Selection;
using MP3Player.Core.IO;
using MP3Player.Localization;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public class ConditionViewerWidget : MP3Widget, ITextBannerListWidget
    {
        private readonly string uuid;

        private readonly int lineHeight;

        private readonly List<string> conditions;

        private readonly TextBanner[] banners;

        private int selectedIndex;

        private string conditionInfo;

        public ConditionViewerWidget(MP3PlayerMainPanel panel, string uuid) : base(panel)
        {
            this.uuid = uuid;

            lineHeight = (int)MP3Player.Font.MeasureString("A").Y;

            conditions = new();

            banners = new TextBanner[2];

            RepopulateConditionList();
        }

        public override void AcceptInput(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Previous:
                    IncrementIndex(-1);
                    break;
                case ButtonType.Next:
                    IncrementIndex(1);
                    break;
                case ButtonType.PausePlay:
                    if (conditions.Count > 0)
                    {
                        DeleteSelectedSong();
                    }
                    else
                    {
                        Panel.ChangeWidget(new SongSelectionWidget(Panel), true);
                    }
                    break;
            }
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            if (banners[0] == null)
            {
                UpdateBanners();
            }

            for (int i = 0; i < banners.Length; i++)
            {
                banners[i]?.UpdateScrolling();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            DrawTitle(spriteBatch, drawBox);
            DrawInfo(spriteBatch, drawBox);
            DrawButtonIndicators(spriteBatch, drawBox);

            base.Draw(spriteBatch);
        }

        private void DrawTitle(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            int selected = conditions.Count > 0 ? selectedIndex + 1 : 0;
            int total = conditions.Count;

            string title = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionView", selected, total);

            spriteBatch.DrawString(MP3Player.Font, title, new(drawBox.X + 4, drawBox.Y + 4), MP3Player.Cyan);
        }

        private void DrawInfo(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            if (conditions.Count == 0)
            {
                DrawNoConditionsMessage(spriteBatch, drawBox);

                return;
            }

            DrawSelectedCondition(spriteBatch, drawBox);
        }

        private void DrawNoConditionsMessage(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Vector2 centerPoint = new(drawBox.X + drawBox.Width / 2, drawBox.Y + drawBox.Height / 2);

            string conditionText = LocalizationHelper.GetGUIText($"MP3PlayerMenu.NoExistingConditions");

            Vector2 drawPoint = centerPoint - MP3Player.Font.MeasureString(conditionText) / 2;

            spriteBatch.DrawString(MP3Player.Font, conditionText, drawPoint, MP3Player.Pink);
        }

        private void DrawSelectedCondition(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            int centerX = drawBox.X + (drawBox.Width / 2);

            int y = drawBox.Y + lineHeight + 4;

            if (banners[0] != null)
            {
                string info = LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionThisPlays");

                int halfWidth = (int)MP3Player.Font.MeasureString(info).X / 2;

                spriteBatch.DrawString(MP3Player.Font, info, new(centerX - halfWidth, y), MP3Player.Pink);

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, banners[0].Rectangle, MP3Player.Pink);
                banners[0].Draw(spriteBatch, new(drawBox.X + 4, banners[0].Rectangle.Top), MP3Player.Cyan);

                y += lineHeight * 2;

                halfWidth = (int)MP3Player.Font.MeasureString(conditionInfo).X / 2;

                spriteBatch.DrawString(MP3Player.Font, conditionInfo, new(centerX - halfWidth, y), MP3Player.Pink);

                // Extra line for list conditions.
                if (banners[1] != null)
                {
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, banners[1].Rectangle, MP3Player.Pink);
                    banners[1].Draw(spriteBatch, new(drawBox.X + 4, banners[1].Rectangle.Top), MP3Player.Cyan);
                }
            }
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

            string selectText = conditions.Count > 0 ? LocalizationHelper.GetGUIText("MP3PlayerMenu.DeleteCondition") :
                LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionBack");

            Vector2 size = MP3Player.Font.MeasureString(selectText);

            spriteBatch.DrawString(MP3Player.Font, selectText, new(drawBox.X + drawBox.Width / 2 - size.X / 2, selectVerticalLine.Y - size.Y), MP3Player.Cyan);
        }

        private void RepopulateConditionList()
        {
            conditions.Clear();

            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            // Populate the condition list with all conditions pertaining to this song's UUID.
            foreach (KeyValuePair<string, string> condition in store.Conditions)
            {
                if (condition.Value == uuid)
                {
                    conditions.Add(condition.Key);
                }
            }

            IncrementIndex(0);
        }

        private void IncrementIndex(int increment)
        {
            if (conditions.Count > 0)
            {
                selectedIndex += increment;

                if (selectedIndex < 0)
                {
                    selectedIndex = conditions.Count - 1;
                }

                if (selectedIndex > conditions.Count - 1)
                {
                    selectedIndex = 0;
                }
            }

            UpdateBanners();
        }

        private void DeleteSelectedSong()
        {
            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            string condition = conditions[selectedIndex];

            if (condition.StartsWith(MP3PlayerDataStore.BossConditionKey))
            {
                BossCondition bossCondition = new(uuid)
                {
                    Choice = condition.Split(':')[1]
                };

                bossCondition.RemoveCondition();
            }
            else if (condition.StartsWith(MP3PlayerDataStore.BiomeConditionKey))
            {
                BiomeCondition biomeCondition = new(uuid)
                {
                    Choice = condition.Split(':')[1]
                };

                biomeCondition.RemoveCondition();
            }
            else if (condition.StartsWith(MP3PlayerDataStore.LastManStandingKey))
            {
                store.DeleteSongDataByCondition(condition);

                Main.NewText(LocalizationHelper.GetGUIText("MP3PlayerMenu.ClearedLastManStanding"), MP3Player.Pink);
            }
            else if (condition.StartsWith(MP3PlayerDataStore.OnDeathKey))
            {
                store.DeleteSongDataByCondition(condition);

                Main.NewText(LocalizationHelper.GetGUIText("MP3PlayerMenu.ClearedOnDeath"), MP3Player.Pink);
            }

            conditions.RemoveAt(selectedIndex);

            RepopulateConditionList();
        }

        private string GetSelectedConditionInfo()
        {
            string condition = conditions[selectedIndex];

            if (condition.StartsWith(MP3PlayerDataStore.BossConditionKey))
            {
                BossCondition bossCondition = new(uuid);

                return LocalizationHelper.GetGUIText(bossCondition.Info);
            }
            else if (condition.StartsWith(MP3PlayerDataStore.BiomeConditionKey))
            {
                BiomeCondition biomeCondition = new(uuid);

                return LocalizationHelper.GetGUIText(biomeCondition.Info);
            }
            else if (condition.StartsWith(MP3PlayerDataStore.LastManStandingKey))
            {
                return LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionLastManStanding");
            }
            else if (condition.StartsWith(MP3PlayerDataStore.OnDeathKey))
            {
                return LocalizationHelper.GetGUIText("MP3PlayerMenu.ConditionOnDeath");
            }

            return null;
        }

        private bool TryGetListCondition(out string choice)
        {
            string condition = conditions[selectedIndex];

            choice = null;

            if (condition.StartsWith(MP3PlayerDataStore.BossConditionKey))
            {
                BossCondition bossCondition = new(uuid)
                {
                    Choice = condition.Split(':')[1]
                };

                choice = bossCondition.GetDataDisplay(bossCondition.Choice);

                return true;
            }
            else if (condition.StartsWith(MP3PlayerDataStore.BiomeConditionKey))
            {
                BiomeCondition biomeCondition = new(uuid)
                {
                    Choice = condition.Split(':')[1]
                };

                choice = biomeCondition.GetDataDisplay(biomeCondition.Choice);

                return true;
            }

            return false;
        }

        public void UpdateBanners()
        {
            if (conditions.Count > 0)
            {
                conditionInfo = GetSelectedConditionInfo();

                string infoFile = Path.Combine(MP3Player.CachePath, $"{uuid}.txt");

                string songName = File.ReadAllLines(infoFile)[0];

                Rectangle drawBox = GetDimensions().ToRectangle();

                Rectangle screenRectangle = new(drawBox.X + 4, drawBox.Y + (lineHeight * 2) + 4, drawBox.Width - 8, lineHeight);

                banners[0] = new TextBanner(songName, screenRectangle, MP3Player.Font);

                if (TryGetListCondition(out string choice))
                {
                    screenRectangle.Y += lineHeight * 2;

                    banners[1] = new TextBanner(choice, screenRectangle, MP3Player.Font);
                }
                else
                {
                    banners[1] = null;
                }
            }
        }
    }
}
