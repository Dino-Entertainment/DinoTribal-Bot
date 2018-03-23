﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Extensions.Collections;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("filter")]
    [Description("Message filtering commands. If invoked without subcommand, adds a new filter for the given word list. Words can be regular expressions.")]
    [Aliases("f", "filters")]
    [UsageExample("!filter fuck fk f+u+c+k+")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class FilterModule : TheGodfatherBaseModule
    {

        public FilterModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
            => await AddAsync(ctx, filters).ConfigureAwait(false);


        #region COMMAND_FILTER_ADD
        [Command("add")]
        [Description("Add filter to guild filter list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!filter add fuck f+u+c+k+")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Filter list. Filter is a regular expression (case insensitive).")] params string[] filters)
        {
            if (filters == null || !filters.Any())
                throw new InvalidCommandUsageException("Filter regexes missing.");

            var errors = new StringBuilder();
            foreach (var filter in filters) {
                if (filter.Contains('%')) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} cannot contain '%' character.");
                    continue;
                }

                if (filter.Length < 3 || filter.Length > 60) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} doesn't fit the size requirement. Filters cannot be shorter than 3 and longer than 60 characters.");
                    continue;
                }

                if (Shared.TextTriggerExists(ctx.Guild.Id, filter)) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} cannot be added because of a conflict with an existing text trigger in this guild.");
                    continue;
                }

                if (!TryParseRegex(filter, out var regex)) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} is not a valid regular expression.");
                    continue;
                }

                if (ctx.Client.GetCommandsNext().RegisteredCommands.Any(kvp => regex.IsMatch(kvp.Key))) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} collides with an existing bot command.");
                    continue;
                }

                if (Shared.Filters.ContainsKey(ctx.Guild.Id)) {
                    if (Shared.Filters[ctx.Guild.Id].Any(r => r.ToString() == regex.ToString())) {
                        errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} already exists.");
                        continue;
                    }
                    Shared.Filters[ctx.Guild.Id].Add(regex);
                } else {
                    Shared.Filters.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<Regex>() { regex });
                }

                try {
                    await Database.AddFilterAsync(ctx.Guild.Id, filter)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Logger.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to add filter {Formatter.Bold(filter)} to the database.");
                }
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FILTERS_CLEAR
        [Command("clear")]
        [Description("Delete all filters for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!filter clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all filters for this guild?").ConfigureAwait(false))
                return;

            if (Shared.Filters.ContainsKey(ctx.Guild.Id))
                Shared.Filters.TryRemove(ctx.Guild.Id, out _);

            try {
                await Database.DeleteAllGuildFiltersAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to delete filters from the database.");
            }

            await ctx.RespondWithIconEmbedAsync("Removed all filters!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FILTER_DELETE
        [Command("delete")]
        [Description("Remove filters from guild filter list.")]
        [Aliases("-", "remove", "del", "rm", "rem", "d")]
        [UsageExample("!filter delete fuck f+u+c+k+")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Filters to remove.")] params string[] filters)
        {
            if (!Shared.Filters.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no filters registered.");

            var errors = new StringBuilder();
            foreach (var filter in filters) {
                var rstr = $@"\b{filter}\b";
                if (Shared.Filters[ctx.Guild.Id].RemoveWhere(r => r.ToString() == rstr) == 0) {
                    errors.AppendLine($"Error: Filter {Formatter.Bold(filter)} does not exist.");
                    continue;
                }

                try {
                    await Database.RemoveFilterAsync(ctx.Guild.Id, filter)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Logger.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to remove filter {Formatter.Bold(filter)} from the database.");
                }
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FILTER_LIST
        [Command("list")]
        [Description("Show all filters for this guild.")]
        [Aliases("ls", "l")]
        [UsageExample("!filter list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!Shared.Filters.ContainsKey(ctx.Guild.Id) || !Shared.Filters[ctx.Guild.Id].Any())
                throw new CommandFailedException("No filters registered for this guild.");

            await ctx.SendPaginatedCollectionAsync(
                "Filters in this guild",
                Shared.Filters[ctx.Guild.Id],
                r => r.ToString().Replace(@"\b", ""),
                DiscordColor.DarkGreen
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
