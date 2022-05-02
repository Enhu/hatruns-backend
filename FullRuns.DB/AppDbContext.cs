using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace FullRuns.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Run> Runs { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> Subcategories { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<RunVariable> RunVariables { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=HatCommunityWebsite;Trusted_Connection=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .Property(c => c.IsLevel)
                .HasDefaultValue(false);

            modelBuilder.Entity<Category>()
                .Property(c => c.IsConsole)
                .HasDefaultValue(false);

            modelBuilder
                .Entity<RunVariable>()
                .HasKey(t => t.Id);

            modelBuilder
                .Entity<RunVariable>()
                .HasOne(r => r.AssociatedRun)
                .WithMany(r => r.RunVariables)
                .HasForeignKey(rv => rv.RunId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<RunVariable>()
                .HasOne(v => v.AssociatedVariable)
                .WithMany(v => v.RunVariables)
                .HasForeignKey(rv => rv.VariableId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Category>()
                .HasMany(r => r.Runs)
                .WithOne(c => c.Category)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Game>()
                .HasMany(r => r.Subcategories)
                .WithOne(c => c.Game)
                .HasForeignKey(c => c.GameId)
                .OnDelete(DeleteBehavior.NoAction);


            //foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            //{
            //    relationship.DeleteBehavior = DeleteBehavior.Restrict;
            //}

            base.OnModelCreating(modelBuilder);
        }
    }
}
