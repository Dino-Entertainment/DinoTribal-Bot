﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CheckIgnoreAttribute : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Dependencies.GetDependency<TheGodfather>().Listening) {
                ctx.Dependencies.GetDependency<TheGodfather>().LogHandle.Log(LogLevel.Info,
                    $" Attemping to execute: {ctx.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                    $" In message: {ctx.Message.Content}" + Environment.NewLine +
                    $" User: {ctx.User.ToString()}" + Environment.NewLine +
                    $" Location: '{ctx.Guild.Name}' ({ctx.Guild.Id}) ; {ctx.Channel.ToString()}"
                );
                ctx.TriggerTypingAsync();
                return Task.FromResult(true);
            } else {
                return Task.FromResult(false);
            }
        }
    }
}
