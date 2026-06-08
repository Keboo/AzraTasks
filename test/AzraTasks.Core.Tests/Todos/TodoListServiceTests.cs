using AzraTasks.Core.Todos;
using AzraTasks.Testing;

using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Core.Tests.Todos;

public sealed class TodoListServiceTests : UnitTestBase
{
    [Test]
    public async Task GetListsAsync_ReturnsExpectedLists()
    {
        var user = await CreateUserAsync("current-user");
        var olderList = await CreateTodoList(user.Id, "Older List");
        await CreateTodoItemAsync(olderList.Id, "Older-1");
        await CreateTodoItemAsync(olderList.Id, "Older-2", isComplete: true);

        var newerList = await CreateTodoList(user.Id, "Newer List");
        await CreateTodoItemAsync(newerList.Id, "Newer-1");

        var otherUser = await CreateUserAsync("other-user");
        var otherUsersList = await CreateTodoList(otherUser.Id, "Other User List");
        await CreateTodoItemAsync(otherUsersList.Id, "Other-1");

        Mocker.WithUser(user.Id);
        var service = Mocker.CreateInstance<TodoListService>();

        List<TodoListLite> lists = [];
        await foreach (var list in service.GetListsAsync(CT))
        {
            lists.Add(list);
        }

        await Assert.That(lists.Count).IsEqualTo(2);
        await Assert.That(lists[0].Id).IsEqualTo(newerList.Id);
        await Assert.That(lists[0].Name).IsEqualTo("Newer List");
        await Assert.That(lists[0].ItemCount).IsEqualTo(1);
        await Assert.That(lists[0].CompletedItemCount).IsEqualTo(0);

        await Assert.That(lists[1].Id).IsEqualTo(olderList.Id);
        await Assert.That(lists[1].Name).IsEqualTo("Older List");
        await Assert.That(lists[1].ItemCount).IsEqualTo(2);
        await Assert.That(lists[1].CompletedItemCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetListById_ReturnsList()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var firstItem = await CreateTodoItemAsync(list.Id, "Review PR");
        var secondItem = await CreateTodoItemAsync(list.Id, "Write tests", isComplete: true);
        var service = Mocker.CreateInstance<TodoListService>();

        var result = await service.GetListByIdAsync(list.Id, CT);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(list.Id);
        await Assert.That(result.Name).IsEqualTo("Inbox");
        await Assert.That(result.LastModified).IsGreaterThan(DateTimeOffset.MinValue);
        await Assert.That(result.Items.Count).IsEqualTo(2);

        var returnedFirst = result.Items.Single(item => item.Id == firstItem.Id);
        await Assert.That(returnedFirst.Text).IsEqualTo("Review PR");
        await Assert.That(returnedFirst.IsComplete).IsFalse();
        await Assert.That(returnedFirst.LastModified).IsGreaterThan(DateTimeOffset.MinValue);

        var returnedSecond = result.Items.Single(item => item.Id == secondItem.Id);
        await Assert.That(returnedSecond.Text).IsEqualTo("Write tests");
        await Assert.That(returnedSecond.IsComplete).IsTrue();
        await Assert.That(returnedSecond.LastModified).IsGreaterThan(DateTimeOffset.MinValue);
    }

    [Test]
    public async Task GetListById_WithIdFromAnotherUser_ReturnsNull()
    {
        var owner = await CreateUserAsync("owner");
        var ownersList = await CreateTodoList(owner.Id, "Inbox");
        var _ = await CreateUserAsync("other");
        var service = Mocker.CreateInstance<TodoListService>();

        var result = await service.GetListByIdAsync(ownersList.Id, CT);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task CreateListAsync_WithValidData_CreatesList()
    {
        var user = await CreateUserAsync();
        var service = Mocker.CreateInstance<TodoListService>();

        var created = await service.CreateListAsync("  Inbox  ", CT);

        await Assert.That(created.Name).IsEqualTo("Inbox");
        await Assert.That(created.ItemCount).IsEqualTo(0);
        await Assert.That(created.CompletedItemCount).IsEqualTo(0);

        await Mocker.InDbScopeAsync(async context =>
        {
            var stored = await context.TodoLists.SingleAsync(list => list.Id == created.Id, CT);
            await Assert.That(stored.Name).IsEqualTo("Inbox");
            await Assert.That(stored.CreatedById).IsEqualTo(user.Id);
        });
    }

    [Test]
    public async Task CreateListAsync_WithDuplicateListName_ThrowsException()
    {
        var user = await CreateUserAsync();
        var service = Mocker.CreateInstance<TodoListService>();

        await service.CreateListAsync("Inbox", CT);

        await Assert.That(async () => await service.CreateListAsync("Inbox", CT))
            .Throws<InvalidOperationException>()
            .WithMessage("You already have a list named 'Inbox'.");
    }

    [Test]
    public async Task CreateListAsync_WithDuplicateListNameAsAnotherUser_CreatesList()
    {
        var owner = await CreateUserAsync("owner");
        var ownerService = Mocker.CreateInstance<TodoListService>();
        await ownerService.CreateListAsync("Inbox", CT);

        var otherUser = await CreateUserAsync("other");
        var otherService = Mocker.CreateInstance<TodoListService>();

        var otherUsersList = await otherService.CreateListAsync("Inbox", CT);

        await Assert.That(otherUsersList.Name).IsEqualTo("Inbox");

        await Mocker.InDbScopeAsync(async context =>
        {
            var matchingLists = await context.TodoLists
                .IgnoreQueryFilters()
                .Where(list => list.Name == "Inbox")
                .ToListAsync(CT);

            await Assert.That(matchingLists.Count).IsEqualTo(2);
            await Assert.That(matchingLists.Any(list => list.CreatedById == owner.Id)).IsTrue();
            await Assert.That(matchingLists.Any(list => list.CreatedById == otherUser.Id)).IsTrue();
        });
    }

    [Test]
    public async Task DeleteListAsync_WithExistingList_DeletesList()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");
        var service = Mocker.CreateInstance<TodoListService>();

        await service.DeleteListAsync(list.Id, CT);

        await Mocker.InDbScopeAsync(async context =>
        {
            await Assert.That(await context.TodoLists.IgnoreQueryFilters().AnyAsync(todoList => todoList.Id == list.Id, CT)).IsFalse();
            await Assert.That(await context.TodoItems.IgnoreQueryFilters().AnyAsync(todoItem => todoItem.Id == item.Id, CT)).IsFalse();
        });
    }

    [Test]
    public async Task DeleteListAsync_WithListIdFromAnotherUser_ThrowsException()
    {
        var owner = await CreateUserAsync("owner");
        var list = await CreateTodoList(owner.Id, "Inbox");
        var otherUser = await CreateUserAsync("other");
        var service = Mocker.CreateInstance<TodoListService>();

        await Assert.That(async () => await service.DeleteListAsync(list.Id, CT))
            .Throws<InvalidOperationException>()
            .WithMessage("Todo list not found.");
    }

    [Test]
    public async Task CreateItemAsync_WithValidData_AddsItemToList()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var service = Mocker.CreateInstance<TodoListService>();

        var item = await service.CreateItemAsync(list.Id, "Review PR", CT);

        await Assert.That(item.Text).IsEqualTo("Review PR");
        await Assert.That(item.IsComplete).IsFalse();

        await Mocker.InDbScopeAsync(async context =>
        {
            var storedItem = await context.TodoItems.SingleAsync(stored => stored.Id == item.Id);
            await Assert.That(storedItem.ListId).IsEqualTo(list.Id);
        });
    }

    [Test]
    public async Task CreateItemAsync_WithAnotherUsersList_ThrowsException()
    {
        var owner = await CreateUserAsync("owner");
        var ownersList = await CreateTodoList(owner.Id, "Inbox");
        var otherUser = await CreateUserAsync("other");
        var service = Mocker.CreateInstance<TodoListService>();

        await Assert.That(async () => await service.CreateItemAsync(ownersList.Id, "Review PR", CT))
            .Throws<InvalidOperationException>()
            .WithMessage("Todo list not found.");
    }

    [Test]
    [MethodDataSource(nameof(InvalidTitleStrings))]
    public async Task CreateItemAsync_WithInvalidTitle_ThrowsException(string invalidTitle)
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var service = Mocker.CreateInstance<TodoListService>();

        var expectedMessage = string.IsNullOrWhiteSpace(invalidTitle)
            ? "Todo item title is required."
            : "Todo item title must be 2000 characters or fewer.";

        await Assert.That(async () => await service.CreateItemAsync(list.Id, invalidTitle, CT))
            .Throws<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Test]
    public async Task UpdateItemAsync_WithValidData_UpdatesItem()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Original title");
        var service = Mocker.CreateInstance<TodoListService>();

        var updated = await service.UpdateItemAsync(item.Id, "  Updated title  ", CT);

        await Assert.That(updated.Id).IsEqualTo(item.Id);
        await Assert.That(updated.Text).IsEqualTo("Updated title");
        await Assert.That(updated.IsComplete).IsFalse();

        await Mocker.InDbScopeAsync(async context =>
        {
            var stored = await context.TodoItems.SingleAsync(todoItem => todoItem.Id == item.Id, CT);
            await Assert.That(stored.Text).IsEqualTo("Updated title");
        });
    }

    [Test]
    public async Task UpdateItemAsync_WithAnotherUsersItem_ThrowsException()
    {
        var owner = await CreateUserAsync("owner");
        var ownersList = await CreateTodoList(owner.Id, "Inbox");
        var ownersItem = await CreateTodoItemAsync(ownersList.Id, "Review PR");
        var otherUser = await CreateUserAsync("other");
        var service = Mocker.CreateInstance<TodoListService>();

        await Assert.That(async () => await service.UpdateItemAsync(ownersItem.Id, "Updated", CT))
            .Throws<InvalidOperationException>()
            .WithMessage("Todo item not found.");
    }

    [Test]
    [MethodDataSource(nameof(InvalidTitleStrings))]
    public async Task UpdateItemAsync_WithInvalidTitle_ThrowsException(string invalidTitle)
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");
        var service = Mocker.CreateInstance<TodoListService>();

        var expectedMessage = string.IsNullOrWhiteSpace(invalidTitle)
            ? "Todo item title is required."
            : "Todo item title must be 2000 characters or fewer.";

        await Assert.That(async () => await service.UpdateItemAsync(item.Id, invalidTitle, CT))
            .Throws<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Test]
    public async Task SetItemCompletedAsync_WithValidData_UpdatesItem()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");
        var service = Mocker.CreateInstance<TodoListService>();

        var updatedItem = await service.SetItemCompletedAsync(item.Id, true, CT);

        await Assert.That(updatedItem.IsComplete).IsTrue();
    }

    [Test]
    public async Task SetItemCompletedAsync_WithAnotherUsersItem_ThrowsException()
    {
        var owner = await CreateUserAsync("owner");
        var ownersList = await CreateTodoList(owner.Id, "Inbox");
        var ownersItem = await CreateTodoItemAsync(ownersList.Id, "Review PR");
        var otherUser = await CreateUserAsync("other");
        var service = Mocker.CreateInstance<TodoListService>();

        await Assert.That(async () => await service.SetItemCompletedAsync(ownersItem.Id, true, CT))
            .Throws<InvalidOperationException>()
            .WithMessage("Todo item not found.");
    }

    [Test]
    [MethodDataSource(nameof(InvalidTitleStrings))]
    public async Task SetItemCompletedAsync_WithInvalidTitle_ThrowsException(string invalidTitle)
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var service = Mocker.CreateInstance<TodoListService>();

        var expectedMessage = string.IsNullOrWhiteSpace(invalidTitle)
            ? "Todo item title is required."
            : "Todo item title must be 2000 characters or fewer.";

        await Assert.That(async () => await service.CreateItemAsync(list.Id, invalidTitle, CT))
            .Throws<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Test]
    public async Task DeleteItemAsync_WithValidItem_DeletesItem()
    {
        var user = await CreateUserAsync();
        var list = await CreateTodoList(user.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");
        var service = Mocker.CreateInstance<TodoListService>();

        await service.DeleteItemAsync(item.Id, CT);

        await Mocker.InDbScopeAsync(async context =>
        {
            await Assert.That(await context.TodoItems.IgnoreQueryFilters().AnyAsync(todoItem => todoItem.Id == item.Id, CT)).IsFalse();
        });
    }

    [Test]
    public async Task DeleteItemAsyncc_WithAnotherUsersItem_ThrowsException()
    {
        var owner = await CreateUserAsync("owner");
        var list = await CreateTodoList(owner.Id, "Inbox");
        var item = await CreateTodoItemAsync(list.Id, "Review PR");

        // NB: This also sets the current user to other.
        var otherUser = await CreateUserAsync("other");
        var service = Mocker.CreateInstance<TodoListService>();

        await Assert.That(async () => await service.DeleteItemAsync(item.Id, CT))
            .Throws<InvalidOperationException>()
            .WithMessage("Todo item not found.");
    }

    public static IEnumerable<Func<string>> InvalidTitleStrings()
    {
        yield return () => "";
        yield return () => "   ";
        yield return () => new string('x', 2001);
    }
}
