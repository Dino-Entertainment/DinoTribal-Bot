﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;

namespace TheGodfather
{
    public sealed class SharedData
    {
        public ConcurrentDictionary<ulong, string> GuildPrefixes { get; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> GuildFilters { get; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> GuildTextTriggers { get; internal set; }

        private BotConfig _cfg { get; }


        public SharedData(BotConfig cfg,
                          ConcurrentDictionary<ulong, string> gp,
                          ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> gf,
                          ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> gtt)
        {
            _cfg = cfg;
            GuildPrefixes = gp;
            GuildFilters = gf;
            GuildTextTriggers = gtt;
        }


        #region FILTERS
        public IReadOnlyCollection<Regex> GetFiltersForGuild(ulong gid)
        {
            if (GuildFilters.ContainsKey(gid) && GuildFilters[gid] != null)
                return GuildFilters[gid];
            else
                return null;
        }

        public bool ContainsFilter(ulong gid, string message)
        {
            if (!GuildFilters.ContainsKey(gid) || GuildFilters[gid] == null)
                return false;

            message = message.ToLower();
            return GuildFilters[gid].Any(f => f.Match(message).Success);
        }

        public bool TryAddGuildFilter(ulong gid, Regex regex)
        {
            if (GuildFilters.ContainsKey(gid)) {
                if (GuildFilters[gid] == null)
                    GuildFilters[gid] = new ConcurrentHashSet<Regex>();
            } else {
                if (!GuildFilters.TryAdd(gid, new ConcurrentHashSet<Regex>()))
                    return false;
            }

            if (GuildFilters[gid].Any(r => r.ToString() == regex.ToString()))
                return false;

            return GuildFilters[gid].Add(regex);
        }

        public bool TryRemoveGuildFilter(ulong gid, string filter)
        {
            if (!GuildFilters.ContainsKey(gid))
                return false;

            var rstr = $@"\b{filter}\b";
            return GuildFilters[gid].RemoveWhere(r => r.ToString() == rstr) > 0;
        }

        public void ClearGuildFilters(ulong gid)
        {
            if (!GuildFilters.ContainsKey(gid))
                return;

            GuildFilters[gid].Clear();
        }
        #endregion

        #region PREFIXES
        public string GetGuildPrefix(ulong gid)
        {
            if (GuildPrefixes.ContainsKey(gid) && !string.IsNullOrWhiteSpace(GuildPrefixes[gid]))
                return GuildPrefixes[gid];
            else
                return _cfg.DefaultPrefix;
        }

        public bool TrySetGuildPrefix(ulong gid, string prefix)
        {
            if (GuildPrefixes.ContainsKey(gid)) {
                GuildPrefixes[gid] = prefix;
                return true;
            } else {
                return GuildPrefixes.TryAdd(gid, prefix);
            }
        }
        #endregion

        #region TRIGGERS
        public IReadOnlyDictionary<string, string> GetAllGuildTextTriggers(ulong gid)
        {
            if (GuildTextTriggers.ContainsKey(gid) && GuildTextTriggers[gid] != null)
                return GuildTextTriggers[gid];
            else
                return null;
        }

        public bool TextTriggerExists(ulong gid, string trigger)
        {
            return GuildTextTriggers.ContainsKey(gid) && GuildTextTriggers[gid] != null && GuildTextTriggers[gid].ContainsKey(trigger);
        }

        public string GetResponseForTextTrigger(ulong gid, string trigger)
        {
            trigger = trigger.ToLower();
            if (TextTriggerExists(gid, trigger))
                return GuildTextTriggers[gid][trigger];
            else
                return null;
        }

        public bool TryAddGuildTextTrigger(ulong gid, string trigger, string response)
        {
            trigger = trigger.ToLower();
            if (GuildTextTriggers.ContainsKey(gid)) {
                if (GuildTextTriggers[gid] == null)
                    GuildTextTriggers[gid] = new ConcurrentDictionary<string, string>();
            } else {
                if (!GuildTextTriggers.TryAdd(gid, new ConcurrentDictionary<string, string>()))
                    return false;
            }

            return GuildTextTriggers[gid].TryAdd(trigger, response);
        }

        public bool TryRemoveGuildTriggers(ulong gid, string[] triggers)
        {
            if (!GuildTextTriggers.ContainsKey(gid))
                return true;

            bool conflict_found = false;
            foreach (var trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;
                if (GuildTextTriggers[gid].ContainsKey(trigger))
                    conflict_found |= !GuildTextTriggers[gid].TryRemove(trigger, out _);
                else
                    conflict_found = true;
            }

            return !conflict_found;
        }

        public void ClearGuildTextTriggers(ulong gid)
        {
            if (!GuildTextTriggers.ContainsKey(gid))
                return;

            GuildTextTriggers[gid].Clear();
        }
        #endregion
    }
}
