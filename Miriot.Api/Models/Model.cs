using Microsoft.EntityFrameworkCore;
using Miriot.Common.Model;
using Miriot.Model;

namespace Miriot.Api.Models
{
    public class MiriotContext : DbContext
    {
        public MiriotContext(DbContextOptions<MiriotContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Widget> Widgets { get; set; }
        public DbSet<ToothbrushingEntry> ToothbrushingHistory { get; set; }
        public DbSet<MiriotConfiguration> Configurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Ignore(u => u.FaceRectangle)
                .Ignore(u => u.FriendlyEmotion);

            base.OnModelCreating(modelBuilder);
        }
    }
}
