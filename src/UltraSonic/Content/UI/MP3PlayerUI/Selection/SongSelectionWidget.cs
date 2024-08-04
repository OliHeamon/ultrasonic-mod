using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Common.UI.Themes;
using MP3Player.Content.Audio;
using MP3Player.Content.IO;
using MP3Player.Content.UI.MP3PlayerUI.Conditions;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Core.IO;
using MP3Player.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MP3Player.Content.UI.MP3PlayerUI.Selection
{
    public class SongSelectionWidget : MP3Widget, ITextBannerListWidget
    {
        private readonly List<Song> songs;

        private readonly TextBannerList bannerList;

        public SongSelectionWidget(MP3PlayerMainPanel panel) : base(panel)
        {
            songs = new();

            bannerList = new TextBannerList();

            RepopulateSongList();
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
                case ButtonType.PausePlay:

                    if (songs.Count > 0)
                    {
                        if (Panel.ActiveSong != null && songs[bannerList.SelectedIndex].Uuid == Panel.ActiveSong.Uuid)
                        {
                            Panel.ChangeWidget(Panel.ActiveSong, false);
                        }
                        else if (!ModContent.GetInstance<MP3TrackUpdaterSystem>().CurrentlyForcingSong)
                        {
                            Panel.BeginPlayingSong(songs[bannerList.SelectedIndex].Uuid);
                        }
                    }

                    break;
                case ButtonType.Previous:

                    if (songs.Count > 0)
                    {
                        string uuid = songs[bannerList.SelectedIndex].Uuid;

                        if (Panel.ActiveSong != null && uuid == Panel.ActiveSong.Uuid)
                        {
                            Panel.StopCurrentSong();
                        }

                        string songFile = Path.Combine(MP3Player.CachePath, $"{uuid}.mp3");
                        string titleFile = Path.Combine(MP3Player.CachePath, $"{uuid}.txt");

                        File.Delete(songFile);
                        File.Delete(titleFile);

                        PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>().DeleteSongDataByUuid(uuid);

                        RepopulateSongList();
                    }

                    break;
                case ButtonType.Next:

                    if (songs.Count > 0)
                    {
                        string uuid = songs[bannerList.SelectedIndex].Uuid;

                        Panel.ChangeWidget(new ConditionSelectionWidget(Panel, uuid), true);
                    }

                    break;
            }
        }

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
            DrawPlayModeIcon(spriteBatch, drawBox);

            base.Draw(spriteBatch);
        }

        public void RepopulateSongList()
        {
            songs.Clear();

            foreach (string file in Directory.GetFiles(MP3Player.CachePath))
            {
                if (Path.GetExtension(file) == ".txt")
                {
                    string uuid = Path.GetFileNameWithoutExtension(file);

                    string title = File.ReadAllLines(file)[0];

                    songs.Add(new Song(title, uuid));
                }
            }

            songs.Sort((a, b) => a.Title.CompareTo(b.Title));

            List<string> elements = songs.Select(song => song.Title).ToList();

            bannerList.SetElements(elements, GetDimensions().ToRectangle());
        }

        public void UpdateBanners() => bannerList.UpdateBanners(GetDimensions().ToRectangle());

        private void DrawTitle(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            string title = LocalizationHelper.GetGUIText("MP3PlayerMenu.SongList");

            spriteBatch.DrawString(MP3Player.Font, title, new(drawBox.X + 4, drawBox.Y + 4), MP3Player.Cyan);
        }

        private void DrawButtonIndicators(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Rectangle deleteVerticalLine = new(drawBox.X + 22, drawBox.Y + drawBox.Height - 12, 4, 12);
            Rectangle deleteHorizontalLine = new(drawBox.X + 22, drawBox.Y + drawBox.Height - 16, 16, 4);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, deleteVerticalLine, MP3Player.Cyan);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, deleteHorizontalLine, MP3Player.Cyan);

            string deleteText = LocalizationHelper.GetGUIText("MP3PlayerMenu.DeleteSong");

            spriteBatch.DrawString(MP3Player.Font, deleteText, new(drawBox.X + 44, drawBox.Y + drawBox.Height - 24), MP3Player.Cyan);

            Rectangle conditionVerticalLine = new(drawBox.X + drawBox.Width - 26, drawBox.Y + drawBox.Height - 12, 4, 12);
            Rectangle conditionHorizontalLine = new(drawBox.X + drawBox.Width - 38, drawBox.Y + drawBox.Height - 16, 16, 4);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, conditionVerticalLine, MP3Player.Cyan);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, conditionHorizontalLine, MP3Player.Cyan);

            string conditionText = LocalizationHelper.GetGUIText("MP3PlayerMenu.AddCondition");

            int width = (int)MP3Player.Font.MeasureString(conditionText).X;

            spriteBatch.DrawString(MP3Player.Font, conditionText, new(conditionHorizontalLine.X - width - 4, drawBox.Y + drawBox.Height - 24), MP3Player.Cyan);
        }

        private void DrawPlayModeIcon(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Texture2D icon = null;

            switch (PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>().PlayMode)
            {
                case PlayMode.Shuffle:
                    icon = ThemeSystem.GetIcon("SmallShuffle");
                    break;
                case PlayMode.Loop:
                    icon = ThemeSystem.GetIcon("SmallLoop");
                    break;
                case PlayMode.Autoplay:
                    icon = ThemeSystem.GetIcon("SmallNextSong");
                    break;
            }

            if (icon != null)
            {
                spriteBatch.Draw(icon, new Vector2(drawBox.X + drawBox.Width - icon.Width - 4, drawBox.Y + 4), MP3Player.Cyan);
            }
        }
    }
}
