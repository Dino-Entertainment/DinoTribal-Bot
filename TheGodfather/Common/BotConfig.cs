﻿#region USING_DIRECTIVES
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog.Events;
using TheGodfather.Database;
#endregion

namespace TheGodfather.Common
{
    public sealed class BotConfig
    {
        [JsonProperty("db-config")]
        public DatabaseConfig DatabaseConfig { get; private set; }

        [JsonProperty("db_sync_interval")]
        public int DatabaseSyncInterval { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("feed_check_interval")]
        public int FeedCheckInterval { get; private set; }

        [JsonProperty("feed_check_start_delay")]
        public int FeedCheckStartDelay { get; private set; }

        [JsonProperty("key-giphy")]
        public string GiphyKey { get; private set; }

        [JsonProperty("key-goodreads")]
        public string GoodreadsKey { get; private set; }

        [JsonProperty("key-imgur")]
        public string ImgurKey { get; private set; }

        [JsonProperty("locale")]
        public string Locale { get; private set; }

        [JsonProperty("log-level")]
        public LogEventLevel LogLevel { get; private set; }

        [JsonProperty("log-path")]
        public string LogPath { get; private set; }

        [JsonProperty("log-to-file")]
        public bool LogToFile { get; private set; }

        [JsonProperty("key-omdb")]
        public string OMDbKey { get; private set; }

        [JsonProperty("shard-count")]
        public int ShardCount { get; private set; }

        [JsonProperty("key-steam")]
        public string SteamKey { get; private set; }

        [JsonProperty("key-weather")]
        public string WeatherKey { get; private set; }

        [JsonProperty("key-youtube")]
        public string YouTubeKey { get; private set; }

        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("logger-special-rules")]
        public List<SpecialLoggingRule> SpecialLoggerRules { get; private set; }


        [JsonIgnore]
        public static BotConfig Default => new BotConfig {
            DatabaseConfig = DatabaseConfig.Default,
            DatabaseSyncInterval = 600,
            Prefix = "!",
            FeedCheckInterval = 300,
            FeedCheckStartDelay = 30,
            GiphyKey = "<insert GIPHY API key>",
            GoodreadsKey = "<insert Goodreads API key>",
            ImgurKey = "<insert Imgur API key>",
            Locale = "en-US",
            LogLevel = LogEventLevel.Information,
            LogPath = "log.txt",
            LogToFile = false,
            OMDbKey = "<insert OMDb API key>",
            ShardCount = 1,
            SpecialLoggerRules = new List<SpecialLoggingRule>(),
            SteamKey = "<insert Steam API key>",
            Token = "<insert bot token here>",
            WeatherKey = "<insert OpenWeatherMaps API key>",
            YouTubeKey = "<insert YouTube API key>"
        };


        public sealed class SpecialLoggingRule
        {
            [JsonProperty("app")]
            public string Application { get; set; }

            [JsonProperty("level")]
            public LogEventLevel MinLevel { get; set; }
        }
    }
}
