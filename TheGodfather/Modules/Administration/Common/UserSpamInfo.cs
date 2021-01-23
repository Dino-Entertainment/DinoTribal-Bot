﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TheGodfather.Extensions;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class UserSpamInfo
    {
        private static readonly TimeSpan _resetAfter = TimeSpan.FromHours(1);

        public int RemainingUses => Volatile.Read(ref this.remainingUses);
        public bool IsActive => DateTimeOffset.UtcNow <= this.resetsAt;

        private DateTimeOffset resetsAt;
        private int remainingUses;
        private readonly int maxAmount;
        private readonly object decrementLock;
        private readonly List<string> msgs;


        public UserSpamInfo(int maxRepeats)
        {
            this.maxAmount = maxRepeats;
            this.remainingUses = maxRepeats;
            this.resetsAt = DateTimeOffset.UtcNow + _resetAfter;
            this.decrementLock = new object();
            this.msgs = new List<string>();
        }


        public bool TryDecrementAllowedMessageCount(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                newContent = "<NULL>";

            newContent = newContent.ToLowerInvariant();

            lock (this.decrementLock) {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (now >= this.resetsAt || this.msgs.All(m => m.LevenshteinDistanceTo(newContent) > 2)) {
                    Interlocked.Exchange(ref this.remainingUses, this.maxAmount);
                    this.resetsAt = now + _resetAfter;
                    this.msgs.Clear();
                }

                this.msgs.Add(newContent);
                if (this.msgs.Count > this.maxAmount) {
                    string leastMatch = this.msgs.MaxBy(s => s.LevenshteinDistanceTo(newContent));
                    this.msgs.Remove(leastMatch);
                }

                if (this.RemainingUses > 0)
                    Interlocked.Decrement(ref this.remainingUses);
            }

            return this.remainingUses > 0;
        }

        public void Reset()
        {
            this.resetsAt = DateTimeOffset.UtcNow;
        }
    }
}
