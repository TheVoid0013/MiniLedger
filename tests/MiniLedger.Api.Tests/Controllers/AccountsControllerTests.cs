namespace MiniLedger.Api.Tests.Controllers;

public sealed class AccountsControllerTests : IClassFixture<MiniLedgerFactory>
{
    private readonly HttpClient _client;

    public AccountsControllerTests(MiniLedgerFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ThenGet_ReturnsTheCreatedAccount()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/accounts", new { owner = "Alice", currency = "USD" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(id, fetched.GetProperty("id").GetGuid());
    }
}