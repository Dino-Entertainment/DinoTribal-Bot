﻿#region USING_DIRECTIVES
using System;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("rss", CanInvokeWithoutSubcommand = true)]
    [Description("RSS feed operations.")]
    public class CommandsRSS
    {
        public async Task ExecuteGroup(CommandContext ctx, [RemainingText, Description("URL")] string url = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                await WMRSS(ctx);
            else
                await RSSFeedRead(ctx, url);
        }

        #region COMMAND_WM
        [Command("wm"), Description("Get newest topics from WM forum.")]
        public async Task WMRSS(CommandContext ctx)
        {
            await RSSFeedRead(ctx, "http://worldmafia.net/forum/forums/-/index.rss");
        }
        #endregion

        #region COMMAND_NEWS
        [Command("news"), Description("Get newest world news.")]
        public async Task NewsRSS(CommandContext ctx)
        {
            await RSSFeedRead(ctx, "https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en");
        }
        #endregion

        #region HELPER_FUNCTIONS
        private async Task RSSFeedRead(CommandContext ctx, string url)
        {
            SyndicationFeed feed = null;
            try {
                XmlReader reader = XmlReader.Create(url);
                feed = SyndicationFeed.Load(reader);
                reader.Close();
            } catch (Exception) {
                await ctx.RespondAsync("Error getting RSS feed from " + url);
                return;
            }

            var embed = new DiscordEmbed() {
                Title = "Topics active recently",
                Color = 0x00FF00    // Green
            };

            int count = 5;
            foreach (SyndicationItem item in feed.Items) {
                if (count-- == 0)
                    break;
                var field = new DiscordEmbedField() {
                    Name = item.Title.Text,
                    Value = item.Links[0].Uri.ToString(),
                };
                embed.Fields.Add(field);
            }

            await ctx.RespondAsync("", embed: embed);
        }
        #endregion
    }
}
