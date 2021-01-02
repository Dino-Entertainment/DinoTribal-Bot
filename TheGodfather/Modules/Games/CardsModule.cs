﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TexasHoldem.Logic.Cards;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Modules.Games
{
    [Group("cards"), Module(ModuleType.Games), NotBlocked]
    [Description("Miscellaneous playing card commands.")]
    [Aliases("deck")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class CardsModule : TheGodfatherModule
    {
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ResetDeckAsync(ctx);


        #region COMMAND_DECK_DRAW
        [Command("draw")]
        [Description("Draw cards from the top of the deck. If amount of cards is not specified, draws one card.")]
        [Aliases("take")]

        public Task DrawAsync(CommandContext ctx,
                                   [Description("Amount (in range [1, 10]).")] int amount = 1)
        {
            Deck deck = CardDecksService.GetDeckForChannel(ctx.Channel.Id);
            if (deck is null)
                throw new CommandFailedException($"No deck to deal from. Use command {Formatter.InlineCode("deck")} to open a new deck.");

            if (amount is < 1 or > 10)
                throw new InvalidCommandUsageException("Amount of cards to draw must be in range [1, 10].");

            IReadOnlyList<Card> drawn = deck.DrawCards(amount);
            if (!drawn.Any())
                throw new CommandFailedException($"Current deck doesn't have enough cards. Use command {Formatter.InlineCode("deck reset")} to open a new deck.");

            return this.InformAsync(ctx, $"{ctx.User.Mention} drew {string.Join(" ", drawn)}", ":ticket:");
        }
        #endregion

        #region COMMAND_DECK_RESET
        [Command("reset")]
        [Description("Opens a brand new card deck.")]
        [Aliases("new", "opennew", "open")]
        public Task ResetDeckAsync(CommandContext ctx)
        {
            CardDecksService.ResetDeckForChannel(ctx.Channel.Id);
            return this.InformAsync(ctx, Emojis.Cards.Suits[0], "A new shuffled deck is opened in this channel!");
        }
        #endregion
    }
}
