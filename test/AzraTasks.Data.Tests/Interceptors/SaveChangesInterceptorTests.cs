using Microsoft.EntityFrameworkCore;
using AzraTasks.Testing;

namespace AzraTasks.Data.Tests.Interceptors;

public sealed class SaveChangesInterceptorTests : UnitTestBase
{
    [Test]
    public async Task TrackingBaseInterceptor_UpdatesTrackingTimestamps()
    {
        await CreateUserAsync();
        Guid listId = Guid.Empty;
        DateTimeOffset createdDate = default;
        DateTimeOffset initialLastModifiedDate = default;

        await Mocker.InDbScopeAsync(async context =>
        {
            var list = new TodoList
            {
                Name = "Work",
                CreatedById = string.Empty
            };

            context.TodoLists.Add(list);
            await context.SaveChangesAsync(CT);

            // NB: We need to reload here to get the actual rounded timestamp that was persisted
            await context.Entry(list).ReloadAsync(CT);

            listId = list.Id;
            createdDate = list.CreatedDate;
            initialLastModifiedDate = list.LastModifiedDate;
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var list = await context.TodoLists.SingleAsync(todoList => todoList.Id == listId, CT);
            list.Name = "Work Updated";
            context.TodoLists.Update(list);
            await context.SaveChangesAsync(CT);
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var updatedList = await context.TodoLists.SingleAsync(todoList => todoList.Id == listId, CT);

            await Assert.That(updatedList.CreatedDate).IsEqualTo(createdDate);
            await Assert.That(initialLastModifiedDate).IsEqualTo(createdDate);

            await Assert.That(updatedList.LastModifiedDate).IsGreaterThan(initialLastModifiedDate);
        });
    }
}
