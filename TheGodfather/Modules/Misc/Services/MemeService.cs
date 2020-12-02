﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc.Services
{
    public sealed class MemeService : DbAbstractionServiceBase<Meme, ulong, string>
    {
        public override bool IsDisabled => false;


        public MemeService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<Meme> DbSetSelector(TheGodfatherDbContext db)
            => db.Memes;

        public override IQueryable<Meme> GroupSelector(IQueryable<Meme> entities, ulong grid)
            => entities.Where(m => m.GuildIdDb == (long)grid);

        public override Meme EntityFactory(ulong grid, string name)
            => new Meme { GuildId = grid, Name = name };

        public override string EntityIdSelector(Meme entity) 
            => entity.Name;

        public override ulong EntityGroupSelector(Meme entity) 
            => entity.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong grid, string name) 
            => new object[] { grid, name };
    }
}
