using Microsoft.Xna.Framework.Audio;
using System;
using System.IO;
using Terraria.Audio;
using XPT.Core.Audio.MP3Sharp;

namespace MP3Player.Core.Audio
{
    public class DynamicMP3AudioTrack : ASoundEffectBasedAudioTrack
    {
        private readonly Stream stream;

        private readonly MP3Stream mp3Stream;

        private readonly long frequency;

        // This is derived from ffmpeg encoding all incoming files at 320kbps (40kHz).
        private const int Bitrate = 40000;

        public byte[] BufferToSubmit => _bufferToSubmit ?? Array.Empty<byte>();

        public float Progress => (float)mp3Stream.Position / mp3Stream.Length;

        public TimeSpan ElapsedTime => SongDuration * Progress;

        public TimeSpan SongDuration => TimeSpan.FromSeconds(mp3Stream.Length / Bitrate);

        public DynamicMP3AudioTrack(Stream stream)
        {
            this.stream = stream;

            MP3Stream mp3Stream = new(stream);

            frequency = mp3Stream.Frequency;

            this.mp3Stream = mp3Stream;

            CreateSoundEffect((int)frequency, AudioChannels.Stereo);
        }

        public override void Reuse()
        {
            mp3Stream.Position = 0L;
        }

        public override void Dispose()
        {
            _soundEffectInstance.Dispose();
            mp3Stream.Dispose();
            stream.Dispose();
        }

        protected override void ReadAheadPutAChunkIntoTheBuffer()
        {
            byte[] bufferToSubmit = _bufferToSubmit;
            if (mp3Stream.Read(bufferToSubmit, 0, bufferToSubmit.Length) < 1)
                Stop(AudioStopOptions.Immediate);
            else
                _soundEffectInstance.SubmitBuffer(_bufferToSubmit);
        }

        public void Skip(double seconds)
        {
            long jumpInBytes = (long)(seconds * Bitrate);

            mp3Stream.Position = Math.Clamp(mp3Stream.Position + jumpInBytes, 0L, mp3Stream.Length);
        }
    }
}
