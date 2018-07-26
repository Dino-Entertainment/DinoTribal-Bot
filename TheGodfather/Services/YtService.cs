﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TheGodfather.Modules.Music.Common;
using YoutubeExplode;
#endregion

namespace TheGodfather.Services
{
    public class YtService : TheGodfatherHttpService
    {
        private static readonly string _apiUrl = "https://www.googleapis.com/youtube/v3";
        private static readonly string _ytUrl = "https://www.youtube.com";
        private static readonly Regex _sanitizeRegex = new Regex("[^a-z0-9-]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _ytRegex = new Regex(@"\.(youtu(be)?)\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly YoutubeClient ytExplode;
        private readonly YouTubeService yt;
        private readonly string key;


        public YtService(string key)
        {
            this.ytExplode = new YoutubeClient();
            this.key = key;
            this.yt = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = key,
                ApplicationName = TheGodfather.ApplicationName
            });
        }


        public static string GetRssUrlForChannel(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || _sanitizeRegex.IsMatch(id))
                throw new ArgumentException("YouTube channel ID is either missing or invalid!", "id");

            return $"{_ytUrl}/feeds/videos.xml?channel_id={id}";
        }


        public async Task<string> ExtractChannelIdAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL missing!", "url");

            if (YoutubeClient.TryParseChannelId(url, out string id))
                return id;

            string[] split = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (!split.Any(s => _ytRegex.IsMatch(s)))
                return null;

            id = split.Last();
            try {
                string getUrl = $"{_apiUrl}/channels?key={this.key}&forUsername={id}&part=id";
                string response = await _http.GetStringAsync(getUrl).ConfigureAwait(false);
                var items = JObject.Parse(response)["items"].ToObject<List<Dictionary<string, string>>>();
                if (items != null && items.Any())
                    return items.First()["id"];
            } catch {

            }

            return null;
        }

        public async Task<string> GetFirstVideoResultAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", "query");

            var res = await SearchAsync(query, 1, "video");
            if (!res.Any())
                return null;

            return $"{_ytUrl}/watch?v={res.First().Id.VideoId}";
        }

        public async Task<IReadOnlyList<Page>> GetPaginatedResultsAsync(string query, int amount = 1, string type = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", "query");

            if (amount < 1 || amount > 20)
                throw new ArgumentException("Result amount out of range (max 20)", "amount");

            IReadOnlyList<SearchResult> results = await SearchAsync(query, amount, type);
            if (results == null || !results.Any())
                return null;

            var pages = new List<Page>();
            foreach (var res in results.Take(10)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = res.Snippet.Title,
                    Description = Formatter.Italic(string.IsNullOrWhiteSpace(res.Snippet.Description) ? "No description provided" : res.Snippet.Description),
                    Color = DiscordColor.Red,
                    ThumbnailUrl = res.Snippet.Thumbnails.Default__.Url
                };

                emb.AddField("Channel", res.Snippet.ChannelTitle, inline: true);
                emb.AddField("Published at", $"{res.Snippet.PublishedAt ?? DateTime.Now}", inline: true);

                switch (res.Id.Kind) {
                    case "youtube#video":
                        emb.WithUrl($"{_ytUrl}/watch?v={res.Id.VideoId}");
                        break;
                    case "youtube#channel":
                        emb.WithDescription($"{_ytUrl}/channel/{res.Id.ChannelId}");
                        break;
                    case "youtube#playlist":
                        emb.WithDescription($"{_ytUrl}/playlist?list={res.Id.PlaylistId}");
                        break;
                }

                pages.Add(new Page() {
                    Embed = emb.Build()
                });
            }

            return pages.AsReadOnly();
        }

        public async Task<SongInfo> GetSongInfoAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL missing!", "url");

            return await GetSongInfoViaYtExplodeAsync(url) ?? await GetSongInfoViaYtDlAsync(url);
        }


        private async Task<SongInfo> GetSongInfoViaYtExplodeAsync(string url)
        {
            if (!YoutubeClient.TryParseVideoId(url, out string id))
                return null;

            var video = await this.ytExplode.GetVideoAsync(id).ConfigureAwait(false);
            if (video == null)
                return null;

            var streamInfo = await this.ytExplode.GetVideoMediaStreamInfosAsync(video.Id).ConfigureAwait(false);
            var stream = streamInfo.Audio
                .OrderByDescending(x => x.Bitrate)
                .FirstOrDefault();
            if (stream == null)
                return null;

            return new SongInfo {
                Provider = "YouTube",
                Query = $"{_ytUrl}/watch?v={video.Id}",
                Thumbnail = video.Thumbnails.MediumResUrl,
                TotalTime = video.Duration,
                Uri = stream.Url,
                VideoId = video.Id,
                Title = video.Title,
            };
        }

        private async Task<SongInfo> GetSongInfoViaYtDlAsync(string url)
        {
            string[] data = null;
            try {
                var ytdlinfo = new ProcessStartInfo() {
                    FileName = "Resources/youtube-dl",
                    Arguments = $"-4 --geo-bypass -f bestaudio -e --get-url --get-id --get-thumbnail --get-duration --no-check-certificate --default-search \"ytsearch:\" \"{url}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                using (var process = new Process() { StartInfo = ytdlinfo }) {
                    process.Start();
                    string str = await process.StandardOutput.ReadToEndAsync();
                    string err = await process.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(err))
                        throw new OperationCanceledException();
                    if (!string.IsNullOrWhiteSpace(str))
                        data = str.Split('\n');
                }

                if (data == null || data.Length < 6)
                    return null;

                if (!TimeSpan.TryParseExact(data[4], new[] { "ss", "m\\:ss", "mm\\:ss", "h\\:mm\\:ss", "hh\\:mm\\:ss", "hhh\\:mm\\:ss" }, CultureInfo.InvariantCulture, out var time))
                    time = TimeSpan.FromHours(24);

                return new SongInfo() {
                    Title = data[0],
                    VideoId = data[1],
                    Uri = data[2],
                    Thumbnail = data[3],
                    TotalTime = time,
                    Provider = "YouTube",
                    Query = "https://youtube.com/watch?v=" + data[1],
                };
            } catch (OperationCanceledException) {
                throw;
            } catch {
                return null;
            }
        }

        private async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int amount, string type = null)
        {
            SearchResource.ListRequest request = this.yt.Search.List("snippet");
            request.Q = query;
            request.MaxResults = amount;
            if (!string.IsNullOrWhiteSpace(type))
                request.Type = type;

            SearchListResponse response = await request.ExecuteAsync().ConfigureAwait(false);

            return response.Items.ToList().AsReadOnly();
        }
    }
}