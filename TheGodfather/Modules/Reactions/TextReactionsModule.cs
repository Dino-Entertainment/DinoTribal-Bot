﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Common.Collections;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Reactions
{
    [Group("textreaction")]
    [Description("Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use \"%user%\" inside response and the bot will replace it with mention for the user who triggers the reaction.")]
    [Aliases("treact", "tr", "txtr", "textreactions")]
    [UsageExample("!textreaction hi hello")]
    [UsageExample("!textreaction \"hi\" \"Hello, %user%!\"")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class TextReactionsModule : TheGodfatherBaseModule
    {

        public TextReactionsModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("Trigger string (case insensitive).")] string trigger,
                                           [RemainingText, Description("Response.")] string response)
            => await AddAsync(ctx, trigger, response).ConfigureAwait(false);


        #region COMMAND_TEXT_REACTION_ADD
        [Command("add")]
        [Description("Add a new text reaction to guild text reaction list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!textreaction add \"hi\" \"Hello, %user%!\"")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Trigger string (case insensitive).")] string trigger,
                                  [RemainingText, Description("Response.")] string response)
            => await AddTextReactionAsync(ctx, trigger, response, false).ConfigureAwait(false);
        #endregion

        #region COMMAND_TEXT_REACTION_ADDREGEX
        [Command("addregex")]
        [Description("Add a new text reaction triggered by a regex to guild text reaction list.")]
        [Aliases("+r", "+regex", "+regexp", "+rgx", "newregex", "addrgx")]
        [UsageExample("!textreaction addregex \"h(i|ey|ello|owdy)\" \"Hello, %user%!\"")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddRegexAsync(CommandContext ctx,
                                       [Description("Regex (case insensitive).")] string trigger,
                                       [RemainingText, Description("Response.")] string response)
            => await AddTextReactionAsync(ctx, trigger, response, true).ConfigureAwait(false);
        #endregion

        #region COMMAND_TEXT_REACTION_CLEAR
        [Command("clear")]
        [Description("Delete all text reactions for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!textreactions clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all text reactions for this guild?").ConfigureAwait(false))
                return;

            if (Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                Shared.TextReactions.TryRemove(ctx.Guild.Id, out _);

            try {
                await Database.RemoveAllGuildTextReactionsAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to delete text reactions from the database.");
            }

            await ctx.RespondWithIconEmbedAsync("Removed all text reactions!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove text reaction from guild text reaction list.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [UsageExample("!textreaction delete 5")]
        [UsageExample("!textreaction delete 5 8")]
        [UsageExample("!textreaction delete hi")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("IDs of the reactions to remove.")] params int[] ids)
        {
            if (!Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no text reactions registered.");

            var sb = new StringBuilder();
            foreach (var id in ids) {
                if (!Shared.TextReactions[ctx.Guild.Id].Any(tr => tr.Id == id)) {
                    sb.AppendLine($"Note: Reaction with ID {id} does not exist in this guild.");
                    continue;
                }
            }

            try {
                await Database.RemoveTextReactionsAsync(ctx.Guild.Id, ids)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                sb.AppendLine($"Warning: Failed to remove some reactions from the database.");
            }

            int removed = Shared.TextReactions[ctx.Guild.Id].RemoveWhere(tr => ids.Contains(tr.Id));

            await ctx.RespondWithIconEmbedAsync($"Removed {removed} reactions!\n\n{sb.ToString()}")
                .ConfigureAwait(false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Triggers missing.");

            if (!Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no text reactions registered.");

            var errors = new StringBuilder();
            foreach (var trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;

                if (!IsValidRegex(trigger)) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                var found = Shared.TextReactions[ctx.Guild.Id].Where(tr => tr.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    errors.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                bool success = true;
                foreach (var tr in found)
                    success |= tr.RemoveTrigger(trigger);
                if (!success) {
                    errors.AppendLine($"Warning: Failed to remove some text reactions for trigger {Formatter.Bold(trigger)}.");
                    continue;
                }
            }

            try {
                await Database.RemoveTextReactionTriggersAsync(ctx.Guild.Id, triggers)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                errors.AppendLine($"Warning: Failed to remove some triggers from the database.");
            }

            int removed = Shared.TextReactions[ctx.Guild.Id].RemoveWhere(tr => tr.TriggerRegexes.Count == 0);

            await ctx.RespondWithIconEmbedAsync($"Removed {removed} reactions!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_LIST
        [Command("list")]
        [Description("Show all text reactions for the guild.")]
        [Aliases("ls", "l", "view")]
        [UsageExample("!textreactions list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!Shared.TextReactions.ContainsKey(ctx.Guild.Id) || !Shared.TextReactions[ctx.Guild.Id].Any())
                throw new CommandFailedException("This guild has no text reactions registered.");
            
            await ctx.SendPaginatedCollectionAsync(
                "Text reactions for this guild",
                Shared.TextReactions[ctx.Guild.Id].OrderBy(tr => tr.OrderedTriggerStrings.First()),
                tr => $"{tr.Id} : {tr.Response} | Triggers: {string.Join(", ", tr.TriggerStrings)}",
                DiscordColor.Blue
            ).ConfigureAwait(false);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task AddTextReactionAsync(CommandContext ctx, string trigger, string response, bool is_regex_trigger)
        {
            if (string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Response missing or invalid.");

            if (trigger.Length < 2 || response.Length < 2)
                throw new CommandFailedException("Trigger or response cannot be shorter than 2 characters.");

            if (trigger.Length > 120 || response.Length > 120)
                throw new CommandFailedException("Trigger or response cannot be longer than 120 characters.");

            if (!Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                Shared.TextReactions.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<TextReaction>());

            if (is_regex_trigger && !IsValidRegex(trigger))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");

            if (Shared.TextTriggerExists(ctx.Guild.Id, trigger))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} already exists.");

            string errors = "";
            int id = 0;
            try {
                id = await Database.AddTextReactionAsync(ctx.Guild.Id, trigger, response, is_regex_trigger)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                errors = $"Warning: Failed to add trigger {Formatter.Bold(trigger)} to the database.";
            }

            var reaction = Shared.TextReactions[ctx.Guild.Id].FirstOrDefault(tr => tr.Response == response);
            if (reaction != null) {
                if (!reaction.AddTrigger(trigger, is_regex_trigger))
                    throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
            } else {
                if (!Shared.TextReactions[ctx.Guild.Id].Add(new TextReaction(id, trigger, response, is_regex_trigger)))
                    throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors}")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
