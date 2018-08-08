﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TexasHoldem.Logic.Cards;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games
{
    [Group("cards"), Module(ModuleType.Games), NotBlocked]
    [Description("Miscellaneous playing card commands.")]
    [Aliases("deck")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class CardsModule : TheGodfatherModule
    {

        public CardsModule(SharedData shared) 
            : base(shared: shared)
        {
            this.ModuleColor = DiscordColor.Blurple;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ResetDeckAsync(ctx);


        #region COMMAND_DECK_DRAW
        [Command("draw")]
        [Description("Draw cards from the top of the deck. If amount of cards is not specified, draws one card.")]
        [Aliases("take")]
        [UsageExamples("!deck draw 5")]
        public Task DrawAsync(CommandContext ctx,
                                   [Description("Amount (in range [1, 10]).")] int amount = 1)
        {
            if (!this.Shared.CardDecks.ContainsKey(ctx.Channel.Id) || this.Shared.CardDecks[ctx.Channel.Id] == null)
                throw new CommandFailedException($"No deck to deal from. Use command {Formatter.InlineCode("deck")} to open a new deck.");

            Deck deck = this.Shared.CardDecks[ctx.Channel.Id];
            
            if (amount < 1|| amount > 10)
                throw new InvalidCommandUsageException("Amount of cards to draw must be in range [1, 10].");

            IReadOnlyList<Card> drawn = deck.DrawCards(amount);
            if (!drawn.Any())
                throw new CommandFailedException($"Current deck doesn't have enough cards. Use command {Formatter.InlineCode("deck reset")} to open a new deck.");

            return InformAsync(ctx, $"{ctx.User.Mention} drew {string.Join(" ", drawn)}", ":ticket:");
        }
        #endregion

        #region COMMAND_DECK_RESET
        [Command("reset")]
        [Description("Opens a brand new card deck.")]
        [Aliases("new", "opennew", "open")]
        [UsageExamples("!deck reset")]
        public Task ResetDeckAsync(CommandContext ctx)
        {
            this.Shared.CardDecks.AddOrUpdate(ctx.Channel.Id, new Deck(), (cid, deck) => new Deck());
            return InformAsync(ctx, StaticDiscordEmoji.CardSuits[0], "A new shuffled deck is opened in this channel!");
        }
        #endregion
    }
}
