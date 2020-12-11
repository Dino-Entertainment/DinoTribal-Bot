﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static string ApplicationName { get; }
        public static string ApplicationVersion { get; }
        public static IReadOnlyList<TheGodfatherShard> ActiveShards => _shards.AsReadOnly();

        private static ServiceProvider? ServiceProvider { get; set; }
        private static PeriodicTasksService? PeriodicService { get; set; }

        private static readonly List<TheGodfatherShard> _shards = new List<TheGodfatherShard>();


        static TheGodfather()
        {
            AssemblyName info = Assembly.GetExecutingAssembly().GetName();
            ApplicationName = info.Name ?? "TheGodfather";
            ApplicationVersion = $"v{info.Version?.ToString() ?? "<unknown>"}";
        }


        internal static async Task Main(string[] _)
        {
            PrintBuildInformation();

            try {
                BotConfigService cfg = await LoadBotConfigAsync();
                Log.Logger = LogExt.CreateLogger(cfg.CurrentConfiguration);
                Log.Information("Logger created.");

                DbContextBuilder dbb = await InitializeDatabaseAsync(cfg);
                await CreateAndBootShardsAsync(cfg, dbb);
                Log.Information("Booting complete!");

                PeriodicService = new PeriodicTasksService(_shards[0], cfg.CurrentConfiguration);

                if (ServiceProvider is null)
                    throw new ArgumentNullException(nameof(ServiceProvider), "Service provider is not initialized.");

                await Task.Delay(Timeout.Infinite, ServiceProvider.GetRequiredService<BotActivityService>().MainLoopCts.Token);
            } catch (TaskCanceledException) {
                Log.Information("Shutdown signal received!");
            } catch (Exception e) {
                Log.Fatal(e, "Critical exception occurred");
                Environment.ExitCode = 1;
            } finally {
                await DisposeAsync();
            }

            Log.Information("Powering off");
            Environment.Exit(Environment.ExitCode);
        }

        public static Task Stop(int exitCode = 0, TimeSpan? after = null)
        {
            Environment.ExitCode = exitCode;
            ServiceProvider?.GetRequiredService<BotActivityService>().MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
            Log.CloseAndFlush();
            return Task.CompletedTask;
        }


        #region Setup
        private static void PrintBuildInformation()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
            Console.WriteLine();
        }

        private static async Task<BotConfigService> LoadBotConfigAsync()
        {
            Console.Write("Loading configuration... ");

            var cfg = new BotConfigService();
            await cfg.LoadConfigAsync();

            Console.Write("\r");
            return cfg;
        }

        private static async Task<DbContextBuilder> InitializeDatabaseAsync(BotConfigService cfg)
        {
            Log.Information("Establishing database connection");
            var dbb = new DbContextBuilder(cfg.CurrentConfiguration.DatabaseConfig);

            Log.Information("Testing database context creation");
            using (TheGodfatherDbContext db = dbb.CreateContext()) {
                IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any()) {
                    Log.Information("Applying pending database migrations: {PendingDbMigrations}", pendingMigrations);
                    await db.Database.MigrateAsync();
                }
            }

            return dbb;
        }

        private static Task CreateAndBootShardsAsync(BotConfigService cfg, DbContextBuilder dbb)
        {
            Log.Information("Initializing services");
            IServiceCollection services = new ServiceCollection()
                .AddSingleton(cfg)
                .AddSingleton(dbb)
                .AddSingleton(new BotActivityService(dbb, cfg.CurrentConfiguration.ShardCount))
                .AddSingleton(new AsyncExecutionService())
                .AddSharedServices()
                ;
            ServiceProvider = services.BuildServiceProvider();

            Log.Information("Creating {ShardCount} shard(s)", cfg.CurrentConfiguration.ShardCount);
            for (int i = 0; i < cfg.CurrentConfiguration.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, services);
                _shards.Add(shard);
            }

            // CheckCommandLocalization(_shards[0]);

            Log.Information("Booting the shards");

            return Task.WhenAll(_shards.Select(s => s.StartAsync()));


            void CheckCommandLocalization(TheGodfatherShard shard)
            {
                LocalizationService lcs = shard.Services.GetRequiredService<LocalizationService>();
                foreach ((string cmdName, Command cmd) in shard.CNext.RegisteredCommands) {
                    try {
                        _ = lcs.GetCommandDescription(0, cmdName);
                        IEnumerable<CommandArgument> args = cmd.Overloads.SelectMany(o => o.Arguments).Distinct();
                        foreach (CommandArgument arg in args)
                            _ = lcs.GetString(null, arg.Description);
                    } catch (LocalizationException e) {
                        Log.Warning(e, "Translation not found");
                    }
                }
            }
        }

        private static async Task DisposeAsync()
        {
            Log.Information("Cleaning up ...");

            PeriodicService?.Dispose();

            if (_shards is { }) {
                foreach (TheGodfatherShard shard in _shards)
                    await shard.DisposeAsync();
            }

            if (ServiceProvider is { }) {
                foreach (IDisposable service in ServiceProvider.GetServices<IDisposable>())
                    service.Dispose();
            }

            Log.Information("Cleanup complete! Powering off");
        }
        #endregion
    }
}
