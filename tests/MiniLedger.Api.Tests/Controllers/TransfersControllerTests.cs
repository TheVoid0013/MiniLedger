namespace MiniLedger.Api.Tests.Controllers;

public sealed class TransfersControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly TransfersController _sut;

    public TransfersControllerTests()
    {
        _sut = new TransfersController(_sender);
    }

    [Fact]
    public async Task Transfer_ReturnsNoContent()
    {
        var req = new TransferRequest(Guid.NewGuid(), Guid.NewGuid(), 50m, "USD");

        var result = await _sut.Transfer(req, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Transfer_MapsRequestToCommand()
    {
        var src = Guid.NewGuid();
        var dst = Guid.NewGuid();

        await _sut.Transfer(new TransferRequest(src, dst, 75.25m, "NPR"), CancellationToken.None);

        // Assumes TransferCommand is a record (value equality). If it's a class,
        // switch to c => c.SourceAccountId == src && ... (adjust property names).
        await _sender.Received(1).Send(
            Arg.Is<TransferCommand>(c => c == new TransferCommand(src, dst, 75.25m, "NPR")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Transfer_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        await _sut.Transfer(new TransferRequest(Guid.NewGuid(), Guid.NewGuid(), 1m, "USD"), cts.Token);

        await _sender.Received(1).Send(Arg.Any<TransferCommand>(), cts.Token);
    }

    [Fact]
    public async Task Transfer_WhenSenderThrows_ExceptionPropagates()
    {
        _sender.Send(Arg.Any<TransferCommand>(), Arg.Any<CancellationToken>())
               .ThrowsAsync(new InvalidOperationException("Insufficient funds"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Transfer(new TransferRequest(Guid.NewGuid(), Guid.NewGuid(), 1m, "USD"), CancellationToken.None));
    }

    [Fact]
    public async Task Transfer_SendsExactlyOneCommand()
    {
        await _sut.Transfer(new TransferRequest(Guid.NewGuid(), Guid.NewGuid(), 10m, "USD"), CancellationToken.None);

        await _sender.ReceivedWithAnyArgs(1).Send(default(TransferCommand)!, default);
    }
}