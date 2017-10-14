﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    internal class BotDependencyList
    {
        internal AliasManager AliasControl { get; private set; }
        internal FilterManager FilterControl { get; private set; }
        internal InsultManager InsultControl { get; private set; }
        internal MemeManager MemeControl { get; private set; }
        internal PrefixManager PrefixControl { get; private set; }
        internal RankManager RankControl { get; private set; }
        internal StatusManager StatusControl { get; private set; }
        // channels in CommandsGuild
        // reactions
        // swat servers
        // hmm... there has to be something else, check files please

        internal BotDependencyList()
        {
            AliasControl = new AliasManager();
            FilterControl = new FilterManager();
            InsultControl = new InsultManager();
            MemeControl = new MemeManager();
            PrefixControl = new PrefixManager();
            RankControl = new RankManager();
            StatusControl = new StatusManager();
        }


        internal void LoadData(DebugLogger log)
        {
            AliasControl.Load(log);
            FilterControl.Load(log);
            InsultControl.Load(log);
            MemeControl.Load(log);
            PrefixControl.Load(log);
            RankControl.Load(log);
            StatusControl.Load(log);
        }

        internal void SaveData(DebugLogger log)
        {
            AliasControl.Save(log);
            FilterControl.Save(log);
            InsultControl.Save(log);
            MemeControl.Save(log);
            PrefixControl.Save(log);
            RankControl.Save(log);
            StatusControl.Save(log);
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(AliasControl)
                .AddInstance(FilterControl)
                .AddInstance(InsultControl)
                .AddInstance(MemeControl)
                .AddInstance(PrefixControl)
                .AddInstance(RankControl)
                .AddInstance(StatusControl);
        }
    }
}
