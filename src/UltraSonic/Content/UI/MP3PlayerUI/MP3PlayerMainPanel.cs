using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Common.UI;
using MP3Player.Common.UI.Abstract;
using MP3Player.Common.UI.Themes;
using MP3Player.Content.IO;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Content.UI.MP3PlayerUI.Playback;
using MP3Player.Content.UI.MP3PlayerUI.Selection;
using MP3Player.Content.UI.NowPlayingUI;
using MP3Player.Core.Audio;
using MP3Player.Core.IO;
using MP3Player.Core.UI;
using ReLogic.OS;
using System;
using Terraria;

namespace MP3Player.Content.UI.MP3PlayerUI
{
    public class MP3PlayerMainPanel : SmartUIElement
    {
        public PlaybackWidget ActiveSong { get; set; }

        private Rectangle LargeWidgetBox => new((int)GetDimensions().X + 8, (int)GetDimensions().Y + 8, (int)Width.Pixels - 16, 144);

        private YoutubeLinkField linkField;

        private VolumeKnob volumeKnob;

        private MP3Widget currentScreenElement;

        public override void OnInitialize()
        {
            SetupMusicButtons(out int width, out int top);
            int height = 40;

            linkField = new();
            linkField.Width.Set(Width.Pixels - 16, 0);
            linkField.Height.Set(32, 0);
            linkField.Left.Set(8, 0);
            linkField.Top.Set(Height.Pixels - 40, 0);

            IconButton clearBox = new("Stop", "MP3PlayerMenu.StopSong");
            clearBox.Width.Set(width, 0);
            clearBox.Height.Set(height, 0);
            clearBox.Left.Set(8, 0);
            clearBox.Top.Set(linkField.Top.Pixels - height - 8, 0);
            clearBox.OnLeftClick += (evt, args) =>
            {
                // Only allow stopping if song isn't being forced.
                if (ActiveSong != null && !ActiveSong.Forced)
                {
                    StopCurrentSong();

                    ChangeWidget(new SongSelectionWidget(this), true);
                }
            };

            IconButton addSong = new("Add", "MP3PlayerMenu.AddSong");
            addSong.Width.Set(width, 0);
            addSong.Height.Set(height, 0);
            addSong.Left.Set(clearBox.Left.Pixels + clearBox.Width.Pixels + 8, 0);
            addSong.Top.Set(clearBox.Top.Pixels, 0);
            addSong.OnLeftClick += (evt, args) =>
            {
                // If the text field is empty, the "+" button will instead paste the clipboard into the box (most likely the link).
                if (string.IsNullOrEmpty(linkField.CurrentValue))
                {
                    linkField.CurrentValue = Platform.Get<IClipboard>().Value;
                    return;
                }

                AsyncMP3Downloader.DownloadFromUrl(
                    linkField.CurrentValue.Trim(),
                    (progress, progressInfo) =>
                    {
                        if (progress > 0.2f && progress < 1)
                        {
                            linkField.BackgroundColor = MP3Player.Pink;
                        }

                        Main.NewText($"{Math.Round(progress * 100, 0)}% - {progressInfo}", progress < 1 ? MP3Player.Pink : MP3Player.Cyan);
                    },
                    uuid =>
                    {
                        linkField.BackgroundColor = MP3Player.Cyan;

                        Main.QueueMainThreadAction(() =>
                        {
                            if (currentScreenElement is SongSelectionWidget widget)
                            {
                                widget.RepopulateSongList();
                            }
                        });
                    },
                    message =>
                    {
                        Main.NewText(message, Color.Red);

                        linkField.BackgroundColor = Color.Red;
                    }
                );

                linkField.CurrentValue = "";
            };

            IconButton shuffle = new("Shuffle", "MP3PlayerMenu.Shuffle");
            shuffle.Width.Set(width, 0);
            shuffle.Height.Set(clearBox.Top.Pixels - top - height - 16, 0);
            shuffle.Left.Set(8, 0);
            shuffle.Top.Set(top + height + 8, 0);
            shuffle.OnLeftClick += (evt, args) =>
            {
                MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

                store.PlayMode++;

                if (store.PlayMode > PlayMode.Loop)
                {
                    store.PlayMode = PlayMode.Once;
                }

                store.ForceSave();
            };

            IconButton menu = new("Menu", "MP3PlayerMenu.Menu");
            menu.Width.Set(width, 0);
            menu.Height.Set(clearBox.Top.Pixels - top - height - 16, 0);
            menu.Left.Set(width + 16, 0);
            menu.Top.Set(top + height + 8, 0);
            menu.OnLeftClick += (evt, args) =>
            {
                if (ActiveSong != null && currentScreenElement is SongSelectionWidget)
                {
                    ChangeWidget(ActiveSong, false);
                }
                else
                {
                    ChangeWidget(new SongSelectionWidget(this), true);
                }
            };

            int volumeSize = 128;

            volumeKnob = new();
            volumeKnob.Width.Set(volumeSize, 0);
            volumeKnob.Height.Set(volumeSize, 0);
            volumeKnob.Left.Set(Width.Pixels - volumeSize - 24, 0);
            volumeKnob.Top.Set(linkField.Top.Pixels - volumeSize - 8, 0);

            Append(linkField);
            Append(clearBox);
            Append(addSong);
            Append(shuffle);
            Append(menu);
            Append(volumeKnob);

            ChangeWidget(new SongSelectionWidget(this), true);

            base.OnInitialize();
        }

        private void SetupMusicButtons(out int width, out int top)
        {
            int left = 8;
            top = LargeWidgetBox.Height + 16;

            int buttonCount = 5;

            int totalPadding = (buttonCount + 1) * 8;

            width = ((int)Width.Pixels - totalPadding) / buttonCount;
            int height = 40;

            IconButton prevButton = new("PreviousSong", "MP3PlayerMenu.PreviousSong");
            prevButton.Width.Set(width, 0);
            prevButton.Height.Set(height, 0);
            prevButton.Left.Set(left, 0);
            prevButton.Top.Set(top, 0);
            prevButton.OnLeftClick += (evt, args) =>
            {
                currentScreenElement?.AcceptInput(ButtonType.Previous);
            };

            left += width + 8;

            IconButton skipBack = new("Rewind", "MP3PlayerMenu.SkipBack");
            skipBack.Width.Set(width, 0);
            skipBack.Height.Set(height, 0);
            skipBack.Left.Set(left, 0);
            skipBack.Top.Set(top, 0);
            skipBack.OnLeftClick += (evt, args) =>
            {
                currentScreenElement?.AcceptInput(ButtonType.Rewind);
            };

            left += width + 8;

            IconButton play = new("PausePlay", "MP3PlayerMenu.PausePlay");
            play.Width.Set(width, 0);
            play.Height.Set(height, 0);
            play.Left.Set(left, 0);
            play.Top.Set(top, 0);
            play.OnLeftClick += (evt, args) =>
            {
                currentScreenElement?.AcceptInput(ButtonType.PausePlay);
            };

            left += width + 8;

            IconButton skipAhead = new("Forward", "MP3PlayerMenu.SkipAhead");
            skipAhead.Width.Set(width, 0);
            skipAhead.Height.Set(height, 0);
            skipAhead.Left.Set(left, 0);
            skipAhead.Top.Set(top, 0);
            skipAhead.OnLeftClick += (evt, args) =>
            {
                currentScreenElement?.AcceptInput(ButtonType.FastForward);
            };

            left += width + 8;

            IconButton nextButton = new("NextSong", "MP3PlayerMenu.NextSong");
            nextButton.Width.Set(width, 0);
            nextButton.Height.Set(height, 0);
            nextButton.Left.Set(left, 0);
            nextButton.Top.Set(top, 0);
            nextButton.OnLeftClick += (evt, args) =>
            {
                currentScreenElement?.AcceptInput(ButtonType.Next);
            };

            Append(prevButton);
            Append(skipBack);
            Append(play);
            Append(skipAhead);
            Append(nextButton);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Recalculate();

            Rectangle drawBox = GetDimensions().ToRectangle();

            Texture2D background = ThemeSystem.GetIcon("Background");

            spriteBatch.Draw(background, new Vector2(drawBox.X, drawBox.Y), Color.White);

            UIHelper.DrawBox(spriteBatch, LargeWidgetBox, Color.Black * 0.75f);

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            base.Draw(spriteBatch);
        }

        public void BeginPlayingSong(string uuid, bool forced = false)
        {
            if (ActiveSong != null)
            {
                RemoveChild(ActiveSong);
                ActiveSong.Dispose();
            }

            PlaybackWidget song = new(uuid, this, forced);

            ChangeWidget(song, true);

            ActiveSong = song;

            MP3PlayerUILoader.GetUIState<NowPlayingState>().NotifyActiveSong(ActiveSong);
        }

        public void StopCurrentSong()
        {
            if (ActiveSong != null)
            {
                RemoveChild(ActiveSong);
                ActiveSong.Dispose();
            }

            if (currentScreenElement is PlaybackWidget)
            {
                ChangeWidget(new SongSelectionWidget(this), true);
            }

            ActiveSong = null;
        }

        public void UpdateActiveSong()
        {
            ActiveSong?.UpdateAudioTrack();

            if (ActiveSong != null)
            {
                ActiveSong.SetVolume(volumeKnob.Volume);
            }
        }

        public void ChangeWidget(MP3Widget newWidget, bool reinitialise)
        {
            currentScreenElement?.RemoveAllChildren();
            currentScreenElement?.Remove();

            if (reinitialise)
            {
                newWidget.Width.Set(LargeWidgetBox.Width, 0);
                newWidget.Height.Set(LargeWidgetBox.Height, 0);
                newWidget.Left.Set(8, 0);
                newWidget.Top.Set(8, 0);

                newWidget.OnInitialize();
            }

            currentScreenElement = newWidget;

            Append(currentScreenElement);

            if (currentScreenElement is ITextBannerListWidget widget)
            {
                widget.UpdateBanners();
            }
        }
    }
}
