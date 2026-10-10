using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using UserDirectory.Application.Users.Dtos;
using Xunit;

namespace UserDirectory.IntegrationTests;

public class UsersEndpointTests : IClassFixture<UserDirectoryApiFactory>, IAsyncLifetime
{
    private readonly UserDirectoryApiFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointTests(UserDirectoryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateUserRequest ValidRequest(string name = "Ada Lovelace", string pincode = "600001") =>
        new(name, 36, "London", "Greater London", pincode);

    [Fact]
    public async Task GetAll_ReturnsEmptyArrayWhenNoUsersExist()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        users.Should().BeEmpty();
    }

    [Fact]
    public async Task Post_CreatesUserAndReturnsCreatedWithLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/users", ValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<UserResponse>();
        created!.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("Ada Lovelace");

        var followUp = await _client.GetAsync(response.Headers.Location);
        followUp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_ReturnsBadRequestWithFieldLevelErrorsForInvalidPayload()
    {
        var invalid = new CreateUserRequest("A", 200, "", "", "1");

        var response = await _client.PostAsJsonAsync("/api/users", invalid);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Keys.Should().BeEquivalentTo("Name", "Age", "City", "State", "Pincode");
    }

    [Fact]
    public async Task Post_ReturnsConflictForADuplicateNameAndPincode()
    {
        await _client.PostAsJsonAsync("/api/users", ValidRequest());

        var response = await _client.PostAsJsonAsync("/api/users", ValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("Duplicate user");
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForAMalformedBody()
    {
        using var content = new StringContent("{ not json }", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/users", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForAnUnknownIdentifier()
    {
        var response = await _client.GetAsync("/api/users/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("User not found");
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForANonNumericIdentifier()
    {
        var response = await _client.GetAsync("/api/users/abc");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_UpdatesAnExistingUser()
    {
        var created = await CreateUserAsync(ValidRequest());

        var update = new UpdateUserRequest("Grace Hopper", 45, "New York", "New York", "10001");
        var response = await _client.PutAsJsonAsync($"/api/users/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<UserResponse>();
        updated!.Name.Should().Be("Grace Hopper");
        updated.UpdatedAtUtc.Should().BeOnOrAfter(created.UpdatedAtUtc);
    }

    [Fact]
    public async Task Put_ReturnsNotFoundForAnUnknownIdentifier()
    {
        var update = new UpdateUserRequest("Grace Hopper", 45, "New York", "New York", "10001");

        var response = await _client.PutAsJsonAsync("/api/users/4242", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_ReturnsConflictWhenTheNewNameAndPincodeBelongToAnotherUser()
    {
        await CreateUserAsync(ValidRequest("Ada Lovelace", "600001"));
        var second = await CreateUserAsync(ValidRequest("Grace Hopper", "10001"));

        var update = new UpdateUserRequest("Ada Lovelace", 45, "New York", "New York", "600001");
        var response = await _client.PutAsJsonAsync($"/api/users/{second.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_AllowsResubmittingTheSameNameAndPincodeForTheSameUser()
    {
        var created = await CreateUserAsync(ValidRequest());

        var update = new UpdateUserRequest("Ada Lovelace", 37, "London", "Greater London", "600001");
        var response = await _client.PutAsJsonAsync($"/api/users/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_RemovesAnExistingUser()
    {
        var created = await CreateUserAsync(ValidRequest());

        var response = await _client.DeleteAsync($"/api/users/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var followUp = await _client.GetAsync($"/api/users/{created.Id}");
        followUp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ReturnsNotFoundForAnUnknownIdentifier()
    {
        var response = await _client.DeleteAsync("/api/users/5151");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ReturnsUsersSortedByName()
    {
        await CreateUserAsync(ValidRequest("Zara Khan", "110001"));
        await CreateUserAsync(ValidRequest("Ada Lovelace", "600001"));

        var users = await _client.GetFromJsonAsync<List<UserResponse>>("/api/users");

        users!.Select(user => user.Name).Should().Equal("Ada Lovelace", "Zara Khan");
    }

    [Fact]
    public async Task Health_ReportsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/users", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }
}
