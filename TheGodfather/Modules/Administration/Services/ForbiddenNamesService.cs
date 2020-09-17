﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class ForbiddenNamesService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly DbContextBuilder dbb;


        public ForbiddenNamesService(DbContextBuilder dbb)
        {
            this.dbb = dbb;
        }


        public bool IsNameForbidden(ulong gid, string name, out ForbiddenName? match)
        {
            match = null;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                match = this.InternalGetForbiddenNamesForGuild(db, gid)
                    .AsEnumerable()
                    .FirstOrDefault(fn => fn.Regex.IsMatch(name))
                    ;
            }
            return match is { };
        }

        public IReadOnlyCollection<ForbiddenName> GetGuildForbiddenNames(ulong gid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            return this.InternalGetForbiddenNamesForGuild(db, gid).ToList().AsReadOnly();
        }

        public Task<bool> AddForbiddenNameAsync(ulong gid, string regexString)
        {
            return regexString.TryParseRegex(out Regex? regex)
                ? this.AddForbiddenNameAsync(gid, regex)
                : throw new ArgumentException($"Invalid regex string: {regexString}", nameof(regexString));
        }

        public async Task<bool> AddForbiddenNameAsync(ulong gid, Regex? regex)
        {
            if (regex is null)
                return false;

            string regexString = regex.ToString();

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<ForbiddenName> fnames = this.InternalGetForbiddenNamesForGuild(db, gid).AsEnumerable();
                if (fnames.Any(fname => string.Compare(fname.RegexString, regexString, true) == 0))
                    return false;
                var fname = new ForbiddenName {
                    GuildId = gid,
                    RegexString = regexString,
                    RegexLazy = regex,
                };
                db.ForbiddenNames.Add(fname);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> AddForbiddenNamesAsync(ulong gid, IEnumerable<string> regexStrings)
        {
            bool[] res = await Task.WhenAll(regexStrings.Select(s => s.ToRegex()).Select(r => this.AddForbiddenNameAsync(gid, r)));
            return res.All(r => r);
        }

        public Task<int> RemoveForbiddenNamesAsync(ulong gid)
            => this.InternalRemoveByPredicateAsync(gid, _ => true);

        public Task<int> RemoveForbiddenNamesAsync(ulong gid, IEnumerable<int> ids)
            => this.InternalRemoveByPredicateAsync(gid, fn => ids.Contains(fn.Id));

        public Task<int> RemoveForbiddenNamesAsync(ulong gid, IEnumerable<string> regexStrings)
            => this.InternalRemoveByPredicateAsync(gid, fn => regexStrings.Any(rstr => string.Compare(rstr, fn.RegexString, true) == 0));

        public Task<int> RemoveForbiddenNamesMatchingAsync(ulong gid, string match)
            => this.InternalRemoveByPredicateAsync(gid, fn => fn.Regex.IsMatch(match));
        

        private IQueryable<ForbiddenName> InternalGetForbiddenNamesForGuild(TheGodfatherDbContext db, ulong gid)
            => db.ForbiddenNames.Where(n => n.GuildIdDb == (long)gid);

        private async Task<int> InternalRemoveByPredicateAsync(ulong gid, Func<ForbiddenName, bool> predicate)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            var fnames = this.InternalGetForbiddenNamesForGuild(db, gid)
                .AsEnumerable()
                .Where(predicate)
                .ToList();
            db.ForbiddenNames.RemoveRange(fnames);
            await db.SaveChangesAsync();
            return fnames.Count;
        }
    }
}
