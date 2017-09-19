﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
/*
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
*/
#endregion


namespace TheGodfatherBot.Modules.Search
{
    [Group("youtube", CanInvokeWithoutSubcommand = true)]
    [Description("Youtube search commands.")]
    [Aliases("y", "yt")]
    public class CommandsYoutube
    {
        #region PRIVATE_FIELDS
        private YouTubeService _yt = null;
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Search query.")] string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query missing.");

            var results = await GetYoutubeResults(query, 5);

            await ctx.RespondAsync($"Search results for ***{query}***", embed: EmbedYouTubeResults(results));
        }


        #region COMMAND_YOUTUBE_SEARCH
        [Command("search")]
        [Description("Advanced youtube search.")]
        [Aliases("s", "find", "query")]
        public async Task SearchYouTubeAdvanced(CommandContext ctx,
                                               [Description("Ammount of results.")] int ammount = 5,
                                               [Description("Search query.")] string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query missing.");

            var results = await GetYoutubeResults(query, ammount);

            await ctx.RespondAsync($"Search results for ***{query}***", embed: EmbedYouTubeResults(results));
        }
        #endregion

        #region COMMAND_YOUTUBE_SEARCHVIDEO
        [Command("searchv")]
        [Description("Advanced youtube search for videos only.")]
        [Aliases("sv", "findv", "queryv")]
        public async Task SearchYouTubeVideo(CommandContext ctx,
                                            [Description("Search query.")] string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query missing.");

            var results = GetYoutubeResults(query, 10).Result.Where(r => r.Id.Kind == "youtube#video").ToList();

            await ctx.RespondAsync($"Search results for ***{query}***", embed: EmbedYouTubeResults(results));
        }
        #endregion

        #region COMMAND_YOUTUBE_SEARCHCHANNEL
        [Command("searchc")]
        [Description("Advanced youtube search for channels only.")]
        [Aliases("sc", "findc", "queryc")]
        public async Task SearchYouTubeChannel(CommandContext ctx,
                                              [Description("Search query.")] string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query missing.");

            var results = GetYoutubeResults(query, 10).Result.Where(r => r.Id.Kind == "youtube#channel").ToList();

            await ctx.RespondAsync($"Search results for ***{query}***", embed: EmbedYouTubeResults(results));
        }
        #endregion

        #region COMMAND_YOUTUBE_SEARCHPLAYLIST
        [Command("searchp")]
        [Description("Advanced youtube search for playlists only.")]
        [Aliases("sp", "findp", "queryp")]
        public async Task SearchYouTubePlaylist(CommandContext ctx,
                                               [Description("Search query.")] string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query missing.");

            var results = GetYoutubeResults(query, 10).Result.Where(r => r.Id.Kind == "youtube#playlist").ToList();

            await ctx.RespondAsync($"Search results for ***{query}***", embed: EmbedYouTubeResults(results));
        }
        #endregion


        #region HELPER_FUNCTIONS
        private /*async Task*/ void SetupYouTubeService()
        {
            /*
            UserCredential credential;
            using (var stream = new FileStream("Resources/yt_secret.json", FileMode.Open, FileAccess.Read)) {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user", CancellationToken.None, new FileDataStore("Books.ListMyLibrary")
                );
            }
            */

            _yt = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = TheGodfather.GetToken("Resources/youtube.txt"),
                ApplicationName = "TheGodfather"
                // HttpClientInitializer = credential
            });
        }

        private async Task<List<SearchResult>> GetYoutubeResults(string query, int ammount)
        {
            if (_yt == null)
                /*await*/
                SetupYouTubeService();

            var searchListRequest = _yt.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = ammount;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<SearchResult> videos = new List<SearchResult>();
            videos.AddRange(searchListResponse.Items);

            return videos;
        }

        private DiscordEmbed EmbedYouTubeResults(List<SearchResult> results)
        {
            if (results == null || results.Count == 0)
                return new DiscordEmbedBuilder() { Description = "No results...", Color = DiscordColor.Red };

            var em = new DiscordEmbedBuilder() {
                Color = DiscordColor.Red
            };
            foreach (var r in results) {
                switch (r.Id.Kind) {
                    case "youtube#video":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/watch?v=" + r.Id.VideoId);
                        break;

                    case "youtube#channel":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/channel/" + r.Id.ChannelId);
                        break;

                    case "youtube#playlist":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/playlist?list=" + r.Id.PlaylistId);
                        break;
                }
            }

            return em;
        }
        #endregion
    }
}
