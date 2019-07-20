﻿using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfatherTests.Modules.Reactions.Services
{
    [TestFixture]
    public class ReactionsServiceTestsBase : ITheGodfatherServiceTest<ReactionsService>
    {
        public ReactionsService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ReactionsService(TestDatabaseProvider.Database, new Logger(BotConfig.Default), loadData: false);
        }
    }
}
