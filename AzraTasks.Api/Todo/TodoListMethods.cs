using AzraTasks.Core.Todos;

using Microsoft.AspNetCore.Http.HttpResults;

namespace AzraTasks.Api.Todo;

public static class TodoListMethods
{
    public static async Task<Ok<IEnumerable<TodoListDto>>> GetLists(
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var lists = await todoListService.GetListsAsync(cancellationToken)
            .ToListAsync(cancellationToken);
        return TypedResults.Ok(lists.Select(TodoListDto.FromList));
    }

    public static async Task<Results<Ok<TodoListFullDto>, NotFound>> GetList(
        Guid listId,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var list = await todoListService.GetListByIdAsync(listId, cancellationToken);

        return list is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(TodoListFullDto.FromList(list));
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
        Guid itemId,
        UpdateTodoItemRequest request,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var item = await todoListService.UpdateItemAsync(itemId, request.Title, cancellationToken);
        return TypedResults.Ok(TodoItemDto.FromTodoItem(item));
    }

    public static async Task<Ok<TodoItemDto>> SetItemCompletion(
        Guid itemId,
        SetTodoItemCompletionRequest request,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        var item = await todoListService.SetItemCompletedAsync(itemId, request.IsCompleted, cancellationToken);
        return TypedResults.Ok(TodoItemDto.FromTodoItem(item));
    }

    public static async Task<NoContent> DeleteItem(
        Guid itemId,
        ITodoListService todoListService,
        CancellationToken cancellationToken)
    {
        await todoListService.DeleteItemAsync(itemId, cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed record CreateTodoListRequest(string Name);

public sealed record CreateTodoItemRequest(string Title);

public sealed record UpdateTodoItemRequest(string Title);

public sealed record SetTodoItemCompletionRequest(bool IsCompleted);

public sealed record TodoListDto(Guid Id, string Name, DateTimeOffset LastModified, int ItemCount, int CompletedItemCount)
{
    public static TodoListDto FromList(TodoListLite list) 
        => new(list.Id, list.Name, list.LastModified, list.ItemCount, list.CompletedItemCount);
}

public sealed record TodoListFullDto(Guid Id, string Name, DateTimeOffset LastModified, IReadOnlyList<TodoItemDto> Items)
{
    public static TodoListFullDto FromList(TodoListFull list) 
        => new(list.Id, list.Name, list.LastModified, [..list.Items.Select(TodoItemDto.FromTodoItem)]);
}


public sealed record TodoItemDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset? LastModified)
{
    public static TodoItemDto FromTodoItem(TodoListItem item) => new(
        item.Id,
        item.Text,
        item.IsComplete,
        item.LastModified);
}
