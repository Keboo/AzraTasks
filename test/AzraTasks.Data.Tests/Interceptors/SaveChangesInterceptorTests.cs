using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Data.Tests.Interceptors;

public sealed class SaveChangesInterceptorTests : DataTestsBase
{
    [Test]
    public async Task SaveChangesAsync_SetsCreatedByIdFromCurrentUser()
    {
        var user = await CreateUserAsync();

        await Mocker.InDbScopeAsync(async context =>
        {
            context.TodoLists.Add(new TodoList
            {
                Name = "Inbox",
                CreatedById = string.Empty
            });

            await context.SaveChangesAsync();
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var storedList = await context.TodoLists.SingleAsync(list => list.Name == "Inbox");
            await Assert.That(storedList.CreatedById).IsEqualTo(user.Id);
        });
    }

    [Test]
    public async Task SaveChangesAsync_UpdatesTrackingTimestamps()
    {
        var user = await CreateUserAsync();
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
            await context.SaveChangesAsync();

            listId = list.Id;
            createdDate = list.CreatedDate;
            initialLastModifiedDate = list.LastModifiedDate;
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var list = await context.TodoLists.SingleAsync(todoList => todoList.Id == listId);
            list.Name = "Work Updated";
            context.TodoLists.Update(list);
            await context.SaveChangesAsync();
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var updatedList = await context.TodoLists.SingleAsync(todoList => todoList.Id == listId);
            await Assert.That(initialLastModifiedDate).IsEqualTo(createdDate);
            await Assert.That(updatedList.LastModifiedDate).IsGreaterThan(initialLastModifiedDate);
        });
    }
}
