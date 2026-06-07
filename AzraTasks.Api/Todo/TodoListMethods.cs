using AzraTasks.Core.Todos;
using AzraTasks.Data;

using Microsoft.AspNetCore.Http.HttpResults;

namespace AzraTasks.Api.Todo;

public static class TodoListMethods
{
    public static async Task<Ok<IEnumerable<TodoListDto>>> GetLists(
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var lists = await todoListService.GetListsAsync(cancellationToken);
        return TypedResults.Ok(lists.Select(TodoListDto.FromList));
    }

    public static async Task<Results<Ok<TodoListDto>, NotFound>> GetList(
        Guid listId,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var list = await todoListService.GetListByIdAsync(listId, cancellationToken);

        return list is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(TodoListDto.FromList(list));
    }

    public static async Task<CreatedAtRoute<TodoListDto>> CreateList(
        CreateTodoListRequest request,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var list = await todoListService.CreateListAsync(request.Name, cancellationToken);
        var response = TodoListDto.FromList(list);

        return TypedResults.CreatedAtRoute(response, TodoListRoutes.GetList, new { listId = list.Id });
    }

    public static async Task<NoContent> DeleteList(
        Guid listId,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        await todoListService.DeleteListAsync(listId, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Ok<IEnumerable<TodoItemDto>>> GetItems(
        Guid listId,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var items = await todoListService.GetItemsAsync(listId, cancellationToken);
        return TypedResults.Ok(items.Select(TodoItemDto.FromTodoItem));
    }

    public static async Task<CreatedAtRoute<TodoItemDto>> CreateItem(
        Guid listId,
        CreateTodoItemRequest request,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var item = await todoListService.CreateItemAsync(listId, request.Title, cancellationToken);
        var response = TodoItemDto.FromTodoItem(item);

        return TypedResults.CreatedAtRoute(response, TodoListRoutes.GetItems, new { listId });
    }

    public static async Task<Ok<TodoItemDto>> UpdateItem(
        Guid listId,
        Guid itemId,
        UpdateTodoItemRequest request,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var item = await todoListService.UpdateItemAsync(listId, itemId, request.Title, cancellationToken);
        return TypedResults.Ok(TodoItemDto.FromTodoItem(item));
    }

    public static async Task<Ok<TodoItemDto>> SetItemCompletion(
        Guid listId,
        Guid itemId,
        SetTodoItemCompletionRequest request,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var item = await todoListService.SetItemCompletedAsync(listId, itemId, request.IsCompleted, cancellationToken);
        return TypedResults.Ok(TodoItemDto.FromTodoItem(item));
    }

    public static async Task<NoContent> DeleteItem(
        Guid listId,
        Guid itemId,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        await todoListService.DeleteItemAsync(listId, itemId, cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed record CreateTodoListRequest(string Name);

public sealed record CreateTodoItemRequest(string Title);

public sealed record UpdateTodoItemRequest(string Title);

public sealed record SetTodoItemCompletionRequest(bool IsCompleted);

public sealed record TodoListDto(Guid Id, string Name, DateTimeOffset CreatedDate, int? ItemCount)
{
    public static TodoListDto FromList(TodoList list) => new(list.Id, list.Name, list.CreatedDate, list.Items.Count);
}

public sealed record TodoItemDto(
    Guid Id,
    Guid ListId,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedDate,
    DateTimeOffset? LastModifiedDate)
{
    public static TodoItemDto FromTodoItem(TodoItem item) => new(
        item.Id,
        item.ListId,
        item.Text,
        item.IsComplete,
        item.CreatedDate,
        item.LastModifiedDate);
}
