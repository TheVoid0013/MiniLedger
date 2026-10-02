using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLedger.Domain.Exceptions;

namespace MiniLedger.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            AccountNotFoundException        => (StatusCodes.Status404NotFound, "Account not found"),
            InsufficientFundsException      => (StatusCodes.Status422UnprocessableEntity, "Insufficient funds"),
            DomainException                 => (StatusCodes.Status400BadRequest, "Business rule violated"),
            ArgumentException               => (StatusCodes.Status400BadRequest, "Invalid input"),
            DbUpdateConcurrencyException    => (StatusCodes.Status409Conflict, "Concurrent update, please retry"),
            _                               => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled exception");

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == 500 ? null : ex.Message
        }, ct);

        return true;
    }
}