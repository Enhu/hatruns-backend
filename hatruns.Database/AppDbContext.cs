using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Run> Runs { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public DbSet<VariableValue> VariableValues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<RunVariableValue> RunVariables { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<RunUser> RunUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=hatruns;Trusted_Connection=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .Property(c => c.IsConsole)
                .HasDefaultValue(false);

            //many to many run and variables

            modelBuilder
                .Entity<RunVariableValue>()
                .HasKey(t => t.Id);

            modelBuilder
                .Entity<RunVariableValue>()
                .HasOne(r => r.AssociatedRun)
                .WithMany(r => r.RunVariableValues)
                .HasForeignKey(rv => rv.RunId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder
                .Entity<RunVariableValue>()
                .HasOne(v => v.AssociatedVariableValue)
                .WithMany(v => v.RunVariables)
                .HasForeignKey(rv => rv.VariableValueId)
                .OnDelete(DeleteBehavior.NoAction);

            //many to many run and users

            modelBuilder
                .Entity<RunUser>()
                .HasKey(t => t.Id);

            modelBuilder
                .Entity<RunUser>()
                .HasOne(r => r.AssociatedRun)
                .WithMany(r => r.RunUsers)
                .HasForeignKey(rv => rv.RunId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder
                .Entity<RunUser>()
                .HasOne(v => v.AssociatedUser)
                .WithMany(v => v.RunUsers)
                .HasForeignKey(rv => rv.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //one to many category

            modelBuilder.Entity<Category>()
                .HasMany(r => r.Runs)
                .WithOne(c => c.Category)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>()
                .HasMany(r => r.Subcategories)
                .WithOne(c => c.Category)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            //one to many variables

            modelBuilder.Entity<Variable>()
                .HasMany(r => r.Values)
                .WithOne(c => c.Variable)
                .HasForeignKey(c => c.VariableId)
                .OnDelete(DeleteBehavior.NoAction);

            //one to many subcategory

            modelBuilder.Entity<Subcategory>()
                .HasMany(r => r.Runs)
                .WithOne(c => c.SubCategory)
                .HasForeignKey(c => c.SubcategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            //one to many game

            modelBuilder.Entity<Game>()
                .HasMany(r => r.Levels)
                .WithOne(c => c.Game)
                .HasForeignKey(c => c.GameId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Game>()
                .HasMany(r => r.Categories)
                .WithOne(c => c.Game)
                .HasForeignKey(c => c.GameId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Game>()
                .HasMany(r => r.Variables)
                .WithOne(c => c.Game)
                .HasForeignKey(c => c.GameId)
                .OnDelete(DeleteBehavior.NoAction);

            //one to many level

            modelBuilder.Entity<Level>()
                .HasMany(r => r.Categories)
                .WithOne(c => c.Level)
                .HasForeignKey(c => c.LevelId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }
    }
}
