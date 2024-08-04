using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Common.UI.Themes;
using MP3Player.Content.Audio;
using MP3Player.Content.IO;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Core.Audio;
using MP3Player.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MP3Player.Content.UI.MP3PlayerUI.Playback
{
    public class PlaybackWidget : MP3Widget, IDisposable
    {
        public float VolumeFadeMultiplier { get; set; }

        public string Uuid { get; private set; }

        public string Name { get; private set; }

        public string Author { get; private set; }

        public bool Forced { get; private set; }

        private TextBanner nameBanner;

        private TextBanner authorBanner;

        private readonly DynamicMP3AudioTrack audioTrack;

        private const int VisualiserHeight = 48;

        public PlaybackWidget(string uuid, MP3PlayerMainPanel panel, bool forced) : base(panel)
        {
            Uuid = uuid;
            Forced = forced;

            string titlePath = Path.Combine(MP3Player.CachePath, $"{uuid}.txt");

            string[] text = File.ReadAllLines(titlePath);

            Name = text[0];
            Author = text[1];

            string songPath = Path.Combine(MP3Player.CachePath, $"{uuid}.mp3");

            audioTrack = new(new FileStream(songPath, FileMode.Open));

            Toggle();
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            nameBanner ??= new(Name, GetDimensions().ToRectangle(), MP3Player.Font);
            authorBanner ??= new(Author, GetDimensions().ToRectangle(), MP3Player.Font);

            nameBanner.UpdateScrolling();
            authorBanner.UpdateScrolling();

            base.SafeUpdate(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            int sizeY = (int)MP3Player.Font.MeasureString(Name).Y;

            int y = drawBox.Y + 4;

            nameBanner?.Draw(spriteBatch, new(drawBox.X + 4, y), MP3Player.Cyan);

            y += sizeY;

            authorBanner?.Draw(spriteBatch, new(drawBox.X + 4, y), MP3Player.Pink);

            y += sizeY;

            int leftX = drawBox.X + 4;
            int rightX = drawBox.X + drawBox.Width - 8;

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(leftX, y, 4, 12), MP3Player.Pink);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rightX, y, 4, 12), MP3Player.Pink);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(drawBox.X + 4, y + 4, drawBox.Width - 8, 4), MP3Player.Pink);

            int caretX = (int)MathHelper.Lerp(leftX, rightX, audioTrack.Progress);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(caretX, y, 4, 12), MP3Player.Cyan);

            y += 12;

            string time = $"{FormatTimeString(audioTrack.ElapsedTime)} / {FormatTimeString(audioTrack.SongDuration)}";

            spriteBatch.DrawString(MP3Player.Font, time, new(drawBox.X + 4, y), MP3Player.Pink);

            y += sizeY;

            DrawVisualiser(spriteBatch, drawBox.X + 4, y);

            y += VisualiserHeight + 2;

            int symbolX = drawBox.X + 4;

            if (audioTrack.IsPaused)
            {
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(symbolX, y - 2, 4, 16), MP3Player.Pink);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(symbolX + 8, y - 2, 4, 16), MP3Player.Pink);

                symbolX += 20;
            }

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
                spriteBatch.Draw(icon, new Vector2(symbolX, y - 4), MP3Player.Cyan);
            }

            base.Draw(spriteBatch);
        }

        public void UpdateAudioTrack()
        {
            audioTrack.Update();

            if (audioTrack.IsStopped)
            {
                // If the song is currently being forced then it always needs looping.
                if (ModContent.GetInstance<MP3TrackUpdaterSystem>().CurrentlyForcingSong)
                {
                    audioTrack.Reuse();
                    audioTrack.Play();

                    return;
                }

                switch (PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>().PlayMode)
                {
                    case PlayMode.Once:
                        Panel.StopCurrentSong();

                        break;
                    case PlayMode.Loop:
                        audioTrack.Reuse();
                        audioTrack.Play();

                        break;
                    case PlayMode.Autoplay:
                        Panel.StopCurrentSong();

                        string nextSong = GetNextSong(1);

                        Panel.BeginPlayingSong(nextSong);

                        break;
                    case PlayMode.Shuffle:
                        Panel.StopCurrentSong();

                        string randomSong = GetRandomSong();

                        Panel.BeginPlayingSong(randomSong);

                        break;
                }
            }
        }

        private List<Song> GetSortedSongList()
        {
            List<Song> songs = new();

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

            return songs;
        }

        public void SetVolume(float volume)
        {
            MP3TrackUpdaterSystem system = ModContent.GetInstance<MP3TrackUpdaterSystem>();

            // If the song was played manually, no need to fade.
            if (!system.CurrentlyFadingOut && !system.CurrentlyForcingSong)
            {
                VolumeFadeMultiplier = 1;
            }

            float adjustedVolume = volume * VolumeFadeMultiplier;

            audioTrack.SetVariable("Volume", adjustedVolume);
        }

        public override void AcceptInput(ButtonType type)
        {
            bool forcing = ModContent.GetInstance<MP3TrackUpdaterSystem>().CurrentlyForcingSong;

            switch (type)
            {
                // Only allow songs to be changed if it's not being forced.
                case ButtonType.Previous:
                    if (!forcing)
                    {
                        Panel.StopCurrentSong();
                        Panel.BeginPlayingSong(GetNextSong(-1));
                    }
                    break;
                case ButtonType.Next:
                    if (!forcing)
                    {
                        Panel.StopCurrentSong();
                        Panel.BeginPlayingSong(GetNextSong(1));
                    }
                    break;
                case ButtonType.Rewind:
                    Skip(-10);
                    break;
                case ButtonType.PausePlay:
                    Toggle();
                    break;
                case ButtonType.FastForward:
                    Skip(10);
                    break;
            }
        }

        public void Dispose()
        {
            audioTrack.Stop(AudioStopOptions.Immediate);

            Panel.ActiveSong = null;

            audioTrack.Dispose();
        }

        private void DrawVisualiser(SpriteBatch spriteBatch, int x, int y)
        {
            byte[] bytes = audioTrack.BufferToSubmit;

            int blocks = ((int)Width.Pixels - 8) / 4;

            int currentBlock = 0;

            for (int offset = 0; offset < Width.Pixels - 8; offset += 6)
            {
                int positionStart = (int)((float)currentBlock / blocks * bytes.Length);
                int blockLength = bytes.Length / blocks;

                int height = (int)((float)Average(bytes, positionStart, blockLength) / byte.MaxValue * (VisualiserHeight - 4)) + 4;

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x + offset, y + VisualiserHeight / 2 - height / 2, 4, height / 2), MP3Player.Pink);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x + offset, y + VisualiserHeight / 2, 4, height / 2), MP3Player.Pink);

                currentBlock++;
            }
        }

        private byte Average(byte[] array, int start, int length)
        {
            int total = 0;

            for (int i = start; i < start + length; i++)
            {
                total += array[i];
            }

            return (byte)(total / length);
        }

        private void Skip(double seconds) => audioTrack.Skip(seconds);

        private void Toggle()
        {
            if (audioTrack.IsPlaying)
            {
                audioTrack.Pause();
            }
            else if (audioTrack.IsStopped)
            {
                audioTrack.Play();
            }
            else if (audioTrack.IsPaused)
            {
                audioTrack.Resume();
            }
        }

        private string FormatTimeString(TimeSpan time)
        {
            string timeString = "";

            if (time.Hours > 0)
            {
                timeString += $"{time.Hours}:";
            }

            timeString += $"{time.Minutes}:";

            string seconds = time.Seconds < 10 ? $"0{time.Seconds}" : $"{time.Seconds}";

            timeString += seconds;

            return timeString;
        }

        private string GetNextSong(int step)
        {
            List<Song> songs = GetSortedSongList();

            int index = songs.FindIndex(song => song.Uuid == Uuid);

            index += step;

            // Loop round the list.
            if (index < 0)
            {
                index = songs.Count - 1;
            }
            if (index > songs.Count - 1)
            {
                index = 0;
            }

            return songs[index].Uuid;
        }

        private string GetRandomSong()
        {
            List<Song> songs = GetSortedSongList();

            if (songs.Count == 1)
            {
                return songs[0].Uuid;
            }

            int randomIndex = Main.rand.Next(songs.Count);

            // Make sure the new song isn't this song.
            while (randomIndex == songs.FindIndex(song => song.Uuid == Uuid))
            {
                randomIndex = Main.rand.Next(songs.Count);
            }

            return songs[randomIndex].Uuid;
        }
    }
}
