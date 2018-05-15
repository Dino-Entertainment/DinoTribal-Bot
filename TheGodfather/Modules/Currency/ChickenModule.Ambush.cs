﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class ChickenModule
    {
        [Group("ambush"), Module(ModuleType.Gambling)]
        [Description("Start an ambush for another user's chicken. Other users can put their chickens into your ambush and collectively attack the target chicken combining their strength.")]
        [Aliases("gangattack")]
        [UsageExample("!chicken ambush @Someone")]
        public class AmbushModule : TheGodfatherBaseModule
        {

            public AmbushModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Whose chicken to ambush.")] DiscordUser user = null)
            {
                if (user == null)
                    throw new InvalidCommandUsageException("You need to specify a user whose chicken you want to ambush!");

                var ambushed = await Database.GetChickenInfoAsync(user.Id)
                    .ConfigureAwait(false);
                if (ambushed == null)
                    throw new CommandFailedException("Given user does not have a chicken!");

                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenAmbush)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }
                
                var ambush = new ChickenAmbush(ctx.Client.GetInteractivity(), ctx.Channel, ambushed);
                ChannelEvent.RegisterEventInChannel(ambush, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync($"The ambush will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("chicken ambussh")} to make your chicken join the ambush.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    await ambush.RunAsync()
                        .ConfigureAwait(false);

                    if (ambush.AmbushedChickenSurvived) {
                        ambushed.Strength += 20;
                        await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(ambushed.Name)} survived the ambush and slaughtered all the other chickens! (+20 STR)\n\n{user.Mention} won 100000 credits!")
                            .ConfigureAwait(false);
                        await Database.GiveCreditsToUserAsync(ambushed.OwnerId, 100000)
                            .ConfigureAwait(false);

                        foreach (var chicken in ambush.Ambushers)
                            await Database.RemoveChickenAsync(chicken.OwnerId).ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(ambushed.Name)} is no more...")
                            .ConfigureAwait(false);
                        await Database.RemoveChickenAsync(user.Id)
                            .ConfigureAwait(false);
                    }
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CHICKEN_AMBUSH_JOIN
            [Command("join"), Module(ModuleType.Gambling)]
            [Description("Join a pending chicken ambush.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!chicken ambush join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenAmbush ambush))
                    throw new CommandFailedException("There are no ambushes running in this channel.");

                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ambush.Started)
                    throw new CommandFailedException("Ambush has already started, you can't join it.");

                if (ambush.AmbusherCount >= 10)
                    throw new CommandFailedException("Ambush slots are full (max 10 chickens). It has to be a BIT fair...");
                
                if (ambush.IsParticipating(ctx.User))
                    throw new CommandFailedException("Your chicken is already participating in the ambush!");

                ambush.AddParticipant(chicken, ctx.User);
                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} joined the ambush.")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
