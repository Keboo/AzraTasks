namespace AzraTasks.Api.TodoLists;

public static class TodoListExtensions
{
    public static void MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        var todoLists = app.MapGroup("/api/todo-lists")
            .RequireAuthorization();

        todoLists.MapGet("/", TodoListMethods.GetLists)
            .WithName(TodoListRoutes.GetLists);

        todoLists.MapGet("/{listId:guid}", TodoListMethods.GetList)
            .WithName(TodoListRoutes.GetList);

        todoLists.MapPost("/", TodoListMethods.CreateList)
            .WithName(TodoListRoutes.CreateList);

        todoLists.MapDelete("/{listId:guid}", TodoListMethods.DeleteList)
            .WithName(TodoListRoutes.DeleteList);

        todoLists.MapGet("/{listId:guid}/items", TodoListMethods.GetItems)
            .WithName(TodoListRoutes.GetItems);

        todoLists.MapPost("/{listId:guid}/items", TodoListMethods.CreateItem)
            .WithName(TodoListRoutes.CreateItem);

        todoLists.MapPut("/{listId:guid}/items/{itemId:guid}", TodoListMethods.UpdateItem)
            .WithName(TodoListRoutes.UpdateItem);

        todoLists.MapPut("/{listId:guid}/items/{itemId:guid}/completion", TodoListMethods.SetItemCompletion)
            .WithName(TodoListRoutes.SetItemCompletion);

        todoLists.MapDelete("/{listId:guid}/items/{itemId:guid}", TodoListMethods.DeleteItem)
            .WithName(TodoListRoutes.DeleteItem);
    }
}

public static class TodoListRoutes
{
    public const string GetLists = nameof(GetLists);
    public const string GetList = nameof(GetList);
    public const string CreateList = nameof(CreateList);
    public const string DeleteList = nameof(DeleteList);
    public const string GetItems = nameof(GetItems);
    public const string CreateItem = nameof(CreateItem);
    public const string UpdateItem = nameof(UpdateItem);
    public const string SetItemCompletion = nameof(SetItemCompletion);
    public const string DeleteItem = nameof(DeleteItem);
}
