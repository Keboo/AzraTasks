using System.Security.Claims;

using AzraTasks.Core.Todos;
using AzraTasks.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzraTasks.Controllers;

[ApiController]
[Authorize]
[Route("api/todo-lists")]
public class TodoListsController(ITodoListService todoListService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLists()
    {
        var userId = GetUserId();
        var lists = await todoListService.GetListsAsync(userId);
        return Ok(lists.Select(TodoListDto.FromRoom));
    }

    [HttpGet("{listId:guid}")]
    public async Task<IActionResult> GetList(Guid listId)
    {
        var userId = GetUserId();
        var list = await todoListService.GetListByIdAsync(listId, userId);

        return list is null ? NotFound() : Ok(TodoListDto.FromRoom(list));
    }

    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] CreateTodoListRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var list = await todoListService.CreateListAsync(request.Name, userId, cancellationToken);

        return CreatedAtAction(nameof(GetList), new { listId = list.Id }, TodoListDto.FromRoom(list));
    }

    [HttpDelete("{listId:guid}")]
    public async Task<IActionResult> DeleteList(Guid listId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await todoListService.DeleteListAsync(listId, userId, cancellationToken);

        return NoContent();
    }

    [HttpGet("{listId:guid}/items")]
    public async Task<IActionResult> GetItems(Guid listId)
    {
        var userId = GetUserId();
        var items = await todoListService.GetItemsAsync(listId, userId);

        return Ok(items.Select(TodoItemDto.FromQuestion));
    }

    [HttpPost("{listId:guid}/items")]
    public async Task<IActionResult> CreateItem(
        Guid listId,
        [FromBody] CreateTodoItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var item = await todoListService.CreateItemAsync(listId, request.Title, userId, cancellationToken);

        return CreatedAtAction(nameof(GetItems), new { listId }, TodoItemDto.FromQuestion(item));
    }

    [HttpPut("{listId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid listId,
        Guid itemId,
        [FromBody] UpdateTodoItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var item = await todoListService.UpdateItemAsync(listId, itemId, request.Title, userId, cancellationToken);

        return Ok(TodoItemDto.FromQuestion(item));
    }

    [HttpPut("{listId:guid}/items/{itemId:guid}/completion")]
    public async Task<IActionResult> SetItemCompletion(
        Guid listId,
        Guid itemId,
        [FromBody] SetTodoItemCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var item = await todoListService.SetItemCompletedAsync(listId, itemId, request.IsCompleted, userId, cancellationToken);

        return Ok(TodoItemDto.FromQuestion(item));
    }

    [HttpDelete("{listId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await todoListService.DeleteItemAsync(listId, itemId, userId, cancellationToken);

        return NoContent();
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}

public record CreateTodoListRequest(string Name);
public record CreateTodoItemRequest(string Title);
public record UpdateTodoItemRequest(string Title);
public record SetTodoItemCompletionRequest(bool IsCompleted);

public sealed record TodoListDto(Guid Id, string Name, DateTimeOffset CreatedDate)
{
    public static TodoListDto FromRoom(Room room) => new(room.Id, room.FriendlyName, room.CreatedDate);
}

public sealed record TodoItemDto(Guid Id, Guid ListId, string Title, bool IsCompleted, DateTimeOffset CreatedDate, DateTimeOffset? LastModifiedDate)
{
    public static TodoItemDto FromQuestion(Question question) => new(
        question.Id,
        question.RoomId,
        question.QuestionText,
        question.IsAnswered,
        question.CreatedDate,
        question.LastModifiedDate);
}
