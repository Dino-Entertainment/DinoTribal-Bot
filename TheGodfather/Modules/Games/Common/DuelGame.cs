﻿#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class DuelGame : ChannelEvent
    {
        public string FinishingMove { get; private set; }

        private int hp1;
        private int hp2;
        private string hp1str;
        private string hp2str;
        private bool potionUsed1;
        private bool potionUsed2;
        private DiscordMessage messageHandle;
        private readonly DiscordUser player1;
        private readonly DiscordUser player2;
        private readonly StringBuilder eb;


        public DuelGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2)
            : base(interactivity, channel)
        {
            this.player1 = p1;
            this.player2 = p2;
            this.hp1 = 5;
            this.hp2 = 5;
            this.potionUsed1 = false;
            this.potionUsed2 = false;
            this.eb = new StringBuilder();
        }
        

        public override async Task RunAsync()
        {
            UpdateHpBars();

            this.messageHandle = await this.Channel.EmbedAsync($"{this.player1.Mention} {this.hp1str} {StaticDiscordEmoji.DuelSwords} {this.hp2str} {this.player2.Mention}");

            while (this.hp1 > 0 && this.hp2 > 0)
                await AdvanceAsync();

            this.Winner = this.hp1 > 0 ? this.player1 : this.player2;

            await this.Channel.EmbedAsync($"{StaticDiscordEmoji.DuelSwords} {this.Winner.Mention}, FINISH HIM! {StaticDiscordEmoji.DuelSwords}");
            this.FinishingMove = await WaitForFinishingMoveAsync();
        }

        private async Task AdvanceAsync()
        {
            DealDamage();
            await WaitForPotionUseAsync();
            UpdateHpBars();

            this.messageHandle = await this.messageHandle.ModifyAsync($"{this.player1.Mention} {this.hp1str} {StaticDiscordEmoji.DuelSwords} {this.hp2str} {this.player2.Mention}", embed: new DiscordEmbedBuilder() {
                Title = "ITS TIME TO DUDUDUDU... DUEL!",
                Description = this.eb.ToString(),
                Color = DiscordColor.Teal
            }.Build());
        }

        private void UpdateHpBars()
        {
            this.hp1str = string.Join("", Enumerable.Repeat(StaticDiscordEmoji.WhiteSquare, this.hp1)) + string.Join("", Enumerable.Repeat(StaticDiscordEmoji.BlackSquare, 5 - this.hp1));
            this.hp2str = string.Join("", Enumerable.Repeat(StaticDiscordEmoji.BlackSquare, 5 - this.hp2)) + string.Join("", Enumerable.Repeat(StaticDiscordEmoji.WhiteSquare, this.hp2));
        }

        private void DealDamage()
        {
            int damage = 1;
            if (GFRandom.Generator.GetBool()) {
                this.eb.AppendLine($"{this.player1.Username} {StaticDiscordEmoji.GetRandomDuelWeapon()} {this.player2.Username}");
                this.hp2 -= damage;
            } else {
                this.eb.AppendLine($"{this.player2.Username} {StaticDiscordEmoji.GetRandomDuelWeapon()} {this.player1.Username}");
                this.hp1 -= damage;
            }
        }

        private async Task WaitForPotionUseAsync()
        {
            MessageContext mctx = await this.Interactivity.WaitForMessageAsync(
                msg => {
                    if (msg.ChannelId != this.Channel.Id) return false;
                    if (msg.Content.ToLowerInvariant() != "hp") return false;
                    if (!this.potionUsed1 && msg.Author.Id == this.player1.Id) return true; 
                    if (!this.potionUsed2 && msg.Author.Id == this.player2.Id) return true;
                    return false;
                },
                TimeSpan.FromSeconds(2)
            );
            if (mctx != null) {
                if (mctx.User.Id == this.player1.Id) {
                    this.hp1 = (this.hp1 + 1 > 5) ? 5 : this.hp1 + 1;
                    this.potionUsed1 = true;
                    this.eb.AppendLine($"{this.player1.Username} {StaticDiscordEmoji.Syringe}");
                } else {
                    this.hp2 = (this.hp2 + 1 > 5) ? 5 : this.hp2 + 1;
                    this.potionUsed2 = true;
                    this.eb.AppendLine($"{this.player2.Username} {StaticDiscordEmoji.Syringe}");
                }
            }
        }

        private async Task<string> WaitForFinishingMoveAsync()
        {
            MessageContext mctx = await this.Interactivity.WaitForMessageAsync(
                m => m.ChannelId == this.Channel.Id && m.Author.Id == this.Winner.Id
            );

            if (mctx != null && !string.IsNullOrWhiteSpace(mctx.Message?.Content))
                return mctx.Message.Content.Trim();
            else
                return null;
        }
    }
}


