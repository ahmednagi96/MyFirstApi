using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFirstApi.Entities;

namespace MyFirstApi.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Content).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.UserId).IsRequired();
        builder.HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
