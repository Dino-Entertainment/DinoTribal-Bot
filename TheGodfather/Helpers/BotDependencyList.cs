﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using TheGodfather.Helpers;
using TheGodfather.Services;
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
        internal AliasManager       AliasControl        { get; private set; }
        internal BankManager        BankControl         { get; private set; }
        internal FeedManager        FeedControl         { get; private set; }
        internal FilterManager      FilterControl       { get; private set; }
        internal InsultManager      InsultControl       { get; private set; }
        internal MemeManager        MemeControl         { get; private set; }
        internal RankManager        RankControl         { get; private set; }
        internal ReactionManager    ReactionControl     { get; private set; }
        internal StatusManager      StatusControl       { get; private set; }
        internal SwatServerManager  SwatServerControl   { get; private set; }
        internal GuildConfigManager GuildConfigControl  { get; private set; }
        internal GiphyService       GiphyService        { get; private set; }
        internal ImgurService       ImgurService        { get; private set; }
        internal SteamService       SteamService        { get; private set; }
        internal YoutubeService     YoutubeService      { get; private set; }
        internal JokesService       JokesService        { get; private set; }


        internal BotDependencyList(DiscordClient client, BotConfig cfg)
        {
            AliasControl = new AliasManager();
            BankControl = new BankManager();
            FeedControl = new FeedManager(client);
            FilterControl = new FilterManager();
            InsultControl = new InsultManager();
            MemeControl = new MemeManager();
            RankControl = new RankManager();
            ReactionControl = new ReactionManager();
            StatusControl = new StatusManager();
            SwatServerControl = new SwatServerManager();
            GuildConfigControl = new GuildConfigManager(cfg);
            GiphyService = new GiphyService(cfg.GiphyKey);
            ImgurService = new ImgurService(cfg.ImgurKey);
            SteamService = new SteamService(cfg.SteamKey);
            YoutubeService = new YoutubeService(cfg.YoutubeKey);
            JokesService = new JokesService();
        }


        internal void LoadData(DebugLogger log)
        {
            AliasControl.Load(log);
            BankControl.Load(log);
            FeedControl.Load(log);
            FilterControl.Load(log);
            InsultControl.Load(log);
            MemeControl.Load(log);
            RankControl.Load(log);
            ReactionControl.Load(log);
            StatusControl.Load(log);
            SwatServerControl.Load(log);
            GuildConfigControl.Load(log);
        }

        internal void SaveData(DebugLogger log)
        {
            AliasControl.Save(log);
            BankControl.Save(log);
            FeedControl.Save(log);
            FilterControl.Save(log);
            InsultControl.Save(log);
            MemeControl.Save(log);
            RankControl.Save(log);
            ReactionControl.Save(log);
            StatusControl.Save(log);
            SwatServerControl.Save(log);
            GuildConfigControl.Save(log);
        }

        internal DependencyCollectionBuilder GetDependencyCollectionBuilder()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(AliasControl)
                .AddInstance(BankControl)
                .AddInstance(FeedControl)
                .AddInstance(FilterControl)
                .AddInstance(InsultControl)
                .AddInstance(MemeControl)
                .AddInstance(RankControl)
                .AddInstance(ReactionControl)
                .AddInstance(StatusControl)
                .AddInstance(SwatServerControl)
                .AddInstance(GiphyService)
                .AddInstance(ImgurService)
                .AddInstance(SteamService)
                .AddInstance(YoutubeService)
                .AddInstance(JokesService)
                .AddInstance(GuildConfigControl);
        }
    }
}
