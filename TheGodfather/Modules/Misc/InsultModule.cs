﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("insult")]
    [Description("Insults manipulation. If invoked without subcommands, insults a given user.")]
    [Aliases("burn", "insults", "ins", "roast")]
    [UsageExample("!insult @Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class InsultModule : TheGodfatherBaseModule
    {

        public InsultModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("User to insult.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            if (user.Id == ctx.Client.CurrentUser.Id) {
                await ctx.RespondWithIconEmbedAsync("How original, trying to make me insult myself. Sadly it won't work.", ":middle_finger:")
                    .ConfigureAwait(false);
                return;
            }

            string insult = await Database.GetRandomInsultAsync()
                .ConfigureAwait(false);
            if (insult == null)
                throw new CommandFailedException("No available insults.");

            await ctx.RespondWithIconEmbedAsync(insult.Replace("%user%", user.Mention), ":middle_finger:")
                .ConfigureAwait(false);
        }


        #region COMMAND_INSULTS_ADD
        [Command("add")]
        [Description("Add insult to list (use %user% instead of user mention).")]
        [Aliases("+", "new", "a")]
        [UsageExample("!insult add You are so dumb, %user%!")]
        [RequireOwner]
        public async Task AddInsultAsync(CommandContext ctx,
                                        [RemainingText, Description("Insult (must contain ``%user%``).")] string insult)
        {
            if (string.IsNullOrWhiteSpace(insult))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (insult.Length >= 120)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 120 characters please.");

            if (insult.Split(new string[] { "%user%" }, StringSplitOptions.None).Count() < 2)
                throw new InvalidCommandUsageException($"Insult not in correct format (missing {Formatter.Bold("%user%")} in the insult)!");

            await Database.AddInsultAsync(insult)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_INSULTS_CLEAR
        [Command("clear")]
        [Description("Delete all insults.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!insults clear")]
        [RequireOwner]
        public async Task ClearAllInsultsAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all insults?").ConfigureAwait(false))
                return;

            await Database.RemoveAllInsultsAsync()
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("All insults successfully removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete")]
        [Description("Remove insult with a given index from list. (use command ``insults list`` to view insult indexes).")]
        [Aliases("-", "remove", "del", "rm", "rem", "d")]
        [UsageExample("!insult delete 2")]
        [RequireOwner]
        public async Task DeleteInsultAsync(CommandContext ctx, 
                                           [Description("Index of the insult to remove.")] int index)
        {
            await Database.RemoveInsultAsync(index)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list")]
        [Description("Show all insults.")]
        [Aliases("ls", "l")]
        [UsageExample("!insult list")]
        public async Task ListInsultsAsync(CommandContext ctx)
        {
            var insults = await Database.GetAllInsultsAsync()
                .ConfigureAwait(false);

            if (insults == null || !insults.Any())
                throw new CommandFailedException("No insults registered.");

            await ctx.SendPaginatedCollectionAsync(
                "Available insults",
                insults.Values,
                i => Formatter.Italic(i),
                DiscordColor.Green
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
