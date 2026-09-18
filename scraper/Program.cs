using backend.Data;
using Microsoft.EntityFrameworkCore;
using scrapper;

DotNetEnv.Env.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();