namespace AzraTasks.UITests.PageObjects;

public class TodoListPage(IPage page) : TestPageBase(page)
{
    private ILocator ItemInput => Page.GetByTestId("todo-item-input").Locator("input");
    private ILocator AddItemButton => Page.GetByTestId("add-todo-item-button");

    public async Task AddItemAsync(string title)
    {
        await ItemInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = PlaywrightConfiguration.DefaultTimeout
        });

        await ItemInput.FillAsync(title);
        await AddItemButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<bool> ItemExistsAsync(string title)
    {
        await Task.Delay(500);
        return await Page.Locator($"text={title}").CountAsync() > 0;
    }
}
