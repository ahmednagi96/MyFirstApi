using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFirstApi.Entities;

namespace MyFirstApi.Data.Configurations;

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasMany(c => c.Tools)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

            builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

            builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasQueryFilter(c => c.DeletedAt == null && c.IsActive);

    }
    }
