﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static string ApplicationName => "TheGodfather";
        public static string ApplicationVersion => "v5.0.0-beta";

        public static IReadOnlyList<TheGodfatherShard> ActiveShards => _shards.AsReadOnly();

        private static BotConfig _cfg;
        private static DatabaseContextBuilder _dbb;
        private static List<TheGodfatherShard> _shards;
        private static SharedData _shared;
        private static AsyncExecutor _async;

        #region Timers
        private static Timer BotStatusUpdateTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }
        private static Timer MiscActionsTimer { get; set; }
        private static Timer SavedTaskLoadTimer { get; set; }
        #endregion


        internal static async Task Main(string[] _)
        {
            PrintBuildInformation();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try {
                await LoadBotConfigAsync();
                await InitializeDatabaseAsync();
                InitializeSharedData();
                await CreateAndBootShardsAsync();

                _shared.LogProvider.ElevatedLog(LogLevel.Info, "Booting complete!");

                await Task.Delay(Timeout.Infinite, _shared.MainLoopCts.Token);
                await DisposeAsync();
            } catch (TaskCanceledException) {
                _shared.LogProvider.ElevatedLog(LogLevel.Info, "Shutdown signal received!");
            } catch (Exception e) {
                Console.WriteLine($"\nException occured: {e.GetType()} :\n{e.Message}");
                if (!(e.InnerException is null))
                    Console.WriteLine($"Inner exception: {e.InnerException.GetType()} :\n{e.InnerException.Message}");
                Environment.ExitCode = 1;
            }
            Console.WriteLine("\nPowering off...");
            Environment.Exit(Environment.ExitCode);
        }


        public static Task Stop(int exitCode = 0, TimeSpan? after = null)
        {
            Environment.ExitCode = exitCode;
            _shared.MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
            return Task.CompletedTask;
        }


        #region Setup
        private static void PrintBuildInformation()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
            Console.WriteLine();
        }

        private static async Task LoadBotConfigAsync()
        {
            Console.Write("\r[1/5] Loading configuration...                    ");

            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/config.json");
            if (!fi.Exists) {
                Console.WriteLine("\rLoading configuration failed!             ");

                json = JsonConvert.SerializeObject(BotConfig.Default, Formatting.Indented);
                using (FileStream fs = fi.Create())
                using (var sw = new StreamWriter(fs, utf8)) {
                    await sw.WriteAsync(json);
                    await sw.FlushAsync();
                }

                Console.WriteLine("New default configuration file has been created at:");
                Console.WriteLine(fi.FullName);
                Console.WriteLine("Please fill it with appropriate values and re-run the bot.");

                throw new IOException("Configuration file not found!");
            }

            using (FileStream fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();

            _cfg = JsonConvert.DeserializeObject<BotConfig>(json);
        }

        private static async Task InitializeDatabaseAsync()
        {
            Console.Write("\r[2/5] Establishing database connection...         ");

            _dbb = new DatabaseContextBuilder(_cfg.DatabaseConfig);

            Console.Write("\r[2/5] Migrating the database...                   ");

            await _dbb.CreateContext().Database.MigrateAsync();
        }

        private static void InitializeSharedData()
        {
            Console.Write("\r[3/5] Loading data from the database...           ");

            ConcurrentHashSet<ulong> blockedChannels;
            ConcurrentHashSet<ulong> blockedUsers;
            ConcurrentDictionary<ulong, CachedGuildConfig> guildConfigurations;

            using (DatabaseContext db = _dbb.CreateContext()) {
                blockedChannels = new ConcurrentHashSet<ulong>(db.BlockedChannels.Select(c => c.ChannelId));
                blockedUsers = new ConcurrentHashSet<ulong>(db.BlockedUsers.Select(u => u.UserId));
                guildConfigurations = new ConcurrentDictionary<ulong, CachedGuildConfig>(db.GuildConfig.Select(
                    gcfg => new KeyValuePair<ulong, CachedGuildConfig>(gcfg.GuildId, new CachedGuildConfig {
                        AntispamSettings = new AntispamSettings {
                            Action = gcfg.AntispamAction,
                            Enabled = gcfg.AntispamEnabled,
                            Sensitivity = gcfg.AntispamSensitivity
                        },
                        Currency = gcfg.Currency,
                        LinkfilterSettings = new LinkfilterSettings {
                            BlockBooterWebsites = gcfg.LinkfilterBootersEnabled,
                            BlockDiscordInvites = gcfg.LinkfilterDiscordInvitesEnabled,
                            BlockDisturbingWebsites = gcfg.LinkfilterDisturbingWebsitesEnabled,
                            BlockIpLoggingWebsites = gcfg.LinkfilterIpLoggersEnabled,
                            BlockUrlShorteners = gcfg.LinkfilterUrlShortenersEnabled,
                            Enabled = gcfg.LinkfilterEnabled
                        },
                        LogChannelId = gcfg.LogChannelId,
                        Prefix = gcfg.Prefix,
                        RatelimitSettings = new RatelimitSettings {
                            Action = gcfg.RatelimitAction,
                            Enabled = gcfg.RatelimitEnabled,
                            Sensitivity = gcfg.RatelimitSensitivity
                        },
                        ReactionResponse = gcfg.ReactionResponse,
                        SuggestionsEnabled = gcfg.SuggestionsEnabled
                    }
                )));
            }

            var logger = new Logger(_cfg);
            foreach (Logger.SpecialLoggingRule rule in _cfg.SpecialLoggerRules)
                logger.ApplySpecialLoggingRule(rule);

            _shared = new SharedData {
                BlockedChannels = blockedChannels,
                BlockedUsers = blockedUsers,
                BotConfiguration = _cfg,
                MainLoopCts = new CancellationTokenSource(),
                LogProvider = logger,
                UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime)
            };
        }

        private static async Task CreateAndBootShardsAsync()
        {
            Console.Write($"\r[4/5] Creating {_cfg.ShardCount} shards...                  ");

            IServiceCollection sharedServices = new ServiceCollection()
                .AddSingleton(_shared)
                .AddSingleton(_dbb)
                .AddSingleton(new ChannelEventService())
                .AddSingleton(new FilteringService(_dbb, _shared.LogProvider))
                .AddSingleton(new GiphyService(_shared.BotConfiguration.GiphyKey))
                .AddSingleton(new GoodreadsService(_shared.BotConfiguration.GoodreadsKey))
                .AddSingleton(new GuildConfigService(_cfg, _dbb))
                .AddSingleton(new ImgurService(_shared.BotConfiguration.ImgurKey))
                .AddSingleton(new InteractivityService())
                .AddSingleton(new OMDbService(_shared.BotConfiguration.OMDbKey))
                .AddSingleton(new ReactionsService(_dbb, _shared.LogProvider))
                .AddSingleton(new SteamService(_shared.BotConfiguration.SteamKey))
                .AddSingleton(new UserRanksService())
                .AddSingleton(new WeatherService(_shared.BotConfiguration.WeatherKey))
                .AddSingleton(new YtService(_shared.BotConfiguration.YouTubeKey))
                ;

            _shards = new List<TheGodfatherShard>();
            for (int i = 0; i < _cfg.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, _dbb, _shared);
                shard.Services = sharedServices
                    .AddSingleton(new AntifloodService(shard))
                    .AddSingleton(new AntiInstantLeaveService(shard))
                    .AddSingleton(new AntispamService(shard))
                    .AddSingleton(new LinkfilterService(shard))
                    .AddSingleton(new RatelimitService(shard))
                    .BuildServiceProvider();
                shard.Initialize(e => RegisterPeriodicTasks());
                _shards.Add(shard);
            }

            Console.WriteLine("\r[5/5] Booting the shards...                   ");
            Console.WriteLine();

            await Task.WhenAll(_shards.Select(s => s.StartAsync()));
        }

        private static Task RegisterPeriodicTasks()
        {
            _async = new AsyncExecutor();
            BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, _shards[0], TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, _shards[0], TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(_cfg.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, _shards[0], TimeSpan.FromSeconds(_cfg.FeedCheckStartDelay), TimeSpan.FromSeconds(_cfg.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, _shards[0], TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));
            SavedTaskLoadTimer = new Timer(RegisterSavedTasksCallback, _shards[0], TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private static async Task DisposeAsync()
        {
            _shared.LogProvider.ElevatedLog(LogLevel.Info, "Cleaning up...");

            BotStatusUpdateTimer.Dispose();
            DatabaseSyncTimer.Dispose();
            FeedCheckTimer.Dispose();
            MiscActionsTimer.Dispose();
            SavedTaskLoadTimer.Dispose();

            foreach (TheGodfatherShard shard in _shards)
                await shard.DisposeAsync();
            _shared.Dispose();

            _shared.LogProvider.ElevatedLog(LogLevel.Info, "Cleanup complete! Powering off...");
        }
        #endregion

        #region Callbacks
        private static void BotActivityChangeCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            if (!shard.SharedData.StatusRotationEnabled)
                return;

            try {
                DatabaseBotStatus status;
                using (DatabaseContext db = shard.Database.CreateContext())
                    status = db.BotStatuses.Shuffle().FirstOrDefault();

                var activity = new DiscordActivity(status?.Status ?? "@TheGodfather help", status?.Activity ?? ActivityType.Playing);
                _async.Execute(shard.Client.UpdateStatusAsync(activity));
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void DatabaseSyncCallback(object _)
        {
            var shard = _ as TheGodfatherShard;
            try {
                using (DatabaseContext db = shard.Database.CreateContext())
                    shard.Services.GetService<UserRanksService>().Sync(db);
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                _async.Execute(RssService.CheckFeedsForChangesAsync(shard.Client, _dbb));
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void MiscellaneousActionsCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                List<DatabaseBirthday> todayBirthdays;
                using (DatabaseContext db = _dbb.CreateContext()) {
                    todayBirthdays = db.Birthdays
                        .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                        .ToList();
                }
                foreach (DatabaseBirthday birthday in todayBirthdays) {
                    DiscordChannel channel = _async.Execute(shard.Client.GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = _async.Execute(shard.Client.GetUserAsync(birthday.UserId));
                    _async.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
                        Description = $"{StaticDiscordEmoji.Tada} Happy birthday, {user.Mention}! {StaticDiscordEmoji.Cake}",
                        Color = DiscordColor.Aquamarine
                    }));

                    using (DatabaseContext db = _dbb.CreateContext()) {
                        birthday.LastUpdateYear = DateTime.Now.Year;
                        db.Birthdays.Update(birthday);
                        db.SaveChanges();
                    }
                }

                using (DatabaseContext db = _dbb.CreateContext()) {
                    db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                    db.SaveChanges();
                }
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void RegisterSavedTasksCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                using (DatabaseContext db = _dbb.CreateContext()) {
                    var savedTasks = db.SavedTasks
                        .Where(t => t.ExecutionTime <= DateTimeOffset.Now + TimeSpan.FromMinutes(5))
                        .ToDictionary<DatabaseSavedTask, int, SavedTaskInfo>(
                            t => t.Id,
                            t => {
                                switch (t.Type) {
                                    case SavedTaskType.Unban:
                                        return new UnbanTaskInfo(t.GuildId, t.UserId, t.ExecutionTime);
                                    case SavedTaskType.Unmute:
                                        return new UnmuteTaskInfo(t.GuildId, t.UserId, t.RoleId, t.ExecutionTime);
                                    default:
                                        return null;
                                }
                            }
                        );
                    RegisterSavedTasks(savedTasks);

                    var reminders = db.Reminders
                        .Where(t => t.ExecutionTime <= DateTimeOffset.Now + TimeSpan.FromMinutes(5))
                        .ToDictionary(
                            t => t.Id,
                            t => new SendMessageTaskInfo(t.ChannelId, t.UserId, t.Message, t.ExecutionTime, t.IsRepeating, t.RepeatInterval)
                        );
                    RegisterReminders(reminders);
                }
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }


            void RegisterSavedTasks(IReadOnlyDictionary<int, SavedTaskInfo> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SavedTaskInfo task) in tasks) {
                    if (_async.Execute(RegisterTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                _shared.LogProvider.ElevatedLog(LogLevel.Info, $"Saved task scheduler: {scheduled} scheduled; {missed} missed.");
            }

            void RegisterReminders(IReadOnlyDictionary<int, SendMessageTaskInfo> reminders)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SendMessageTaskInfo task) in reminders) {
                    if (_async.Execute(RegisterTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                _shared.LogProvider.ElevatedLog(LogLevel.Info, $"Reminder scheduler: {scheduled} scheduled; {missed} missed.");
            }

            async Task<bool> RegisterTaskAsync(int id, SavedTaskInfo tinfo)
            {
                var texec = new SavedTaskExecutor(id, shard.Client, tinfo, _shared, _dbb);
                if (texec.TaskInfo.IsExecutionTimeReached) {
                    await texec.HandleMissedExecutionAsync();
                    return false;
                } else {
                    texec.Schedule();
                    return true;
                }
            }
        }
        #endregion
    }
}