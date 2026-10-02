global using MiniLedger.Domain.Entities;
global using MiniLedger.Domain.ValueObjects;
global using Microsoft.Extensions.DependencyInjection;
global using MiniLedger.Infrastructure.Persistence;

global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


global using Microsoft.EntityFrameworkCore;          // UseSqlite, AddDbContext
global using Microsoft.Extensions.Configuration;     // IConfiguration
global using Microsoft.Extensions.DependencyInjection; // IServiceCollection, AddScoped
global using MiniLedger.Application.Abstractions;     // IAppDbContext, ITransactionManager (adjust to your actual namespace)