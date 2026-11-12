namespace MiniLedger.Api.Tests.Controllers;


public sealed class TransfersApiTests : IClassFixture<MiniLedgerFactory>
{
    private readonly HttpClient _client;

    public TransfersApiTests(MiniLedgerFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ---------- helpers ----------

    private async Task<Guid> CreateAccountAsync(string owner, string currency = "USD")
    {
        var res = await _client.PostAsJsonAsync("/api/accounts", new { owner, currency });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateFundedAccountAsync(string owner, decimal amount, string currency = "USD")
    {
        var id = await CreateAccountAsync(owner, currency);
        var res = await _client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { amount, currency });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return id;
    }

    private async Task<decimal> GetBalanceAsync(Guid id)
    {
        var body = await _client.GetFromJsonAsync<JsonElement>($"/api/accounts/{id}");
        return body.GetProperty("balance").GetDecimal();
    }

    private Task<HttpResponseMessage> TransferAsync(Guid source, Guid destination, decimal amount, string currency = "USD") =>
        _client.PostAsJsonAsync("/api/transfers", new
        {
            sourceAccountId = source,
            destinationAccountId = destination,
            amount,
            currency
        });

    // ---------- happy path ----------

    [Fact]
    public async Task Transfer_Returns204_AndMovesMoney()
    {
        var src = await CreateFundedAccountAsync("Alice", 100m);
        var dst = await CreateAccountAsync("Bob");

        var res = await TransferAsync(src, dst, 40m);

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Equal(60m, await GetBalanceAsync(src));
        Assert.Equal(40m, await GetBalanceAsync(dst));
    }

    [Fact]
    public async Task Transfer_EntireBalance_LeavesSourceAtZero()
    {
        var src = await CreateFundedAccountAsync("Alice", 100m);
        var dst = await CreateAccountAsync("Bob");

        var res = await TransferAsync(src, dst, 100m);

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Equal(0m, await GetBalanceAsync(src));
        Assert.Equal(100m, await GetBalanceAsync(dst));
    }

    [Fact]
    public async Task Transfer_RecordsHistoryOnBothAccounts()
    {
        var src = await CreateFundedAccountAsync("Alice", 100m);
        var dst = await CreateAccountAsync("Bob");

        await TransferAsync(src, dst, 25m);

        var srcHistory = await _client.GetFromJsonAsync<JsonElement>($"/api/accounts/{src}/transactions");
        var dstHistory = await _client.GetFromJsonAsync<JsonElement>($"/api/accounts/{dst}/transactions");

        Assert.Equal(2, srcHistory.GetArrayLength()); // deposit + transfer out
        Assert.Equal(1, dstHistory.GetArrayLength()); // transfer in
    }

    [Fact]
    public async Task Transfer_TotalMoneyIsConserved_AcrossMultipleTransfers()
    {
        var a = await CreateFundedAccountAsync("A", 100m);
        var b = await CreateFundedAccountAsync("B", 50m);

        await TransferAsync(a, b, 30m);
        await TransferAsync(b, a, 10m);
        await TransferAsync(a, b, 5m);

        var total = await GetBalanceAsync(a) + await GetBalanceAsync(b);
        Assert.Equal(150m, total);
    }

    // ---------- failure paths: money must not move ----------

    [Fact]
    public async Task Transfer_InsufficientFunds_IsRejected_AndNoBalancesChange()
    {
        var src = await CreateFundedAccountAsync("Alice", 50m);
        var dst = await CreateAccountAsync("Bob");

        var res = await TransferAsync(src, dst, 500m);

        Assert.True((int)res.StatusCode is >= 400 and < 500);
        Assert.Equal(50m, await GetBalanceAsync(src));
        Assert.Equal(0m, await GetBalanceAsync(dst));
    }

    [Fact]
    public async Task Transfer_UnknownDestination_Returns404_AndSourceIsNotDebited()
    {
        // This is the one that proves your TransactionBehavior rolls back:
        // if the debit is saved before the destination lookup fails, this breaks.
        var src = await CreateFundedAccountAsync("Alice", 100m);

        var res = await TransferAsync(src, Guid.NewGuid(), 40m);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal(100m, await GetBalanceAsync(src));
    }

    [Fact]
    public async Task Transfer_UnknownSource_Returns404_AndDestinationIsNotCredited()
    {
        var dst = await CreateAccountAsync("Bob");

        var res = await TransferAsync(Guid.NewGuid(), dst, 40m);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal(0m, await GetBalanceAsync(dst));
    }

    [Fact]
    public async Task Transfer_ToSameAccount_IsRejected_AndBalanceUnchanged()
    {
        var acc = await CreateFundedAccountAsync("Alice", 100m);

        var res = await TransferAsync(acc, acc, 10m);

        Assert.True((int)res.StatusCode is >= 400 and < 500);
        Assert.Equal(100m, await GetBalanceAsync(acc));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-25)]
    public async Task Transfer_NonPositiveAmount_IsRejected_AndNoBalancesChange(decimal amount)
    {
        var src = await CreateFundedAccountAsync("Alice", 100m);
        var dst = await CreateAccountAsync("Bob");

        var res = await TransferAsync(src, dst, amount);

        Assert.True((int)res.StatusCode is >= 400 and < 500);
        Assert.Equal(100m, await GetBalanceAsync(src));
        Assert.Equal(0m, await GetBalanceAsync(dst));
    }

    [Fact]
    public async Task Transfer_CurrencyMismatch_IsRejected_AndNoBalancesChange()
    {
        var src = await CreateFundedAccountAsync("Alice", 100m, "USD");
        var dst = await CreateAccountAsync("Bob", "EUR");

        var res = await TransferAsync(src, dst, 10m, "USD");

        Assert.True((int)res.StatusCode is >= 400 and < 500);
        Assert.Equal(100m, await GetBalanceAsync(src));
        Assert.Equal(0m, await GetBalanceAsync(dst));
    }

    // ---------- request validation ----------

    [Fact]
    public async Task Transfer_WithEmptyBody_Returns4xx()
    {
        var res = await _client.PostAsJsonAsync("/api/transfers", new { });

        Assert.True((int)res.StatusCode is >= 400 and < 500);
    }

    [Fact]
    public async Task Transfer_WithMalformedJson_Returns400()
    {
        var res = await _client.PostAsync(
            "/api/transfers",
            new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}