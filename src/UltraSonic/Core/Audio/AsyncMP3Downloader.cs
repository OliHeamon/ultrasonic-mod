using MP3Player.Localization;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MP3Player.Core.Audio
{
    public static class AsyncMP3Downloader
    {
        public static bool IsBusy { get; private set; }

        private static bool internetRequired;

        public delegate void ProgressUpdateDelegate(float progress, string progressInfo);

        public delegate void CompletionDelegate(string title);

        public delegate void FailureDelegate(string message);

        public static bool DownloadFromUrl(string url, ProgressUpdateDelegate onProgressUpdate, CompletionDelegate onComplete, FailureDelegate onFail)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                internetRequired = true;

                CancellationTokenSource source = new();
                CancellationToken token = source.Token;

                Task.Run(() => DownloadFromUrlAsync(url, onProgressUpdate, onComplete, onFail, token));

                // This will cancel all network tasks and reset state if it could not finish the downloads in the given time frame.
                Task.Run(() =>
                {
                    double elapsedMs = 0;

                    int incrementMs = 100;
                    int limitSeconds = 90;

                    while (IsBusy && internetRequired)
                    {
                        if (!internetRequired)
                        {
                            return;
                        }

                        Thread.Sleep(incrementMs);
                        elapsedMs += incrementMs;

                        if (elapsedMs / 1000 > limitSeconds)
                        {
                            source.Cancel();

                            onFail.Invoke(LocalizationHelper.GetGUIText("AsyncMP3Downloader.ConnectionTimeout"));

                            internetRequired = false;
                            IsBusy = false;
                        }
                    }
                });
            }

            // If a download is already occurring, notify the caller and do not start another one.
            return !IsBusy;
        }

        private static async Task DownloadFromUrlAsync(string url, ProgressUpdateDelegate onProgressUpdate, CompletionDelegate onComplete, FailureDelegate onFail, CancellationToken cancellationToken)
        {
            string path = MP3Player.CachePath;
            string uuid = Guid.NewGuid().ToString();

            try
            {
                YoutubeClient youtube = new();

                onProgressUpdate.Invoke(0.2f, LocalizationHelper.GetGUIText("AsyncMP3Downloader.GetManifest"));

                StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(url, cancellationToken);

                IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                onProgressUpdate.Invoke(0.4f, LocalizationHelper.GetGUIText("AsyncMP3Downloader.GetTitle"));

                (string, string) videoInfo = await GetTitle(youtube, url);

                string songFile = Path.Combine(path, $"{uuid}.mp4");
                string titleFile = Path.Combine(path, $"{uuid}.txt");

                onProgressUpdate.Invoke(0.6f, LocalizationHelper.GetGUIText("AsyncMP3Downloader.GetAudio"));

                await youtube.Videos.Streams.DownloadAsync(streamInfo, songFile, cancellationToken: cancellationToken);

                internetRequired = false;

                onProgressUpdate.Invoke(0.8f, LocalizationHelper.GetGUIText("AsyncMP3Downloader.ConvertFile"));

                ConvertMp4ToMp3(path, uuid);

                File.WriteAllText(titleFile, $"{videoInfo.Item1}{Environment.NewLine}{videoInfo.Item2}");

                onComplete.Invoke(uuid);
                onProgressUpdate.Invoke(1, LocalizationHelper.GetGUIText("AsyncMP3Downloader.Complete"));

                IsBusy = false;
            }
            catch (ArgumentException)
            {
                onFail.Invoke(LocalizationHelper.GetGUIText("AsyncMP3Downloader.InvalidYoutubeUrl"));

                IsBusy = false;
            }
            catch (FileNotFoundException)
            {
                onFail.Invoke(LocalizationHelper.GetGUIText("AsyncMP3Downloader.FFmpegError"));

                File.Delete($"{Path.Combine(path, $"{uuid}.mp4")}");
                File.Delete($"{Path.Combine(path, $"{uuid}.mp3")}");

                IsBusy = false;
            }
        }

        private static void ConvertMp4ToMp3(string path, string uuid)
        {
            string args = $"-i \"{Path.Combine(path, $"{uuid}.mp4")}\" -vn -f mp3 -ab 320k \"{Path.Combine(path, $"{uuid}.mp3")}\"";

            Process process = FFmpeg.Run(path, args);

            // Wait until ffmpeg conversion is finished before deleting the MP4 file.
            while (!process.HasExited)
            {

            }

            // If FFmpeg encounters an error then the file is malformatted, so throw exception.
            if (process.ExitCode != 0)
            {
                throw new FileNotFoundException("Malformatted MP3.");
            }

            File.Delete($"{Path.Combine(path, $"{uuid}.mp4")}");
        }

        private static async Task<(string, string)> GetTitle(YoutubeClient youtube, string url)
        {
            string id = GetYouTubeVideoIdFromUrl(url);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Invalid link!");
            }

            Video video = await youtube.Videos.GetAsync(id);

            return (video.Title, video.Author.ChannelTitle);
        }

        // This entire thing is from SO.
        private static string GetYouTubeVideoIdFromUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                try
                {
                    uri = new UriBuilder("http", url).Uri;
                }
                catch
                {
                    return "";
                }
            }

            string host = uri.Host;

            string[] youTubeHosts = { "www.youtube.com", "youtube.com", "youtu.be", "www.youtu.be" };

            if (!youTubeHosts.Contains(host))
            {
                return "";
            }

            var query = HttpUtility.ParseQueryString(uri.Query);

            if (query.AllKeys.Contains("v"))
            {
                return Regex.Match(query["v"], @"^[a-zA-Z0-9_-]{11}$").Value;
            }
            else if (query.AllKeys.Contains("u"))
            {
                // some urls have something like "u=/watch?v=AAAAAAAAA16"
                return Regex.Match(query["u"], @"/watch\?v=([a-zA-Z0-9_-]{11})").Groups[1].Value;
            }
            else
            {
                // remove a trailing forward space
                var last = uri.Segments.Last().Replace("/", "");

                if (Regex.IsMatch(last, @"^v=[a-zA-Z0-9_-]{11}$"))
                {
                    return last.Replace("v=", "");
                }

                string[] segments = uri.Segments;

                if (segments.Length > 2 && segments[segments.Length - 2] != "v/" && segments[segments.Length - 2] != "watch/")
                {
                    return "";
                }

                return Regex.Match(last, @"^[a-zA-Z0-9_-]{11}$").Value;
            }
        }
    }
}
