﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Description("Random number generation commands.")]
    public class CommandsGamble
    {
        #region COMMAND_8BALL
        [Command("8ball"), Description("An almighty ball which knows answer to everything.")]
        [Aliases("question")]
        public async Task EightBall(CommandContext ctx, [RemainingText, Description("A question for the almighty ball.")] string q = null)
        {
            if (q == null || (q = q.Trim()) == "") {
                await ctx.RespondAsync("The almighty ball requires a question.");
                return;
            }

            if (q[q.Length - 1] != '?') {
                await ctx.RespondAsync("That doesn't seem like a question...");
                return;
            }

            string[] answers = {
                "Yes.",
                "Possibly.",
                "No.",
                "Maybe.",
                "Definitely.",
                "Perhaps.",
                "More than you can imagine.",
                "Definitely not."
            };

            var rnd = new Random();
            await ctx.RespondAsync(answers[rnd.Next(0, answers.Length)]);
        }
        #endregion

        #region COMMAND_DUEL
        [Command("duel"), Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs")]
        public async Task Duel(CommandContext ctx, [Description("Who to fight")] DiscordUser u)
        {
            if (u.Id == ctx.User.Id) {
                await ctx.RespondAsync("You can't duel yourself...");
                return;
            }

            string[] weapons = { "sword", "axe", "keyboard", "stone", "cheeseburger", "belt from yo momma" };

            await ctx.RespondAsync($"Duel between {ctx.User.Mention} and {u.Mention} is about to start!");

            int hp1 = 100, hp2 = 100;
            var rnd = new Random();
            while (hp1 > 0 && hp2 > 0) {
                await ctx.RespondAsync($"HP: {ctx.User.Username} ({hp1}) : {u.Username} ({hp2})");
                await Task.Delay(2000);
                int damage = rnd.Next(20, 40);
                if (rnd.Next() % 2 == 0) {
                    await ctx.RespondAsync($"{ctx.User.Username} hits {u.Username} with a {weapons[rnd.Next(0, weapons.Length)]} for {damage} damage!");
                    hp2 -= damage;
                } else {
                    await ctx.RespondAsync($"{u.Username} hits {ctx.User.Username} with a {weapons[rnd.Next(0, weapons.Length)]} for {damage} damage!");
                    hp1 -= damage;
                }
                await Task.Delay(2000);
            }
            if (hp1 < 0)
                await ctx.RespondAsync($"{u.Mention} wins!");
            else
                await ctx.RespondAsync($"{ctx.User.Mention} wins!");
        }
        #endregion

        #region COMMAND_ROLL
        [Command("roll"), Description("Rolls a dice.")]
        [Aliases("dice")]
        public async Task Roll(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a {rnd.Next(1, 7)} !");
        }
        #endregion

        #region COMMAND_RPS
        [Command("rps"), Description("Rock, paper, scissors game.")]
        [Aliases("rockpaperscissors")]
        public async Task RPS(CommandContext ctx)
        {
            await ctx.RespondAsync("Get ready!");
            for (int i = 3; i > 0; i--) {
                await ctx.RespondAsync(i + "...");
                await Task.Delay(1000);
            }
            
            var rnd = new Random();
            switch (rnd.Next(0, 3)) {
                case 0: await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":new_moon:")}"); break;
                case 1: await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":newspaper:")}"); break;
                case 2: await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":scissors:")}"); break;
            }
        }
        #endregion

        #region COMMAND_SLOT
        [Command("slot"), Description("Roll a slot machine for 5 credits")]
        [Aliases("slotmachine")]
        public async Task SlotMachine(CommandContext ctx)
        {
            if (!CommandsBank.RetrieveCreditsSucceeded(ctx.User.Id, 5)) {
                await ctx.RespondAsync("You do not have enough credits in WM bank!");
                return;
            }

            DiscordEmoji[] emoji = {
                DiscordEmoji.FromName(ctx.Client, ":peach:"),
                DiscordEmoji.FromName(ctx.Client, ":moneybag:"),
                DiscordEmoji.FromName(ctx.Client, ":gift:"),
                DiscordEmoji.FromName(ctx.Client, ":large_blue_diamond:"),
                DiscordEmoji.FromName(ctx.Client, ":seven:"),
                DiscordEmoji.FromName(ctx.Client, ":cherries:")
            };

            var rnd = new Random();
            string result = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    result += emoji[rnd.Next(0, emoji.Length)].ToString();
                result += '\n';
            }

            int won = Evaluate(result);

            var embed = new DiscordEmbed() {
                Title = "TOTALLY NOT RIGGED SLOT MACHINE",
                Description = result,
                Color = 0xFFFF00    // Yellow
            };
            var res = new DiscordEmbedField() {
                Name = "Result: ",
                Value = $"You won {won} credits!"
            };
            embed.Fields.Add(res);

            await ctx.RespondAsync("", embed: embed);

            if (won > 0)
                CommandsBank.IncreaseBalance(ctx.User.Id, won);
        }
        #endregion

        #region HELPER_FUNCTIONS
        private int Evaluate(string slotres)
        {
            return 5;
        }
        #endregion
    }

    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("$$$")]
    public class CommandsBank
    {
        #region STATIC_FIELDS
        private static Dictionary<ulong, int> _accounts = new Dictionary<ulong, int>();
        #endregion

        public async Task ExecuteGroup(CommandContext ctx)
        {
            await Status(ctx);
        }

        #region COMMAND_REGISTER
        [Command("register")]
        [Aliases("r", "signup", "activate")]
        public async Task Register(CommandContext ctx)
        {
            if (_accounts.ContainsKey(ctx.User.Id)) {
                await ctx.RespondAsync("You already own an account in WM bank!");
            } else {
                _accounts.Add(ctx.User.Id, 25);
                await ctx.RespondAsync("Account opened! Since WM bank is so generous, you get 100 credits for free.");
            }
        }
        #endregion

        #region COMMAND_STATUS
        [Command("status")]
        [Aliases("s", "balance")]
        public async Task Status(CommandContext ctx)
        {
            int ammount = 0;
            if (_accounts.ContainsKey(ctx.User.Id))
                ammount = _accounts[ctx.User.Id];

            var embed = new DiscordEmbed() {
                Title = "Account balance for " + ctx.User.Username,
                Timestamp = DateTime.Now,
                Color = 0xFFFF00    // Yellow
            };
            var balance = new DiscordEmbedField() {
                Name = "Balance: ",
                Value = ammount.ToString()
            };
            embed.Fields.Add(balance);
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region HELPER_FUNCTIONS
        public static bool RetrieveCreditsSucceeded(ulong id, int ammount)
        {
            if (!_accounts.ContainsKey(id) || _accounts[id] < ammount)
                return false;
            _accounts[id] += ammount;
            return true;
        }

        public static void IncreaseBalance(ulong id, int ammount)
        {
            if (!_accounts.ContainsKey(id))
                _accounts.Add(id, 0);
            _accounts[id] += ammount;
        }
        #endregion
    }
}
