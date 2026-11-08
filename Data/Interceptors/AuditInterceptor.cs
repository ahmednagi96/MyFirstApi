using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Entities;
namespace MyFirstApi.Data.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is not null){
            foreach(var entry in context.ChangeTracker.Entries()){
                if(entry.Entity is Post post){
                    if(entry.State == EntityState.Added){
                        post.CreatedAt = DateTime.UtcNow;
                        post.UpdatedAt = DateTime.UtcNow;
                    }
                    if(entry.State == EntityState.Modified){
                        post.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}