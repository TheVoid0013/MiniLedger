using Microsoft.EntityFrameworkCore;
using MiniLedger.Api.Middleware;
using MiniLedger.Application.Abstractions;
using MiniLedger.Application.Behaviors;
using MiniLedger.Domain.Services;
using MiniLedger.Infrastructure;
using MiniLedger.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Error handling -> ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Domain
builder.Services.AddScoped<TransferService>();

// Application (mediator + pipeline)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

// Infrastructure (DbContext, IAppDbContext, ITransactionManager)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Create/upgrade the DB on startup (fine for a learning project)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection(); // dev only, the container will serve plain HTTP
}

app.UseAuthorization();
app.MapControllers();

app.Run();