namespace AzraTasks.UITests.PageObjects;

public class TodoListsPage(IPage page) : TestPageBase(page)
{
    private ILocator CreateListButton => Page.GetByTestId("create-list-button");
    private ILocator ListNameInput => Page.GetByTestId("list-name-dialog-input").Locator("input");
    private ILocator CreateButton => Page.GetByTestId("create-list-dialog-button");

    public Task NavigateAsync(Uri baseUrl) => PerformNavigationAsync(baseUrl, "lists");

    public async Task CreateListAsync(string listName)
    {
        await CreateListButton.ClickAsync();
        await ListNameInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = PlaywrightConfiguration.DefaultTimeout
        });

        await ListNameInput.FillAsync(listName);
        await CreateButton.ClickAsync();
        await Page.WaitForURLAsync("**/lists/*", new PageWaitForURLOptions { Timeout = PlaywrightConfiguration.DefaultTimeout });
    }

    public async Task<bool> ListExistsAsync(string listName)
    {
        await Task.Delay(500);
        return await Page.Locator($"text={listName}").CountAsync() > 0;
    }
}
