using AzraTasks.UITests.PageObjects;

namespace AzraTasks.UITests;

public class AuthTests : UITestBase
{
    [Test]
    public async Task CanRegisterAndLoginWithNewAccount()
    {
        var registerPage = new RegisterPage(Page);
        await registerPage.NavigateAsync(FrontendBaseUri);
        await registerPage.RegisterAsync(TestEmail, TestPassword);

        await Assert.That(await registerPage.IsConfirmationMessageVisibleAsync()).IsTrue().Because("User should be redirected to lists after registration");

        var loginPage = new LoginPage(Page);
        await Assert.That(await loginPage.IsLoggedInAsync()).IsTrue().Because("User should be logged in after successful registration");

        await loginPage.LogoutAsync();
        
        await loginPage.NavigateAsync(FrontendBaseUri);
        await loginPage.LoginAsync(TestEmail, TestPassword);

        await Assert.That(await loginPage.IsLoggedInAsync()).IsTrue().Because("User should be logged in after successful login");
    }

    [Test]
    [Category(TestCategories.Accessibility)]
    public async Task LoginPageIsAccessible()
    {
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync(FrontendBaseUri);
        await AssertNoAccessibilityViolations();
    }

    [Test]
    [Category(TestCategories.Accessibility)]
    public async Task RegisterPageIsAccessible()
    {
        RegisterPage registerPage = new(Page);
        await registerPage.NavigateAsync(FrontendBaseUri);
        await AssertNoAccessibilityViolations();
    }
}

