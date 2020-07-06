﻿using Microsoft.EntityFrameworkCore.Design;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TheGodfatherDbContext>
    {
        private readonly BotConfigService cfg;
        private readonly AsyncExecutionService async;


        public DesignTimeDbContextFactory()
        {
            this.cfg = new BotConfigService();
            this.async = new AsyncExecutionService();
        }


        public TheGodfatherDbContext CreateDbContext(params string[] _)
        {
            BotConfig cfg = this.async.Execute(this.cfg.LoadConfigAsync("Resources/config.json"));
            return new DatabaseContextBuilder(cfg.DatabaseConfig).CreateDbContext();
        }
    }
}
