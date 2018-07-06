﻿#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal class NotBlockedAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var shared = ctx.Services.GetService<SharedData>();
            if (shared.ListeningStatus) {
                if (shared.BlockedUsers.Contains(ctx.User.Id) || shared.BlockedChannels.Contains(ctx.Channel.Id))
                    return Task.FromResult(false);

                if (!help) {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "TheGodfather",
                        $"| Executing: {ctx.Command?.QualifiedName ?? "<unknown command>"}\n" + 
                        $"| {ctx.User.ToString()}\n" +
                        $"| {ctx.Guild.ToString()} ; {ctx.Channel.ToString()}\n" +
                        $"| Full message: {ctx.Message.Content}",
                        DateTime.Now
                    );
                }
                return Task.FromResult(true);
            } else {
                return Task.FromResult(false);
            }
        }
    }
}
