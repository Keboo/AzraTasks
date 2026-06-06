namespace AzraTasks.UITests.PageObjects;

/// <summary>
/// Page Object Model for the Login page
/// </summary>
public class LoginPage(IPage page): TestPageBase(page)
{
    private ILocator EmailInput => Page.GetByTestId("email-input").Locator("input");
    private ILocator PasswordInput => Page.GetByTestId("password-input").Locator("input");
    private ILocator LoginButton => Page.GetByTestId("login-button");
    private ILocator LogoutButton => Page.GetByTestId("nav-logout-button");
    private ILocator ListsButton => Page.GetByTestId("nav-lists-button");

    public Task NavigateAsync(Uri baseUrl) => PerformNavigationAsync(baseUrl, "login");

    public async Task LoginAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        
        await LoginButton.ClickAsync();
        await Page.WaitForURLAsync("**/lists", new PageWaitForURLOptions { Timeout = 30000 });
    }
    
    public async Task<bool> IsLoggedInAsync()
    {
        var url = Page.Url;
        
        if (url.Contains("/lists"))
        {
            return true;
        }
        
        if (url.Contains("/login"))
        {
            return false;
        }
        
        var logoutButtonCount = await LogoutButton.CountAsync();
        var listsButtonCount = await ListsButton.CountAsync();
        
        return logoutButtonCount > 0 || listsButtonCount > 0;
    }
    
    public async Task LogoutAsync()
    {
        await LogoutButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
