﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Messages
{
    [Group("filter", CanInvokeWithoutSubcommand = false)]
    [Description("Message filtering commands.")]
    [Aliases("f", "filters")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [CheckListeningAttribute]
    public class CommandsFilter
    {        
        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter. Can be a regex (case insensitive).")] string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new InvalidCommandUsageException("Filter trigger missing.");
            
            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TriggerExists(ctx.Guild.Id, filter))
                throw new CommandFailedException("You cannot add a filter if a trigger for that trigger exists!");

            if (filter.Contains("%") || filter.Length < 3)
                throw new CommandFailedException($"Filter must not contain {Formatter.Bold("%")} or have less than 3 characters.");

            var regex = new Regex($"^{filter}$", RegexOptions.IgnoreCase);

            if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kv => regex.Match(kv.Key).Success))
                throw new CommandFailedException("You cannot add a filter that matches one of the commands!");
            
            if (ctx.Dependencies.GetDependency<FilterManager>().TryAdd(ctx.Guild.Id, regex))
                await ctx.RespondAsync($"Filter successfully added.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Filter already exists!");
        }
        #endregion
        
        #region COMMAND_FILTER_DELETE
        [Command("delete")]
        [Description("Remove filter from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [Description("Filter index.")] int i)
        {
            if (ctx.Dependencies.GetDependency<FilterManager>().TryRemoveAt(ctx.Guild.Id, i))
                await ctx.RespondAsync("Filter successfully removed.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Filter at that index does not exist.");
        }
        #endregion
        
        #region COMMAND_FILTER_SAVE
        [Command("save")]
        [Description("Save filters to file.")]
        [RequireOwner]
        public async Task SaveAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<FilterManager>().Save(ctx.Client.DebugLogger))
                await ctx.RespondAsync("Filters successfully saved.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed saving filters.", new IOException());
        }
        #endregion
        
        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page")] int page = 1)
        {
            var filters = ctx.Dependencies.GetDependency<FilterManager>().Filters;

            if (!filters.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No filters registered.");
                return;
            }

            if (page < 1 || page > filters[ctx.Guild.Id].Count / 10 + 1)
                throw new CommandFailedException("No filters on that page.");

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < filters[ctx.Guild.Id].Count ? starti + 10 : filters[ctx.Guild.Id].Count;
            var pagefilters = filters[ctx.Guild.Id].Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++) {
                var filter = pagefilters[i].ToString();
                desc += $"{Formatter.Bold(i.ToString())} : {filter.Substring(1, filter.Length - 2)}\n";
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available filters (page {page}/{filters[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = desc,
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_FILTERS_CLEAR
        [Command("clear")]
        [Description("Delete all filters for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<FilterManager>().ClearGuildFilters(ctx.Guild.Id))
                await ctx.RespondAsync("All filters for this guild successfully removed.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Clearing guild filters failed");
        }
        #endregion

        #region COMMAND_FILTER_CLEARALL
        [Command("clearall")]
        [Description("Delete all filters stored for ALL guilds.")]
        [RequireOwner]
        public async Task ClearAllAsync(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<FilterManager>().ClearAllFilters();
            await ctx.RespondAsync("All filters successfully removed.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
