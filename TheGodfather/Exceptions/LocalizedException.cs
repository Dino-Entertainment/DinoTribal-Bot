﻿using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services;

namespace TheGodfather.Exceptions
{
    public abstract class LocalizedException : Exception
    {
        public string LocalizedMessage { get; }

        
        public LocalizedException(string message)
            : base(message)
        {
            this.LocalizedMessage = message;
        }

        public LocalizedException(CommandContext ctx, params object[]? args)
            : base("err-loc")
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild.Id, "err-loc", args);
        }

        public LocalizedException(CommandContext ctx, string key, params object[]? args)
            : base(key)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild.Id, key, args);
        }

        public LocalizedException(CommandContext ctx, Exception inner, string key, params object[]? args)
            : base(key, inner)
        {
            this.LocalizedMessage = ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild.Id, key, args);
        }
    }
}
