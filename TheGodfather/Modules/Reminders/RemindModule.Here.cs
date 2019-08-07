﻿#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Reminders
{
    public partial class RemindModule
    {
        [Group("here")]
        [Description("Send a reminder to the current channel after specific time span.")]
        
        public class RemindHereModule : RemindModule
        {

            public RemindHereModule(SavedTasksService service, SharedData shared, DatabaseContextBuilder db)
                : base(service, shared, db)
            {

            }


            [GroupCommand, Priority(1)]
            new public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Time span until reminder.")] TimeSpan timespan,
                                             [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, ctx.Channel, message);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx, ctx.Channel);


            [Group("in")]
            [Description("Send a reminder to the current channel after specific time span.")]
            
            public class RemindHereInModule : RemindHereModule
            {

                public RemindHereInModule(SavedTasksService service, SharedData shared, DatabaseContextBuilder db)
                    : base(service, shared, db)
                {

                }


                [GroupCommand]
                new public Task ExecuteGroupAsync(CommandContext ctx,
                                                 [Description("Time span until reminder.")] TimeSpan timespan,
                                                 [RemainingText, Description("What to send?")] string message)
                    => this.AddReminderAsync(ctx, timespan, ctx.Channel, message);
            }


            [Group("at")]
            [Description("Send a reminder to the current channel at a specific point in time (given by date and time string).")]
            
            public class RemindHereAtModule : RemindModule
            {

                public RemindHereAtModule(SavedTasksService service, SharedData shared, DatabaseContextBuilder db)
                    : base(service, shared, db)
                {

                }


                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Date and/or time.")] DateTimeOffset when,
                                             [RemainingText, Description("What to send?")] string message)
                    => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, ctx.Channel, message);
            }
        }
    }
}
