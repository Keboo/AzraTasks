using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzraTasks.Data;

public class TrackingBaseInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;

        foreach(var changedEntity in context?.ChangeTracker.Entries<TrackingBase>() ?? [])
        {
            switch(changedEntity.State)
            {
                case EntityState.Added:
                    changedEntity.Entity.LastModifiedDate = 
                        changedEntity.Entity.CreatedDate = 
                            DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    changedEntity.Entity.LastModifiedDate = DateTimeOffset.UtcNow;
                    break;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
