﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database.Models;
#endregion

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class DbSetExtensions
    {
        public static void SafeAddRange<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> entities)
            where TEntity : class, IEquatable<TEntity>
        {
            set.AddRange(entities.Except(set));
        }

        public static void AddExemptions<TEntity, TExempt>(this DbSet<TEntity> set, ulong gid, IEnumerable<TExempt> exempts, ExemptedEntityType type)
            where TEntity : ExemptedEntity, new()
            where TExempt : SnowflakeObject
        {
            set.AddRange(exempts
                .Where(e => !set.Where(dbe => dbe.GuildId == gid).Any(dbe => dbe.Type == type && dbe.Id == e.Id))
                .Select(e => new TEntity {
                    GuildId = gid,
                    Id = e.Id,
                    Type = type
                })
            );
        }
    }
}
