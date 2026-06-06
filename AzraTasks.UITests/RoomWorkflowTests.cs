using AzraTasks.UITests.PageObjects;

namespace AzraTasks.UITests;

public class RoomWorkflowTests : AuthedUserTestBase
{
    private string TestListName { get; set; } = "";

    protected override async Task AfterTestSetupAsync()
    {
        await base.AfterTestSetupAsync();
        TestListName = $"TestList{CreateUniqueId()}";
    }

    [Test]
    public async Task CreateList_ShouldAppearInMyLists()
    {
        var listsPage = new TodoListsPage(Page);
        await listsPage.NavigateAsync(FrontendBaseUri);
        await listsPage.CreateListAsync(TestListName);

        await Page.GoBackAsync();
        await Page.WaitForURLAsync("**/lists", new PageWaitForURLOptions { Timeout = PlaywrightConfiguration.DefaultTimeout });
        
        await Assert.That(listsPage.ListExistsAsync(TestListName))
            .IsTrue()
            .Because("List should exist in the list overview");
    }

    [Test]
    public async Task AddItem_ShouldAppearInTodoList()
    {
        var listsPage = new TodoListsPage(Page);
        await listsPage.NavigateAsync(FrontendBaseUri);
        await listsPage.CreateListAsync(TestListName);

        var todoListPage = new TodoListPage(Page);
        var itemTitle = $"Task {CreateUniqueId()}";
        await todoListPage.AddItemAsync(itemTitle);

        await Assert.That(await todoListPage.ItemExistsAsync(itemTitle))
            .IsTrue()
            .Because("Newly added task should appear in the current list");
    }

    [Test]
    [Category(TestCategories.Accessibility)]
    public async Task ListsPageIsAccessible()
    {
        var listsPage = new TodoListsPage(Page);
        await listsPage.NavigateAsync(FrontendBaseUri);
        await listsPage.CreateListAsync(TestListName);
        await Page.GoBackAsync();
        await Page.WaitForURLAsync("**/lists", new PageWaitForURLOptions { Timeout = PlaywrightConfiguration.DefaultTimeout });
        
        await AssertNoAccessibilityViolations();
    }

    [Test]
    [Category(TestCategories.Accessibility)]
    public async Task TodoListPageIsAccessible()
    {
        var listsPage = new TodoListsPage(Page);
        await listsPage.NavigateAsync(FrontendBaseUri);
        await listsPage.CreateListAsync(TestListName);

        await AssertNoAccessibilityViolations();
    }
}

