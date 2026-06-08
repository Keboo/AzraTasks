using Microsoft.EntityFrameworkCore;
using AzraTasks.Testing;

namespace AzraTasks.Data.Tests.Interceptors;

public sealed class CreatedByUserInterceptorTests : UnitTestBase
{
    [Test]
    public async Task SaveChangesAsync_WithNewEntity_SetsCreatedById()
    {
        //NB: Order maters since CreateUserAsync also sets the current user
        var otherUser = await CreateUserAsync("other-user");
        var currentUser = await CreateUserAsync("current-user");

        await Mocker.InDbScopeAsync(async context =>
        {
            context.TodoLists.Add(new TodoList
            {
                Name = "Overwritten Owner",
                CreatedById = otherUser.Id
            });

            await context.SaveChangesAsync(CT);
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var storedList = await context.TodoLists.SingleAsync(list => list.Name == "Overwritten Owner", CT);
            await Assert.That(storedList.CreatedById).IsEqualTo(currentUser.Id);
        });
    }

    [Test]
    public async Task SaveChangesAsync_WithExistingEnitity_DoesNotChangeCreatedById()
    {
        //NB: Order maters since CreateUserAsync also sets the current user
        var nextUser = await CreateUserAsync("next-user");
        var originalUser = await CreateUserAsync("original-user");

        Guid listId = Guid.Empty;
        await Mocker.InDbScopeAsync(async context =>
        {
            var list = new TodoList
            {
                Name = "Owned List",
                CreatedById = string.Empty
            };

            context.TodoLists.Add(list);
            await context.SaveChangesAsync(CT);
            listId = list.Id;
        });

        Mocker.WithUser(nextUser.Id);

        await Mocker.InDbScopeAsync(async context =>
        {
            var list = await context.TodoLists.IgnoreQueryFilters().SingleAsync(todoList => todoList.Id == listId, CT);
            list.Name = "Owned List Updated";
            context.TodoLists.Update(list);
            await context.SaveChangesAsync(CT);
        });

        await Mocker.InDbScopeAsync(async context =>
        {
            var updatedList = await context.TodoLists.IgnoreQueryFilters().SingleAsync(todoList => todoList.Id == listId, CT);
            await Assert.That(updatedList.CreatedById).IsEqualTo(originalUser.Id);
        });
    }
}
