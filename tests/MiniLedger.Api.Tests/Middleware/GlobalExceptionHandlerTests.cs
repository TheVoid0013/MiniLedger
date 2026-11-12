namespace MiniLedger.Api.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    public static TheoryData<Exception, int, string> Mappings => new()
    {
        { new AccountNotFoundException(Guid.NewGuid()), 404, "Account not found" },
        { new InsufficientFundsException(), 422, "Insufficient funds" },
        { new InvalidAmountException(), 400, "Business rule violated" },
        { new SameAccountTransferException(), 400, "Business rule violated" },
        { new CurrencyMismatchException(Currency.NPR, new Currency("USD")), 400, "Business rule violated" },
        { new ArgumentException("bad input"), 400, "Invalid input" },
        { new DbUpdateConcurrencyException("conflict"), 409, "Concurrent update, please retry" },
        { new InvalidOperationException("boom"), 500, "Unexpected error" },
    };

    [Theory]
    [MemberData(nameof(Mappings))]
    public async Task Maps_exception_to_status_and_title(Exception ex, int expectedStatus, string expectedTitle)
    {
        var result = await HandleAsync(ex);

        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.Equal(expectedStatus, result.Problem.Status);
        Assert.Equal(expectedTitle, result.Problem.Title);
    }

    [Fact]
    public async Task Returns_true_to_mark_exception_as_handled()
    {
        var result = await HandleAsync(new InsufficientFundsException());

        Assert.True(result.Handled);
    }

    [Fact]
    public async Task Non_500_detail_is_the_exception_message()
    {
        var ex = new AccountNotFoundException(Guid.NewGuid());

        var result = await HandleAsync(ex);

        Assert.Equal(ex.Message, result.Problem.Detail);
    }

    [Fact]
    public async Task Unexpected_error_in_production_hides_detail()
    {
        var result = await HandleAsync(new InvalidOperationException("secret internals"), "Production");

        Assert.Null(result.Problem.Detail);
    }

    [Fact]
    public async Task Unexpected_error_in_development_includes_full_exception()
    {
        var result = await HandleAsync(new InvalidOperationException("boom"), "Development");

        Assert.Contains("boom", result.Problem.Detail);
        Assert.Contains(nameof(InvalidOperationException), result.Problem.Detail);
    }

    [Fact]
    public async Task Unexpected_error_is_logged_as_error()
    {
        var result = await HandleAsync(new InvalidOperationException("boom"));

        Assert.Contains(LogLevel.Error, result.LogLevels);
    }

    [Fact]
    public async Task Expected_errors_are_not_logged()
    {
        var result = await HandleAsync(new InsufficientFundsException());

        Assert.Empty(result.LogLevels);
    }

    // ---------- helpers ----------

    private static async Task<Result> HandleAsync(Exception ex, string environment = "Production")
    {
        var logger = new CapturingLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger, new FakeEnv(environment));
        var ctx = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        ctx.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        ctx.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            ctx.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return new Result(handled, ctx.Response.StatusCode, problem!, logger.Levels);
    }

    private sealed record Result(bool Handled, int StatusCode, ProblemDetails Problem, List<LogLevel> LogLevels);

    private sealed class FakeEnv(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "MiniLedger.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogLevel> Levels { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) => Levels.Add(logLevel);
    }
}