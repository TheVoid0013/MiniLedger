using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniLedger.Api.Contracts;
using MiniLedger.Application.Accounts;

namespace MiniLedger.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public sealed class AccountsController : ControllerBase
    {
        private readonly ISender _sender;

        public AccountsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountRequest req, CancellationToken ct)
        {
            var account = await _sender.Send(new CreateAccountCommand(req.Owner, req.Currency), ct);
            return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            return Ok(await _sender.Send(new GetAccountQuery(id), ct));
        }

        [HttpGet("{id:guid}/transactions")]
        public async Task<IActionResult> History(Guid id, CancellationToken ct)
        {
            return Ok(await _sender.Send(new GetHistoryQuery(id), ct));
        }

        [HttpPost("{id:guid}/deposit")]
        public async Task<IActionResult> Deposit(Guid id, MoneyRequest req, CancellationToken ct)
        {
            return Ok(await _sender.Send(new DepositCommand(id, req.Amount, req.Currency), ct));
        }

        [HttpPost("{id:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid id, MoneyRequest req, CancellationToken ct)
        {
            return Ok(await _sender.Send(new WithdrawCommand(id, req.Amount, req.Currency), ct));
        }
    }
}