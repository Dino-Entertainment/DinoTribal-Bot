﻿using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public abstract class LocalizedException : Exception
    {
        public string LocalizedMessage { get; }


        public LocalizedException(CommandContext ctx, params object[]? args)
            : base("msg-err")
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild.Id, "msg-err", args);
        }

        public LocalizedException(CommandContext ctx, string key, params object[]? args)
            : base(key)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild.Id, key, args);
        }

        public LocalizedException(CommandContext ctx, string key, Exception inner, params object[]? args)
            : base(key, inner)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild.Id, key, args);
        }
    }
}
