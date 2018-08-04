﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Bank;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("gamble"), Module(ModuleType.Currency), NotBlocked]
    [Description("Betting and gambling commands.")]
    [Aliases("bet")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class GambleModule : TheGodfatherModule
    {

        public GambleModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.SapGreen;
        }


        #region COMMAND_GAMBLE_COINFLIP
        [Command("coinflip"), Priority(1)]
        [Description("Flip a coin and bet on the outcome.")]
        [Aliases("coin", "flip")]
        [UsageExamples("!bet coinflip 10 heads",
                       "!bet coinflip tails 20")]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("Bid.")] long bid,
                                       [Description("Heads/Tails (h/t).")] string bet)
        {
            if (bid <= 0 || bid > 1000000000L)
                throw new InvalidCommandUsageException($"Invalid bid amount! Needs to be in range [1, {1000000000:n0}]");

            if (string.IsNullOrWhiteSpace(bet))
                throw new InvalidCommandUsageException("Missing heads or tails call.");
            bet = bet.ToLowerInvariant();

            bool guess;
            switch (bet) {
                case "heads":
                case "head":
                case "h":
                    guess = true;
                    break;
                case "tails":
                case "tail":
                case "t":
                    guess = false;
                    break;
                default:
                    throw new CommandFailedException($"Invalid coin outcome call (has to be {Formatter.Bold("heads")} or {Formatter.Bold("tails")})");
            }

            if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid))
                throw new CommandFailedException($"You do not have enough credits! Use command {Formatter.InlineCode("bank")} to check your account status.");

            bool rnd = GFRandom.Generator.GetBool();

            var sb = new StringBuilder();
            sb.Append(ctx.User.Mention);
            sb.Append(" flipped ");
            sb.Append(Formatter.Bold(rnd ? "Heads" : "Tails"));
            sb.Append(" and ");
            sb.Append(guess == rnd ? "won " : "lost ");
            sb.Append(Formatter.Bold(bid.ToString()));
            sb.Append(" credits!");

            if (rnd == guess)
                await this.Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid * 2);

            await InformAsync(ctx, StaticDiscordEmoji.Dice, sb.ToString());
        }

        [Command("coinflip"), Priority(0)]
        public Task CoinflipAsync(CommandContext ctx,
                                 [Description("Heads/Tails (h/t).")] string bet,
                                 [Description("Bid.")] long bid)
            => CoinflipAsync(ctx, bid, bet);
        #endregion

        #region COMMAND_GAMBLE_DICE
        [Command("dice"), Priority(1)]
        [Description("Roll a dice and bet on the outcome.")]
        [Aliases("roll", "die")]
        [UsageExamples("!bet dice 50 six",
                       "!bet dice three 10")]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("Bid.")] long bid,
                                       [Description("Number guess (has to be a word one-six).")] string guess)
        {
            if (bid <= 0 || bid > 1000000000L)
                throw new InvalidCommandUsageException($"Invalid bid amount! Needs to be in range [1, {1000000000:n0}]");

            if (string.IsNullOrWhiteSpace(guess))
                throw new InvalidCommandUsageException("Missing guess number.");
            guess = guess.ToLowerInvariant();

            int guess_int;
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

            if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid))
                throw new CommandFailedException($"You do not have enough credits! Use command {Formatter.InlineCode("bank")} to check your account status.");

            int rnd = GFRandom.Generator.Next(1, 7);

            var sb = new StringBuilder();
            sb.Append(ctx.User.Mention);
            sb.Append(" rolled a ");
            sb.Append(Formatter.Bold(rnd.ToString()));
            sb.Append(" and ");
            sb.Append(guess_int == rnd ? $"won {Formatter.Bold((bid * 5).ToString())}" : $"lost {Formatter.Bold(bid.ToString())}");
            sb.Append(" credits!");

            await InformAsync(ctx, StaticDiscordEmoji.Dice, sb.ToString());

            if (rnd == guess_int)
                await this.Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid * 6);
        }

        [Command("dice"), Priority(0)]
        public Task RollDiceAsync(CommandContext ctx,
                                 [Description("Number guess (has to be a word one-six).")] string guess,
                                 [Description("Bid.")] long bid)
            => RollDiceAsync(ctx, bid, guess);
        #endregion
    }
}
