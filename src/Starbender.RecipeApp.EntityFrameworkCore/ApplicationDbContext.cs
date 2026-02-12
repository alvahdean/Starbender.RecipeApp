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

    public virtual DbSet<Instruction> Instructions { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Recipe>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => p.Title).IsUnique();

            // Explicit relationships (EF should infer these)
            b.HasMany(r => r.Instructions)
                .WithOne(r=> r.Recipe)
                .HasForeignKey(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(r => r.RecipeIngredients)
                .WithOne(ri => ri.Recipe)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 image per recipe but images can be reused (e.g. placeholder)
            b.HasOne(r => r.ImageMetadata)
                       .WithMany()
                       .HasForeignKey(r => r.ImageMetadataId)
                       .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Instruction>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => new { p.Order, p.RecipeId}).IsUnique();
        });

        builder.Entity<Ingredient>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => p.Name).IsUnique();

            // Explicit relationships (EF should infer these)
            b.HasMany(i => i.RecipeIngredients)
                .WithOne(ri => ri.Ingredient)
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RecipeIngredient>(b =>
        {
            b.HasKey(p => new { p.RecipeId, p.IngredientId });

            // Explicit relationships (EF should infer these)
            b.HasOne(ri => ri.Unit)
                .WithMany()
                .HasForeignKey(ri => ri.UnitId)
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
