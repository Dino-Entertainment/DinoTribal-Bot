﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Entities;

using DSharpPlus;
#endregion

namespace TheGodfather.Services
{
    public static class MemeGenService
    {
        private static readonly ImmutableDictionary<char, string> _map = new Dictionary<char, string>() {
            {'?', "~q"},
            {'%', "~p"},
            {'#', "~h"},
            {'/', "~s"},
            {' ', "-"},
            {'-', "--"},
            {'_', "__"},
            {'"', "''"}
        }.ToImmutableDictionary();


        public static string GetMemeGenerateUrl(string template, string topText, string bottomText)
            => $"http://memegen.link/{template}/{Replace(topText)}/{Replace(bottomText)}.jpg";

        public static async Task<IReadOnlyList<string>> GetMemeTemplatesAsync()
        {
            try {
                var handler = new HttpClientHandler {
                    AllowAutoRedirect = false
                };
                using (var hc = new HttpClient(handler)) {
                    var json = await hc.GetStringAsync("https://memegen.link/api/templates/")
                        .ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return data.Keys.OrderBy(s => s).ToList().AsReadOnly();
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
            }

            return null;
        }

        private static string Replace(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input) {
                if (_map.TryGetValue(c, out var tmp))
                    sb.Append(tmp);
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
