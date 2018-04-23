﻿#region USING_DIRECTIVES
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("gamble"), Module(ModuleType.Gambling)]
    [Description("Betting and gambling commands.")]
    [Aliases("bet")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class GambleModule : TheGodfatherBaseModule
    {

        public GambleModule(DBService db) : base(db: db) { }


        #region COMMAND_GAMBLE_COINFLIP
        [Command("coinflip"), Priority(1)]
        [Module(ModuleType.Gambling)]
        [Description("Flip a coin and bet on the outcome.")]
        [Aliases("coin", "flip")]
        [UsageExample("!bet coinflip 10 heads")]
        [UsageExample("!bet coinflip tails 20")]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("Bid.")] int bid,
                                       [Description("Heads/Tails (h/t).")] string bet)
        {
            if (bid <= 0 || bid > 10000)
                throw new InvalidCommandUsageException("Invalid bid amount! Needs to be in range [0, 10000]");

            if (string.IsNullOrWhiteSpace(bet))
                throw new InvalidCommandUsageException("Missing heads or tails call.");
            bet = bet.ToLowerInvariant();

            bool guess;
            if (bet == "heads" || bet == "head" || bet == "h")
                guess = true;
            else if (bet == "tails" || bet == "tail" || bet == "t")
                guess = false;
            else
                throw new CommandFailedException($"Invalid coin outcome call (has to be {Formatter.Bold("heads")} or {Formatter.Bold("tails")})");

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            bool rnd = GFRandom.Generator.GetBool();

            StringBuilder sb = new StringBuilder();
            sb.Append(ctx.User.Mention)
              .Append(" flipped ")
              .Append(Formatter.Bold(rnd ? "Heads" : "Tails"))
              .Append(" and ")
              .Append(guess == rnd ? "won " : "lost ")
              .Append(Formatter.Bold(bid.ToString()))
              .Append(" credits!");

            if (rnd == guess)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, bid * 2)
                    .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(sb.ToString(), ":game_die:")
                .ConfigureAwait(false);
        }

        [Command("coinflip"), Priority(0)]
        public Task CoinflipAsync(CommandContext ctx,
                                 [Description("Heads/Tails (h/t).")] string bet,
                                 [Description("Bid.")] int bid)
            => CoinflipAsync(ctx, bid, bet);
        #endregion

        #region COMMAND_GAMBLE_DICE
        [Command("dice"), Priority(1)]
        [Module(ModuleType.Gambling)]
        [Description("Roll a dice and bet on the outcome.")]
        [Aliases("roll", "die")]
        [UsageExample("!bet dice 50 six")]
        [UsageExample("!bet dice three 10")]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("Bid.")] int bid,
                                       [Description("Number guess (has to be a word one-six).")] string guess)
        {
            if (bid <= 0 || bid > 10000)
                throw new InvalidCommandUsageException("Invalid bid amount! Needs to be in range [0, 10000]");

            if (string.IsNullOrWhiteSpace(guess))
                throw new InvalidCommandUsageException("Missing guess number.");
            guess = guess.ToLowerInvariant();

            int guess_int = 0;
            switch (guess) {
                case "one": guess_int = 1; break;
                case "two": guess_int = 2; break;
                case "three": guess_int = 3; break;
                case "four": guess_int = 4; break;
                case "five": guess_int = 5; break;
                case "six": guess_int = 6; break;
                default:
                    throw new CommandFailedException($"Invalid guess. Has to be a number from {Formatter.Bold("one")} to {Formatter.Bold("six")})");
            }

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            int rnd = GFRandom.Generator.Next(1, 7);

            StringBuilder sb = new StringBuilder();
            sb.Append(ctx.User.Mention)
              .Append(" rolled a ")
              .Append(Formatter.Bold(rnd.ToString()))
              .Append(" and ")
              .Append(guess_int == rnd ? $"won {Formatter.Bold((bid * 5).ToString())}" : $"lost {Formatter.Bold((bid).ToString())}")
              .Append(" credits!");

            await ctx.RespondWithIconEmbedAsync(sb.ToString(), ":game_die:")
                .ConfigureAwait(false);

            if (rnd == guess_int)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, bid * 6)
                    .ConfigureAwait(false);
        }

        [Command("dice"), Priority(0)]
        public Task RollDiceAsync(CommandContext ctx,
                                 [Description("Number guess (has to be a word one-six).")] string guess,
                                 [Description("Bid.")] int bid)
            => RollDiceAsync(ctx, bid, guess);
        #endregion
    }
}
