namespace PskOnline.Service.Test.Integration.Identity
{
  using System;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc.Testing;
  using NUnit.Framework;
  using PskOnline.Components.Log;

  [TestFixture]
  public class AccessResourceWithoutAuthTests : IDisposable
  {
    DefaultWebApplicationFactory _app;
    HttpClient _client;

    private string Url(string resource) => $"/api/{resource}";

    private void InitOnce()
    {
      if (_app != null) return;
      LogHelper.ConfigureConsoleLogger();

      _app = new DefaultWebApplicationFactory();
      var options = new WebApplicationFactoryClientOptions()
      {
        AllowAutoRedirect = false
      };
      _client = _app.CreateClient(/* not using options */);
      //_client.DefaultRequestHeaders.Add(
      //  "X-Requested-With", "XMLHttpRequest");
      _client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void Dispose()
    {
      _client?.Dispose();
      _client = null;
      _app?.Dispose();
      _app = null;
      LogHelper.ShutdownLogSystem();
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

    /// <summary>
    /// Notice: may fail (return 200 instead of 401) if URL is 'not found'
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    [Test(Author = "Alexey Adadurov")]
    [Category("L1")]
    [TestCase("account/roles")]
    [TestCase("account/users")]
    [TestCase("Tenant")]
    [TestCase("BranchOffice")]
    [TestCase("Department")]
    [TestCase("Position")]
    [TestCase("Employee")]
    [TestCase("Inspection")]
    [TestCase("Test")]
    public async Task POST_Without_Auth_Should_Fail(string resource)
    {
      var newResource = new
      {
        Name = "New resource",
        Address = "821B, Baker Street",
        City = "London",
        PrimaryContactName = "Sherlock Holmes",
        PhoneNumber = "1273",
        AlternameContactName = "Dr. James Watson"
      };
      var response = await _client.PostAsJsonAsync(Url(resource), newResource);
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      Assert.That(response.IsSuccessStatusCode, Is.False);
    }

    /// <summary>
    /// Notice: may fail (return 200 instead of 401) if URL is 'not found'
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    [Test(Author = "Alexey Adadurov")]
    [Category("L1")]
    [TestCase("account/roles")]
    [TestCase("account/users")]
    [TestCase("Tenant")]
    [TestCase("BranchOffice")]
    [TestCase("Department")]
    [TestCase("Position")]
    [TestCase("Employee")]
    [TestCase("Inspection")]
    [TestCase("Test")]
    public async Task GET_Without_Auth_Should_Fail(string resource)
    {
      var response = await _client.GetAsync(
        Url(resource) + "/" + Guid.NewGuid().ToString());
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      Assert.That(response.IsSuccessStatusCode, Is.False);
    }

    /// <summary>
    /// Notice: may fail (return 200 instead of 401) if URL is 'not found'
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    [Test(Author = "Alexey Adadurov")]
    [Category("L1")]
    [TestCase("account/roles")]
    [TestCase("account/users")]
    [TestCase("Tenant")]
    [TestCase("BranchOffice")]
    [TestCase("Department")]
    [TestCase("Position")]
    [TestCase("Employee")]
    [TestCase("Inspection")]
    [TestCase("Test")]
    public async Task PUT_Without_Auth_Should_Fail(string resource)
    {
      var newResource = new
      {
        Name = "New resource",
        Address = "821B, Baker Street",
        City = "London",
        PrimaryContactName = "Sherlock Holmes",
        PhoneNumber = "1273",
        AlternameContactName = "Dr. James Watson"
      };
      var response = await _client.PostAsJsonAsync(
        Url(resource), newResource);
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      Assert.That(response.IsSuccessStatusCode, Is.False);
    }

    /// <summary>
    /// Notice: may fail (return 200 instead of 401) if URL is 'not found'
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    [Test(Author = "Alexey Adadurov")]
    [Category("L1")]
    [TestCase("account/roles")]
    [TestCase("account/users")]
    [TestCase("Tenant")]
    [TestCase("BranchOffice")]
    [TestCase("Department")]
    [TestCase("Position")]
    [TestCase("Employee")]
    [TestCase("Inspection")]
    [TestCase("Test")]
    public async Task DELETE_Without_Auth_Should_Fail(string resource)
    {
      var response = await _client.DeleteAsync(
        Url(resource) + "/" + Guid.NewGuid().ToString());
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      Assert.That(response.IsSuccessStatusCode, Is.False);
    }

    public class StatusDto
    {
      public string ServerName { get; set; }
      public string BuildNumber { get; set; }
      public string Uptime { get; set; }
      public string AppState { get; set; }
      public long Errors { get; set; }
      public bool IsStarted { get; set; }
      public SubSystemStatusDto[] SubSystems { get; set; }
    }

    public class SubSystemStatusDto
    {
      public string Label { get; set; }
      public string Status { get; set; }
      public string Description { get; set; }
    }

    [Test]
    public async Task Should_Get_Status_Without_Auth()
    {
      // Given

      // When
      var response = await _client.GetAsync(Url("status"));
      response.EnsureSuccessStatusCode();
      var dto = await response.Content.ReadAsAsync<StatusDto>();

      // Then
      Assert.That(dto.Uptime, Is.GreaterThan(TimeSpan.FromSeconds(0).ToString()));
      Assert.That(dto.BuildNumber.Length, Is.GreaterThan(4));
    }
  }
}
