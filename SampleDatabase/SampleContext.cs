using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace SampleDatabase
{
    public class SampleContext : DbContext
    {
        public DbSet<PostEntity> Posts { get; set; }
        public DbSet<BlogEntity> Blogs { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<PostUserEntity> PostUsers { get; set; }

        public SampleContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PostUserEntity>().HasKey(t => t.Id);
            modelBuilder.Entity<PostUserEntity>().HasOne(t => t.User).WithMany(t => t.PostUsers);

            modelBuilder.Entity<PostEntity>().HasKey(t => t.Id);

            modelBuilder.Entity<BlogEntity>().HasKey(t => t.Id);
            modelBuilder.Entity<BlogEntity>().HasMany(t => t.PostUsers);

            modelBuilder.Entity<UserEntity>().HasKey(t => t.Id);

            modelBuilder.Entity<UserEntity>()
                .HasData(
                new { Id = 1, Name = "Andrea", Surname = "Tosato", PostUserEntityId = 1 },
                new { Id = 2, Name = "Mario", Surname = "Rossi", PostUserEntityId = 2 });

            modelBuilder.Entity<PostUserEntity>()
               .HasData(
               new { Id = 1, PostId = 1, UserId = 1, BlogEntityId = 1 },
               new { Id = 2, PostId = 2, UserId = 2, BlogEntityId = 1 });

            modelBuilder.Entity<PostEntity>()
                .HasData(
                new { Id = 1, CreateDate = DateTime.Now, Text = "My Text" },
                new { Id = 2, CreateDate = DateTime.Now.AddMinutes(50), Text = "My Text 2" },
                new { Id = 3, CreateDate = DateTime.Now.AddMinutes(-50), Text = "My Text 3" });

            modelBuilder.Entity<BlogEntity>()
            .HasData(
                new BlogEntity
                {
                    Id = 1
                }
            );
        }
    }

    public class SampleContextFactory : IDesignTimeDbContextFactory<SampleContext>
    {
        public SampleContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SampleContext>();
            optionsBuilder.UseSqlServer("Server=sqlserver;Database=BlogDb;User Id=sa;Password=m1Password@12J;")
                .EnableSensitiveDataLogging();

            return new SampleContext(optionsBuilder.Options);
        }
    }
}
