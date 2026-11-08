using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFirstApi.Entities;

namespace MyFirstApi.Data.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("UserLogin");
        builder.HasKey(ul => ul.Id);
       builder.HasIndex(ul => new { ul.LoginProvider, ul.ProviderKey })
          .IsUnique();
        builder.HasIndex(ul => ul.UserId);
        builder.HasOne(ul => ul.User).WithMany(u => u.UserLogins).HasForeignKey(ul => ul.UserId);
    }
}
