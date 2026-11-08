using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFirstApi.Entities;

namespace MyFirstApi.Data.Configurations;

    public class ToolConfiguration : IEntityTypeConfiguration<Tool>
    {
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
            
         builder.Property(t => t.Name)
          .HasMaxLength(100)
          .IsRequired();

        builder.Property(t => t.Version)
        .HasMaxLength(20)
        .IsRequired();
    }
}
