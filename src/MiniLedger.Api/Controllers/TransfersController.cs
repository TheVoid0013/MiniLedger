using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniLedger.Api.Contracts;
using MiniLedger.Application.Accounts;

namespace MiniLedger.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public sealed class TransfersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Transfer(TransferRequest req, CancellationToken ct)
    {
        await sender.Send(new TransferCommand(req.SourceAccountId, req.DestinationAccountId, req.Amount, req.Currency), ct);
        return NoContent();
    }
}