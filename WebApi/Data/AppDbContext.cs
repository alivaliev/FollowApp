using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions options): base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call the base method
            modelBuilder.Entity<User>().ToTable("Users");
            // Define your primary key for IdentityUser (in this case, using string)
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .HasConversion<string>();

            // Continue with your UserFollow configuration as before
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Follower)
                .HasForeignKey(uf => uf.FollowerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Followed)
                .WithMany(u => u.Followed)
                .HasForeignKey(uf => uf.FollowedUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Configure value converter for the PhotosJson property
            modelBuilder.Entity<User>()
             .Property(u => u.Photos)
             .HasConversion(
                 v => JsonConvert.SerializeObject(v),
                 v => JsonConvert.DeserializeObject<List<Photo>>(v));
        }
    }
}
