﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services.Common;

namespace TheGodfather.Services
{
    public sealed class BotActivityService : DbAbstractionServiceBase<BotStatus, int>, IDisposable
    {
        public bool IsBotListening {
            get => this.isBotListening;
            set {
                lock (this.lck)
                    this.isBotListening = value;
            }
        }
        public bool StatusRotationEnabled {
            get => this.statusRotationEnabled;
            set {
                lock (this.lck)
                    this.statusRotationEnabled = value;
            }
        }
        public CancellationTokenSource MainLoopCts { get; }
        public ImmutableDictionary<int, UptimeInformation> ShardUptimeInformation { get; }

        public override bool IsDisabled => false;

        private bool statusRotationEnabled;
        private bool isBotListening;
        private readonly object lck = new object();


        public BotActivityService(DbContextBuilder dbb, int shardCount)
            : base(dbb)
        {
            this.IsBotListening = true;
            this.MainLoopCts = new CancellationTokenSource();
            this.StatusRotationEnabled = true;
            var uptimeDict = new Dictionary<int, UptimeInformation>();
            for (int i = 0; i < shardCount; i++)
                uptimeDict.Add(i, new UptimeInformation(Process.GetCurrentProcess().StartTime));
            this.ShardUptimeInformation = uptimeDict.ToImmutableDictionary();
        }


        public bool ToggleListeningStatus()
        {
            lock (this.lck)
                this.IsBotListening = !this.IsBotListening;
            return this.IsBotListening;
        }

        public void Dispose()
        {
            this.MainLoopCts.Dispose();
        }

        public BotStatus? GetRandomStatus()
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            return this.DbSetSelector(db).Shuffle().FirstOrDefault();
        }

        public override DbSet<BotStatus> DbSetSelector(TheGodfatherDbContext db) => db.BotStatuses;
        public override BotStatus EntityFactory(int id) => new BotStatus { Id = id };
        public override int EntityIdSelector(BotStatus entity) => entity.Id;
        public override object[] EntityPrimaryKeySelector(int id) => new object[] { id };
    }
}
