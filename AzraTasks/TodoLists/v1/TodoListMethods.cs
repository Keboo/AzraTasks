using System.Security.Claims;

using AzraTasks.Core.Todos;
using AzraTasks.Data;
using AzraTasks.TodoLists;

using Microsoft.AspNetCore.Http.HttpResults;

namespace AzraTasks.TodoLists.v1;

public static class TodoListMethods
{
    public static async Task<Ok<IEnumerable<TodoListDto>>> GetLists(
        ClaimsPrincipal user,
        ITodoListService todoListService)
    {
        var userId = GetUserId(user);
        var lists = await todoListService.GetListsAsync(userId);
        return TypedResults.Ok(lists.Select(TodoListDto.FromRoom));
    }

    public static async Task<Results<Ok<TodoListDto>, NotFound>> GetList(
        Guid listId,
        ClaimsPrincipal user,
        ITodoListService todoListService)
    {
        var userId = GetUserId(user);
        var list = await todoListService.GetListByIdAsync(listId, userId);

        return list is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(TodoListDto.FromRoom(list));
    }

    public static async Task<CreatedAtRoute<TodoListDto>> CreateList(
        CreateTodoListRequest request,
        ClaimsPrincipal user,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var list = await todoListService.CreateListAsync(request.Name, userId, cancellationToken);
        var response = TodoListDto.FromRoom(list);

        return TypedResults.CreatedAtRoute(response, TodoListRoutes.GetList, new { listId = list.Id });
    }

    public static async Task<NoContent> DeleteList(
        Guid listId,
        ClaimsPrincipal user,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        await todoListService.DeleteListAsync(listId, userId, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Ok<IEnumerable<TodoItemDto>>> GetItems(
        Guid listId,
        ClaimsPrincipal user,
        ITodoListService todoListService)
    {
        var userId = GetUserId(user);
        var items = await todoListService.GetItemsAsync(listId, userId);
        return TypedResults.Ok(items.Select(TodoItemDto.FromQuestion));
    }

    public static async Task<CreatedAtRoute<TodoItemDto>> CreateItem(
        Guid listId,
        CreateTodoItemRequest request,
        ClaimsPrincipal user,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var item = await todoListService.CreateItemAsync(listId, request.Title, userId, cancellationToken);
        var response = TodoItemDto.FromQuestion(item);

        return TypedResults.CreatedAtRoute(response, TodoListRoutes.GetItems, new { listId });
    }

    public static async Task<Ok<TodoItemDto>> UpdateItem(
        Guid listId,
        Guid itemId,
        UpdateTodoItemRequest request,
        ClaimsPrincipal user,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var item = await todoListService.UpdateItemAsync(listId, itemId, request.Title, userId, cancellationToken);
        return TypedResults.Ok(TodoItemDto.FromQuestion(item));
    }

    public static async Task<Ok<TodoItemDto>> SetItemCompletion(
        Guid listId,
        Guid itemId,
        SetTodoItemCompletionRequest request,
        ClaimsPrincipal user,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var item = await todoListService.SetItemCompletedAsync(listId, itemId, request.IsCompleted, userId, cancellationToken);
        return TypedResults.Ok(TodoItemDto.FromQuestion(item));
    }

    public static async Task<NoContent> DeleteItem(
        Guid listId,
        Guid itemId,
        ClaimsPrincipal user,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        await todoListService.DeleteItemAsync(listId, itemId, userId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}

public sealed record CreateTodoListRequest(string Name);

public sealed record CreateTodoItemRequest(string Title);

public sealed record UpdateTodoItemRequest(string Title);

public sealed record SetTodoItemCompletionRequest(bool IsCompleted);

public sealed record TodoListDto(Guid Id, string Name, DateTimeOffset CreatedDate)
{
    public static TodoListDto FromRoom(TodoList room) => new(room.Id, room.Name, room.CreatedDate);
}

public sealed record TodoItemDto(
    Guid Id,
    Guid ListId,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedDate,
    DateTimeOffset? LastModifiedDate)
{
    public static TodoItemDto FromQuestion(TodoItem question) => new(
        question.Id,
        question.ListId,
        question.Text,
        question.IsComplete,
        question.CreatedDate,
        question.LastModifiedDate);
}
