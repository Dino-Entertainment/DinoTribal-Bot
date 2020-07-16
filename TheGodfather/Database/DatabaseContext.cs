﻿using System;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Entities;

namespace TheGodfather.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<DatabaseInsult> Insults { get; set; }
        public virtual DbSet<DatabasePrivilegedUser> PrivilegedUsers { get; set; }

        public virtual DbSet<DatabaseSwatPlayer> SwatPlayers { get; set; }
        public virtual DbSet<DatabaseSwatServer> SwatServers { get; set; }

        private string ConnectionString { get; }
        private DbProvider Provider { get; }


        public DatabaseContext(DbProvider provider, string connectionString)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }

        public DatabaseContext(DbProvider provider, string connectionString, DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            //optionsBuilder.EnableSensitiveDataLogging(true);
            //optionsBuilder.UseLazyLoadingProxies();
            //optionsBuilder.ConfigureWarnings(wb => wb.Ignore(CoreEventId.DetachedLazyLoadingWarning));

            switch (this.Provider) {
                case DbProvider.PostgreSql:
                    optionsBuilder.UseNpgsql(this.ConnectionString);
                    break;
                case DbProvider.Sqlite:
                case DbProvider.SqliteInMemory:
                    optionsBuilder.UseSqlite(this.ConnectionString);
                    break;
                case DbProvider.SqlServer:
                    optionsBuilder.UseSqlServer(this.ConnectionString);
                    break;
                default:
                    throw new NotSupportedException("Provider not supported!");
            }
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.HasDefaultSchema("gf");

            mb.ForNpgsqlUseIdentityAlwaysColumns();

            mb.Entity<DatabaseSwatPlayer>().Property(p => p.IsBlacklisted).HasDefaultValue(false);
            mb.Entity<DatabaseSwatPlayer>().HasIndex(p => p.Name).IsUnique();
            mb.Entity<DatabaseSwatPlayerAlias>().HasKey(p => new { p.Alias, p.PlayerId });
            mb.Entity<DatabaseSwatPlayerIP>().HasKey(p => new { p.IP, p.PlayerId });
            mb.Entity<DatabaseSwatServer>().HasKey(srv => new { srv.IP, srv.JoinPort, srv.QueryPort });
            mb.Entity<DatabaseSwatServer>().Property(srv => srv.JoinPort).HasDefaultValue(10480);
        }
    }
}