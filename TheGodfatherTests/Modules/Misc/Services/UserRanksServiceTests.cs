﻿using NUnit.Framework;
using TheGodfather.Database.Models;
using TheGodfather.Misc.Services;

namespace TheGodfatherTests.Modules.Misc.Services
{
    [TestFixture]
    public sealed class UserRanksServiceTests : ITheGodfatherServiceTest<UserRanksService>
    {
        public UserRanksService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new UserRanksService();
        }


        [Test]
        public void CalculateRankForMessageCountTests()
        {
            Assert.That(this.Service.CalculateRankForMessageCount(0), Is.Zero);
            Assert.That(this.Service.CalculateRankForMessageCount(1), Is.Zero);
            Assert.That(this.Service.CalculateRankForMessageCount(9), Is.Zero);
            Assert.That(this.Service.CalculateRankForMessageCount(10), Is.EqualTo(1));
            Assert.That(this.Service.CalculateRankForMessageCount(20), Is.EqualTo(1));
            Assert.That(this.Service.CalculateRankForMessageCount(39), Is.EqualTo(1));
            Assert.That(this.Service.CalculateRankForMessageCount(40), Is.EqualTo(2));
            Assert.That(this.Service.CalculateRankForMessageCount(60), Is.EqualTo(2));
            Assert.That(this.Service.CalculateRankForMessageCount(89), Is.EqualTo(2));
            Assert.That(this.Service.CalculateRankForMessageCount(90), Is.EqualTo(3));
            Assert.That(this.Service.CalculateRankForMessageCount(101), Is.EqualTo(3));
            Assert.That(this.Service.CalculateRankForMessageCount(159), Is.EqualTo(3));
            Assert.That(this.Service.CalculateRankForMessageCount(160), Is.EqualTo(4));
            Assert.That(this.Service.CalculateRankForMessageCount(250), Is.EqualTo(5));
            Assert.That(this.Service.CalculateRankForMessageCount(88361), Is.EqualTo(94));
        }

        [Test]
        public void GetAndIncrementMessageCountForUserTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.That(this.Service.GetMessageCountForUser(id), Is.Zero);

            Assert.That(this.Service.IncrementMessageCountForUser(MockData.Ids[0]), Is.Zero);

            Assert.That(this.Service.GetMessageCountForUser(MockData.Ids[0]), Is.EqualTo(1));
            Assert.That(this.Service.GetMessageCountForUser(MockData.Ids[1]), Is.Zero);
            Assert.That(this.Service.GetMessageCountForUser(MockData.Ids[2]), Is.Zero);

            for (int i = 0; i < 9; i++)
                Assert.That(this.Service.IncrementMessageCountForUser(MockData.Ids[1]), Is.Zero);
            Assert.That(this.Service.IncrementMessageCountForUser(MockData.Ids[1]), Is.EqualTo(1));
            Assert.That(this.Service.IncrementMessageCountForUser(MockData.Ids[1]), Is.Zero);

            Assert.That(this.Service.GetMessageCountForUser(MockData.Ids[0]), Is.EqualTo(1));
            Assert.That(this.Service.GetMessageCountForUser(MockData.Ids[1]), Is.EqualTo(11));
            Assert.That(this.Service.GetMessageCountForUser(MockData.Ids[2]), Is.Zero);
        }

        [Test]
        public void CalculateRankForUserTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.That(this.Service.GetMessageCountForUser(id), Is.Zero);

            for (int i = 0; i < 10; i++)
                this.Service.IncrementMessageCountForUser(MockData.Ids[0]);

            Assert.That(this.Service.CalculateRankForUser(MockData.Ids[0]), Is.EqualTo(1));
            Assert.That(this.Service.CalculateRankForUser(MockData.Ids[1]), Is.Zero);
            Assert.That(this.Service.CalculateRankForUser(MockData.Ids[2]), Is.Zero);
        }

        [Test]
        public void BlankDatabaseSyncTests()
        {
            this.Service.IncrementMessageCountForUser(MockData.Ids[0]);
            this.Service.IncrementMessageCountForUser(MockData.Ids[0]);
            for (int i = 0; i < 9; i++)
                this.Service.IncrementMessageCountForUser(MockData.Ids[1]);
            this.Service.IncrementMessageCountForUser(MockData.Ids[2]);

            TestDatabaseProvider.AlterAndVerify(
                alter: db => this.Service.Sync(db),
                verify: db => {
                    Assert.That(db.XpCounts, Has.Exactly(3).Items);
                    XpCount u1 = db.XpCounts.Find((long)MockData.Ids[0]);
                    XpCount u2 = db.XpCounts.Find((long)MockData.Ids[1]);
                    XpCount u3 = db.XpCounts.Find((long)MockData.Ids[2]);
                    Assert.That(db.XpCounts.Find(123451234512345), Is.Null);
                    Assert.That(u1, Is.Not.Null);
                    Assert.That(u2, Is.Not.Null);
                    Assert.That(u3, Is.Not.Null);
                    Assert.That(u1.Xp, Is.EqualTo(2));
                    Assert.That(u2.Xp, Is.EqualTo(9));
                    Assert.That(u3.Xp, Is.EqualTo(1));
                }
            );
        }

        [Test]
        public void FilledDatabaseSyncTests()
        {
            this.Service.IncrementMessageCountForUser(MockData.Ids[1]);
            this.Service.IncrementMessageCountForUser(MockData.Ids[2]);

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    var msgcount = new XpCount[] {
                        new XpCount() {
                            Xp = 5,
                            UserId = MockData.Ids[0]
                        },
                        new XpCount() {
                            Xp = 5,
                            UserId = MockData.Ids[1]
                        },
                    };
                    db.XpCounts.AddRange(msgcount);
                },
                alter: db => this.Service.Sync(db),
                verify: db => {
                    Assert.That(db.XpCounts, Has.Exactly(3).Items);
                    XpCount u1 = db.XpCounts.Find((long)MockData.Ids[0]);
                    XpCount u2 = db.XpCounts.Find((long)MockData.Ids[1]);
                    XpCount u3 = db.XpCounts.Find((long)MockData.Ids[2]);
                    Assert.That(db.XpCounts.Find(123451234512345), Is.Null);
                    Assert.That(u1, Is.Not.Null);
                    Assert.That(u2, Is.Not.Null);
                    Assert.That(u3, Is.Not.Null);
                    Assert.That(u1.Xp, Is.EqualTo(5));
                    Assert.That(u2.Xp, Is.EqualTo(6));
                    Assert.That(u3.Xp, Is.EqualTo(1));
                }
            );
        }

        [Test]
        public void RepetitiveSyncTests()
        {
            this.Service.IncrementMessageCountForUser(MockData.Ids[1]);
            this.Service.IncrementMessageCountForUser(MockData.Ids[2]);

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    var msgcount = new XpCount[] {
                        new XpCount() {
                            Xp = 5,
                            UserId = MockData.Ids[0]
                        },
                        new XpCount() {
                            Xp = 5,
                            UserId = MockData.Ids[1]
                        },
                    };
                    db.XpCounts.AddRange(msgcount);
                },
                alter: db => {
                    this.Service.Sync(db);
                    this.Service.IncrementMessageCountForUser(MockData.Ids[1]);
                    this.Service.IncrementMessageCountForUser(MockData.Ids[2]);
                    this.Service.Sync(db);
                    this.Service.IncrementMessageCountForUser(MockData.Ids[1]);
                    this.Service.IncrementMessageCountForUser(MockData.Ids[2]);
                    this.Service.Sync(db);
                    this.Service.Sync(db);
                    this.Service.Sync(db);
                },
                verify: db => {
                    Assert.That(db.XpCounts, Has.Exactly(3).Items);
                    XpCount u1 = db.XpCounts.Find((long)MockData.Ids[0]);
                    XpCount u2 = db.XpCounts.Find((long)MockData.Ids[1]);
                    XpCount u3 = db.XpCounts.Find((long)MockData.Ids[2]);
                    Assert.That(db.XpCounts.Find(123451234512345), Is.Null);
                    Assert.That(u1, Is.Not.Null);
                    Assert.That(u2, Is.Not.Null);
                    Assert.That(u3, Is.Not.Null);
                    Assert.That(u1.Xp, Is.EqualTo(5));
                    Assert.That(u2.Xp, Is.EqualTo(8));
                    Assert.That(u3.Xp, Is.EqualTo(3));
                }
            );
        }
    }
}
