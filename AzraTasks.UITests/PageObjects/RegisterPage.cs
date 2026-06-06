namespace AzraTasks.UITests.PageObjects;

/// <summary>
/// Page Object Model for the Register page
/// </summary>
public class RegisterPage(IPage page) : TestPageBase(page)
{
    private ILocator EmailInput => Page.GetByTestId("email-input").Locator("input");
    private ILocator PasswordInput => Page.GetByTestId("password-input").Locator("input");
    private ILocator ConfirmPasswordInput => Page.GetByTestId("confirm-password-input").Locator("input");
    private ILocator RegisterButton => Page.GetByTestId("register-button");

    public Task NavigateAsync(Uri baseUrl) => PerformNavigationAsync(baseUrl, "register");

    public async Task RegisterAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        await ConfirmPasswordInput.FillAsync(password);
        
        await RegisterButton.ClickAsync();
        await Page.WaitForURLAsync("**/lists", new PageWaitForURLOptions { Timeout = 30000 });
    }
    
    public async Task<bool> IsConfirmationMessageVisibleAsync()
    {
        await Task.CompletedTask;
        return Page.Url.Contains("/lists");
    }
    
    public async Task<string> GetEmailConfirmationLinkAsync()
    {
        await Task.CompletedTask;
        return string.Empty;
    }

    public async Task ConfirmAccountAsync()
    {
        await Task.CompletedTask;
    }
}
