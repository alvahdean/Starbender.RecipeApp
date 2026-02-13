using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Starbender.BlobStorage.Entities;
using Starbender.RecipeApp.Domain.Shared.Entities;

namespace Starbender.RecipeApp.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public virtual DbSet<BlobMetadata> BlobMetadata { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Recipe>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => p.Title).IsUnique();

            b.HasMany(r => r.RecipeIngredients)
                .WithOne()
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Ingredient>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => p.Name).IsUnique();
        });

        builder.Entity<RecipeIngredient>(b =>
        {
            b.HasKey(p => new { p.RecipeId, p.IngredientId });

            // Explicit relationships (EF should infer these)
            b.HasOne(ri => ri.Unit)
                .WithMany()
                .HasForeignKey(ri => ri.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Unit>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => p.Name).IsUnique();
        });

        // Blob metadata must have unique { StoreType, BlobStoreId, Location }
        builder.Entity<BlobMetadata>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => new { p.StoreType, p.ContainerId, p.BlobId }).IsUnique();
        });
    }
}
