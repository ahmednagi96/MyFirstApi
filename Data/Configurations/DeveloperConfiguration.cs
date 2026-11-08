using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFirstApi.Entities;

namespace MyFirstApi.Data.Configurations;

    public class DeveloperConfiguration : IEntityTypeConfiguration<Developer>
    {
        public void Configure(EntityTypeBuilder<Developer> builder)
        {   
            builder.OwnsOne(u => u.MainAddress);
        }
    }
