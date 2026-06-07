using AzraTasks.Core.Todos;

using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Core.Tests.Todos;

public sealed class TodoListServiceTests : ServiceTestsBase
{
    [Test]
    public async Task CreateListAsync_AllowsSameNameForDifferentUsers()
    {
        var userOne = await CreateUserAsync("user-one");
        var userTwo = await CreateUserAsync("user-two");
        var service = Mocker.CreateInstance<TodoListService>();

        var firstList = await service.CreateListAsync("Inbox", userOne.Id, CancellationToken.None);
        var secondList = await service.CreateListAsync("Inbox", userTwo.Id, CancellationToken.None);

        await Assert.That(firstList.Name).IsEqualTo("Inbox");
        await Assert.That(secondList.Name).IsEqualTo("Inbox");
        await Assert.That(firstList.CreatedById).IsNotEqualTo(secondList.CreatedById);
    }

    [Test]
    public async Task CreateListAsync_RejectsDuplicateNameForSameUser()
    {
        var user = await CreateUserAsync();
        var service = Mocker.CreateInstance<TodoListService>();

        await service.CreateListAsync("Inbox", user.Id, CancellationToken.None);

        await Assert.That(async () => await service.CreateListAsync("Inbox", user.Id, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("You already have a list named 'Inbox'.");
    }

    [Test]
    public async Task CreateItemAsync_AddsItemToOwnedList()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var service = Mocker.CreateInstance<TodoListService>();

        var item = await service.CreateItemAsync(list.Id, "Review PR", user.Id, CancellationToken.None);

        await Assert.That(item.Text).IsEqualTo("Review PR");
        await Assert.That(item.IsComplete).IsFalse();

        await Mocker.InDbScopeAsync(async context =>
        {
            var storedItem = await context.TodoItems.SingleAsync(item => item.Id == item.Id);
            await Assert.That(storedItem.ListId).IsEqualTo(list.Id);
        });
    }

    [Test]
    public async Task SetItemCompletedAsync_UpdatesCompletionState()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");
        var service = Mocker.CreateInstance<TodoListService>();

        var updatedItem = await service.SetItemCompletedAsync(list.Id, item.Id, true, user.Id, CancellationToken.None);

        await Assert.That(updatedItem.IsComplete).IsTrue();
        await Assert.That(updatedItem.LastModifiedDate).IsNotNull();
    }

    [Test]
    public async Task DeleteItemAsync_RejectsOtherUsersList()
    {
        var owner = await CreateUserAsync("owner");
        var otherUser = await CreateUserAsync("other");
        var list = await CreateTodoList(owner.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");
        var service = Mocker.CreateInstance<TodoListService>();

        await Assert.That(async () => await service.DeleteItemAsync(list.Id, item.Id, otherUser.Id, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("Todo item not found.");
    }
}
