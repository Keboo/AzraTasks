using AzraTasks.Data.Auth;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AzraTasks.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserIdProvider userIdProvider)
    : IdentityDbContext<ApplicationUser>(options)
{
    public const string CreatedByUserQueryFilterId = nameof(CreatedByUserQueryFilterId);

    public string UserId => userIdProvider.UserId;

    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        void SetupTracking<TTrackingBase>() where TTrackingBase : TrackingBase
        {
            modelBuilder.Entity<TTrackingBase>()
                .Property(e => e.Id)
                .HasValueGenerator<SequentialGuidValueGenerator>();
        }

        void SetupUserObject<TUserObject>() where TUserObject : UserObject
        {
            SetupTracking<TUserObject>();

            modelBuilder.Entity<TUserObject>()
                .HasQueryFilter(CreatedByUserQueryFilterId, x => x.CreatedById == userIdProvider.UserId);
        }

        SetupUserObject<TodoList>();
        SetupTracking<TodoItem>();


        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the DateTimeOffsetToBinaryConverter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            var converter = new DateTimeOffsetToBinaryConverter();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(x => x.ClrType.Namespace?.StartsWith(nameof(AzraTasks)) == true))
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                    {
                        property.SetValueConverter(converter);
                    }
                }

            }

        }
    }
}
