using AzraTasks.Data.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzraTasks.Data;

public class CreatedByUserInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplicationDbContext? context = eventData.Context as ApplicationDbContext;
        foreach (var changedEntity in context?.ChangeTracker.Entries<UserObject>() ?? [])
        {
            if (changedEntity.State == EntityState.Added)
            {
                changedEntity.Entity.CreatedById = context!.UserId;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
