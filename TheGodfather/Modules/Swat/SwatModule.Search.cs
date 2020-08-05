﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Swat.Extensions;
#endregion

namespace TheGodfather.Modules.Swat
{
    public partial class SwatModule
    {
        [Group("search"), Hidden]
        [Description("SWAT4 database search commands.")]
        [Aliases("s", "find", "lookup")]
        [RequirePrivilegedUser]

        public class SwatSearchModule : TheGodfatherModule
        {

            public SwatSearchModule(DbContextBuilder db)
                : base(db)
            {

            }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("IP or range.")] IPAddressRange ip,
                                         [Description("Number of results")] int amount = 10)
                 => this.SearchIpAsync(ctx, ip, amount);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Player name to search.")] string name,
                                         [Description("Number of results")] int amount = 10)
                 => this.SearchNameAsync(ctx, name, amount);


            #region COMMAND_SEARCH_IP
            [Command("ip")]
            [Description("Search for a given IP or range.")]

            public async Task SearchIpAsync(CommandContext ctx,
                                           [Description("IP or range.")] IPAddressRange ip,
                                           [Description("Number of results")] int amount = 10)
            {
                if (amount < 1 || amount > 100)
                    throw new InvalidCommandUsageException("Amount of results to fetch is out of range [1, 100].");

                List<SwatPlayer> matches;
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    matches = db.SwatPlayers
                        .Include(p => p.DbAliases)
                        .Include(p => p.DbIPs)
                        .AsEnumerable()
                        .Where(p => p.IPs.Any(dbip => dbip.StartsWith(ip.Range)))
                        .ToList();
                }

                if (!matches.Any())
                    throw new CommandFailedException("No results.");

                await ctx.SendCollectionInPagesAsync($"Search matches for {ip.Range}", matches, p => p.Stringify(), this.ModuleColor, 1);
            }
            #endregion

            #region COMMAND_SEARCH_NAME
            [Command("name")]
            [Description("Search for a given name.")]
            [Aliases("player", "nickname", "nick")]

            public async Task SearchNameAsync(CommandContext ctx,
                                             [Description("Player name.")] string name,
                                             [Description("Number of results")] int amount = 10)
            {
                if (amount < 1 || amount > 20)
                    throw new InvalidCommandUsageException("Amount of results to fetch is out of range [1, 20].");

                var matches = new List<SwatPlayer>();
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    matches = db.SwatPlayers
                        .Include(p => p.DbAliases)
                        .Include(p => p.DbIPs)
                        .AsEnumerable()
                        .Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase) ||
                               p.Aliases.Any(a => a.Contains(name, StringComparison.InvariantCultureIgnoreCase)))
                        .OrderBy(p => p.Aliases.Any() ?
                            Math.Min(Math.Abs(p.Name.LevenshteinDistanceTo(name)), p.Aliases.Min(a => Math.Abs(a.LevenshteinDistanceTo(name)))) :
                            Math.Abs(p.Name.LevenshteinDistanceTo(name))
                        ).Take(amount)
                        .ToList();
                }

                if (!matches.Any())
                    throw new CommandFailedException("No results.");

                await ctx.SendCollectionInPagesAsync($"Search matches for {name}", matches, p => p.Stringify(), this.ModuleColor, 1);
            }
            #endregion
        }
    }
}
