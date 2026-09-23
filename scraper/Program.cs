using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.Core.Interfaces;
using Scraper.Core.Services;
using Scraper.Core.Settings;
using scraper;
using backend.Data;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<IScraper, BarboraScraper>();


var host = builder.Build();
await host.RunAsync();