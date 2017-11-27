﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Helpers
{
    public sealed class GuildConfig
    {
        [JsonProperty("Prefix")]
        public string Prefix { get; internal set; }

        [JsonProperty("WelcomeChannelId")]
        public ulong? WelcomeChannelId { get; internal set; }

        [JsonProperty("LeaveChannelId")]
        public ulong? LeaveChannelId { get; internal set; }

        [JsonProperty("Triggers")]
        public ConcurrentDictionary<string, string> Triggers { get; internal set; }

        [JsonProperty("Filters")]
        public HashSet<Regex> Filters { get; internal set; }

        [JsonProperty("Reactions")]
        public ConcurrentDictionary<string, string> Reactions { get; internal set; }
    }
}
