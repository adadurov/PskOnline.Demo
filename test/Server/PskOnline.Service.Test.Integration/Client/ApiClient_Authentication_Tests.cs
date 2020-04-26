namespace PskOnline.Service.Test.Integration.Client
{
  using log4net.Core;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using NUnit.Framework;
  using PskOnline.Client.Api;
  using PskOnline.Client.Api.OpenId;
  using PskOnline.Components.Log;
  using PskOnline.Service.Test.Integration.TestData;
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;

  [TestFixture]
  public class ApiClient_Authentication_Tests
  {
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;

    public void Dispose()
    {
      _httpClient?.Dispose();
      _httpClient = null;
      _app?.Dispose();
      _app = null;
      LogHelper.ShutdownLogSystem();
    }

    private void InitOnce()
    {
      if (_app != null) return;

      LogHelper.ConfigureConsoleLogger();

      _app = new DefaultWebApplicationFactory();

      var options = new WebApplicationFactoryClientOptions()
      {
        AllowAutoRedirect = false
      };
      _httpClient = _app.CreateClient(/* not using options */);
    }

    [SetUp]
    public void SetUp()
    {
      InitOnce();
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    [Order(0)]
    public void SecureString_ShouldConvert_BackAndForth()
    {
      var insecure = "Hello, world!";
      var secure = insecure.ToSecureString();
      var insecure2 = secure.ToInsecureString();
      Assert.That(insecure2, Is.EqualTo(insecure));
    }

    [Test]
    [Order(1)]
    public void ShouldThrowSecurityException_GivenIncorrectCredentials()
    {
      // Given
      // invalid credentials
      var apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // When
      // sign in is attempted
      AsyncTestDelegate action = async () =>
        await apiClient.SignInWithUserPassword_WithSlug_Async(
          TestUsers.DefaultSiteAdminName, TestUsers.DefaultSiteAdminPass + "-", _httpClient, "");

      // Then
      // UnauthorizedAccessException is thrown
      var ex = Assert.ThrowsAsync<AuthenticationException>(action);

      Console.WriteLine(ex.Message);
    }

    [Test]
    [Order(2)]
    public async Task ShouldAuthenticateSuccessfully()
    {
      // given
      var apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // When
      await apiClient.SignInWithUserPassword_WithSlug_Async(
        TestUsers.DefaultSiteAdminName,
        TestUsers.DefaultSiteAdminPass,
        _httpClient, "");

      // then
      // no exception is thrown
    }

    [Test]
    [Order(3)]
    public void ShouldThrowBadRequestException_GivenBadCredentials()
    {
      // given
      var apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      // When
      // bad password is supplied
      AsyncTestDelegate action = async () => await apiClient.SignInWithUserPasswordAsync(
        TestUsers.DefaultSiteAdminName,
        "bad_password");

      // then
      // 
      Assert.ThrowsAsync<AuthenticationException>(action);
    }

    [Test]
    [Order(4)]
    public async Task ShouldRefreshTokenSuccessfully()
    {
      // Given
      // a client signed in with valid credentials
      var apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      await apiClient.SignInWithUserPassword_WithSlug_Async(
              TestUsers.DefaultSiteAdminName,
              TestUsers.DefaultSiteAdminPass,
              _httpClient, "");

      // When
      // new token is requested
      var newToken = apiClient.RefreshToken();

      // Then
      // No exception is thrown upon the next request
    }

    [Test]
    public async Task OpenIdRenewalHandler_Should_Fire_Token_Updated_Event()
    {
      // Given
      // a client signed in with valid credentials
      var apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      var tokens = await apiClient.SignInWithUserPasswordAsync(
                            TestUsers.DefaultSiteAdminName,
                            TestUsers.DefaultSiteAdminPass);

      var authHandler = new ResourceOwnerPasswordHandler(
        _httpClient,
        TestUsers.DefaultSiteAdminName.ToSecureString(),
        TestUsers.DefaultSiteAdminPass.ToSecureString());

      // with the OpenIdRenewalHandler
      var oidHandler = new OpenIdRenewalHandler(
        _httpClient,
        tokens,
        authHandler,
        _app.GetLogger<OpenIdRenewalHandler>());

      // and an event handler hooked up to the event
      bool tokenUpdatedEventFired = false;
      void TokenUpdatedEventHandler(TokenHolder newTokens)
      {
        tokenUpdatedEventFired = true;
      }
      oidHandler.TokenUpdated += TokenUpdatedEventHandler;

      // When
      // the renewal handler updates the tokens,
      await oidHandler.RefereshAuthenticationAsync();

      // Then
      // the event should have fired
      Assert.IsTrue(tokenUpdatedEventFired);
    }

  }
}
