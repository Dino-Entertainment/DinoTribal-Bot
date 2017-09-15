﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("race", CanInvokeWithoutSubcommand = true)]
    [Description("Racing!")]
    public class CommandsRace
    {
        #region PRIVATE_FIELDS
        private ConcurrentDictionary<ulong, ConcurrentQueue<ulong>> _participants = new ConcurrentDictionary<ulong, ConcurrentQueue<ulong>>();
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, string>> _emojis = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, string>>();
        private ConcurrentDictionary<ulong, ConcurrentBag<string>> _animals = new ConcurrentDictionary<ulong, ConcurrentBag<string>>();
        private ConcurrentDictionary<ulong, bool> _started = new ConcurrentDictionary<ulong, bool>();
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await NewRace(ctx);
        }



        #region COMMAND_RACE_NEW
        [Command("new"), Description("Start a new race.")]
        [Aliases("create")]
        public async Task NewRace(CommandContext ctx)
        {
            if (_participants.ContainsKey(ctx.Channel.Id))
                throw new Exception("Race already in progress!");

            _animals.TryAdd(ctx.Channel.Id, new ConcurrentBag<string> {
                    ":dog:", ":cat:", ":mouse:", ":hamster:", ":rabbit:", ":bear:", ":pig:", ":cow:", ":koala:", ":tiger:"
                });
            _participants.TryAdd(ctx.Channel.Id, new ConcurrentQueue<ulong>());
            _emojis.TryAdd(ctx.Channel.Id, new ConcurrentDictionary<ulong, string>());
            _started.TryAdd(ctx.Channel.Id, false);

            await ctx.RespondAsync("Race will start in 30s or when there are 10 participants. Type ``!race join`` to join the race.");
            await Task.Delay(30000);

            if (_participants[ctx.Channel.Id].Count > 1)
                await StartRace(ctx);
            else {
                await ctx.RespondAsync("Not enough users joined the race.");
                StopRace(ctx);
            }
        }
        #endregion

        #region COMMAND_RACE_JOIN
        [Command("join"), Description("Join a race.")]
        [Aliases("+", "compete")]
        public async Task JoinRace(CommandContext ctx)
        {
            if (!_participants.ContainsKey(ctx.Channel.Id))
                throw new Exception("There is no race in this channel!");

            if (_participants[ctx.Channel.Id].Any(id => id == ctx.User.Id))
                throw new Exception("You are already participating in the race!");

            if (_started[ctx.Channel.Id])
                throw new Exception("Race already started, you can't join it.");

            if (_participants[ctx.Channel.Id].Count >= 10)
                throw new Exception("Race full.");

            var rnd = new Random();
            string emoji;
            _animals[ctx.Channel.Id].TryTake(out emoji);
            _participants[ctx.Channel.Id].Enqueue(ctx.User.Id);
            _emojis[ctx.Channel.Id].TryAdd(ctx.User.Id, emoji);

            await ctx.RespondAsync($"{ctx.User.Mention} joined the race as {DiscordEmoji.FromName(ctx.Client, emoji)}");
        }
        #endregion
        
        #region COMMAND_RACE_STOP
        [Command("stop"), Description("Stops a running race.")]
        [Aliases("cancel")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task CancelRace(CommandContext ctx)
        {
            StopRace(ctx);
            await ctx.RespondAsync("Race cancelled.");
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task StartRace(CommandContext ctx)
        {
            _started[ctx.Channel.Id] = true;

            Dictionary<ulong, int> progress = new Dictionary<ulong, int>();
            foreach (var p in _participants[ctx.Channel.Id])
                progress.Add(p, 0);

            var msg = await ctx.RespondAsync("Race starting...");
            var rnd = new Random((int)DateTime.Now.Ticks);
            while (!progress.Any(e => e.Value >= 100)) {
                await PrintRace(ctx, progress, msg);

                foreach (var id in _participants[ctx.Channel.Id]) {
                    progress[id] += rnd.Next(2, 5);
                    if (progress[id] > 100)
                        progress[id] = 100;
                }

                await Task.Delay(2000);
            }
            await PrintRace(ctx, progress, msg);

            await ctx.RespondAsync("Race ended!");
            StopRace(ctx);
        }

        private void StopRace(CommandContext ctx)
        {
            ConcurrentQueue<ulong> outl;
            _participants.TryRemove(ctx.Channel.Id, out outl);
            ConcurrentBag<string> outbag;
            _animals.TryRemove(ctx.Channel.Id, out outbag);
            bool outb;
            _started.TryRemove(ctx.Channel.Id, out outb);
        }

        private async Task PrintRace(CommandContext ctx, Dictionary<ulong, int> progress, DiscordMessage msg)
        {
            string s = "LIVE RACING BROADCAST\n| 🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n";
            foreach (var id in _participants[ctx.Channel.Id]) {
                var participant = await ctx.Guild.GetMemberAsync(id);
                s += "|";
                for (int p = progress[id]; p > 0; p--)
                    s += "‣";
                s += DiscordEmoji.FromName(ctx.Client, _emojis[ctx.Channel.Id][id]);
                for (int p = 100 - progress[id]; p > 0; p--)
                    s += "‣";
                s += "| " + participant.Mention;
                if (progress[id] == 100)
                    s += " " + DiscordEmoji.FromName(ctx.Client, ":trophy:");
                s += '\n';
            }
            await msg.ModifyAsync(s);
        }
        #endregion
    }
}
