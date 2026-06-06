namespace AzraTasks.UITests.PageObjects;

public class HomePage(IPage page) : TestPageBase(page)
{
    private ILocator PrimaryActionButton => Page.GetByTestId("home-primary-action");

    public Task NavigateAsync(Uri baseUrl) => PerformNavigationAsync(baseUrl, "");

    public async Task AssertIsLoadedAsync()
    {
        await PrimaryActionButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }
}
