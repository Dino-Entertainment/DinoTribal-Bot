﻿using System.Net;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public sealed class UrbanDictService : TheGodfatherHttpService
{
    public override bool IsDisabled => false;

    private const string UrbanDictUrl = "https://api.urbandictionary.com/v0";


    public static async Task<UrbanDictData?> GetDefinitionForTermAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException(@"Query missing", nameof(query));

        try {
            string url = $"{UrbanDictUrl}/define?term={WebUtility.UrlEncode(query)}";
            string result = await _http.GetStringAsync(url).ConfigureAwait(false);
            UrbanDictData data = JsonConvert.DeserializeObject<UrbanDictData>(result) ?? throw new JsonSerializationException();
            if (data.ResultType == "no_results" || !data.List.Any())
                return null;

            foreach (UrbanDictList res in data.List) {
                res.Definition = new string(res.Definition.Where(c => c is not ']' and not '[').ToArray());
                if (!string.IsNullOrWhiteSpace(res.Example))
                    res.Example = new string(res.Example.Where(c => c is not ']' and not '[').ToArray());
            }

            return data;
        } catch (Exception e) {
            Log.Error(e, "Failed to retrieve Urban Dictionary data");
            return null;
        }
    }
}