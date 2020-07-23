﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class ChannelModule : TheGodfatherModule
    {
        [Group("webhooks")]
        [Description("Manage webhooks for given channel. Group call lists all existing webhooks in channel.")]
        [Aliases("wh", "webhook", "whook")]

        [RequirePermissions(Permissions.ManageWebhooks)]
        public class WebhooksModule : TheGodfatherModule
        {
            public WebhooksModule(DbContextBuilder db)
                : base(db)
            {

            }


            [GroupCommand]
            public Task ExecuteGroupCommandAsync(CommandContext ctx,
                                                [Description("Channel to list webhooks for.")] DiscordChannel channel = null)
                => this.ListAsync(ctx, channel);


            #region COMMAND_CHANNEL_WEBHOOKS_ADD
            [Command("add"), Priority(1)]
            [Description("Create a new webhook in channel.")]
            [Aliases("a", "c", "+", "+=", "create", "<<", "<")]

            public async Task CreateAsync(CommandContext ctx,
                                         [Description("Channel to list webhooks for.")] DiscordChannel channel,
                                         [Description("Name.")] string name,
                                         [Description("Avatar URL.")] Uri avatarUrl = null,
                                         [RemainingText, Description("Reason.")] string reason = null)
            {
                DiscordWebhook wh;
                if (avatarUrl is null)
                    wh = await channel.CreateWebhookAsync(name, reason: ctx.BuildInvocationDetailsString(reason));
                else {
                    try {
                        using (MemoryStream ms = await HttpService.GetMemoryStreamAsync(avatarUrl))
                            wh = await channel.CreateWebhookAsync(name, ms, reason: ctx.BuildInvocationDetailsString(reason));
                    } catch (WebException e) {
                        throw new CommandFailedException("Failed to fetch the image!", e);
                    }
                }

                await this.InformAsync(ctx, "Created a new webhook! Sending you the token in private...", important: false);
                try {
                    DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                    if (dm is null)
                        throw new CommandFailedException("I failed to send you the token in private.");
                    await dm.SendMessageAsync($"Token for webhook {Formatter.Bold(wh.Name)} in {Formatter.Bold(ctx.Guild.ToString())}, {Formatter.Bold(channel.ToString())}: {Formatter.BlockCode(wh.Token)}\nWebhook URL: {wh.BuildUrlString()}");
                } catch {
                    // Failed to create DM or insufficient perms to send
                }
            }

            [Command("add"), Priority(0)]
            public Task CreateAsync(CommandContext ctx,
                                   [Description("Name.")] string name,
                                   [Description("Avatar URL.")] Uri avatarUrl = null,
                                   [RemainingText, Description("Reason.")] string reason = null)
                => this.CreateAsync(ctx, ctx.Channel, name, avatarUrl, reason);
            #endregion

            #region COMMAND_CHANNEL_WEBHOOKS_DELETE
            [Command("delete")]
            [Description("Create a new webhook in channel.")]
            [Aliases("-", "del", "d", "remove", "rm", ">>", ">")]

            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name,
                                         [Description("Channel to list webhooks for.")] DiscordChannel channel = null)
            {
                channel = channel ?? ctx.Channel;

                IEnumerable<DiscordWebhook> whs = await channel.GetWebhooksAsync();
                DiscordWebhook wh = whs.SingleOrDefault(w => w.Name.ToLowerInvariant() == name.ToLowerInvariant());
                if (wh is null)
                    throw new CommandFailedException($"Webhook with name {Formatter.InlineCode(name)} does not exist!");

                await wh.DeleteAsync();
                await this.InformAsync(ctx, $"Successfully deleted webhook {Formatter.InlineCode(wh.Name)}!", important: false);
            }
            #endregion

            #region COMMAND_CHANNEL_WEBHOOKS_DELETEALL
            [Command("deleteall"), UsesInteractivity]
            [Description("Create a new webhook in channel.")]
            [Aliases("-a", "clear", "delall", "da", "removeall", "rmrf", ">>>")]

            public async Task DeleteAllAsync(CommandContext ctx,
                                            [Description("Channel to list webhooks for.")] DiscordChannel channel = null)
            {
                channel = channel ?? ctx.Channel;

                IReadOnlyList<DiscordWebhook> whs = await channel.GetWebhooksAsync();
                if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to delete {Formatter.Bold("ALL")} of the webhooks ({Formatter.Bold(whs.Count.ToString())} total)?"))
                    return;

                await Task.WhenAll(whs.Select(w => w.DeleteAsync()));
                await this.InformAsync(ctx, $"Successfully deleted all webhooks!", important: false);
            }
            #endregion

            #region COMMAND_CHANNEL_WEBHOOKS_LIST
            [Command("list"), UsesInteractivity]
            [Description("Lists all existing webhooks in channel.")]
            [Aliases("l", "ls")]

            public async Task ListAsync(CommandContext ctx,
                                       [Description("Channel to list webhooks for.")] DiscordChannel channel = null)
            {
                channel = channel ?? ctx.Channel;
                IReadOnlyList<DiscordWebhook> whs = await channel.GetWebhooksAsync();

                if (!whs.Any())
                    throw new CommandFailedException("There are no webhooks in this channel.");

                bool displayToken = await ctx.WaitForBoolReplyAsync("Do you wish to display the tokens?", reply: false);

                await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel,
                    ctx.User,
                    whs.Select(wh => new Page(embed: new DiscordEmbedBuilder {
                        Title = $"Webhook: {wh.Name}",
                        Description = $"{(displayToken ? $"Token: {Formatter.InlineCode(wh.Token)}\n" : "")}Created by {wh.User.ToString()}",
                        Color = this.ModuleColor,
                        ThumbnailUrl = wh.AvatarUrl,
                        Timestamp = wh.CreationTimestamp,
                    }.AddField("URL", displayToken ? wh.BuildUrlString() : "Hidden")))
                );
            }
            #endregion
        }
    }
}
