﻿#region USING_DIRECTIVES
using TheGodfather.Services;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherServiceModule<TService> : TheGodfatherModule where TService : ITheGodfatherService
    {
        protected TService Service { get; }


        protected TheGodfatherServiceModule(TService service, SharedData shared, DBService db)
            : base(shared, db)
        {
            this.Service = service;
        }
    }
}
