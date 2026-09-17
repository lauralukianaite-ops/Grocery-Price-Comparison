using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.Core.Interfaces;
using Scraper.Core.Services;
using Scraper.Core.Settings;
using Scraper.Core.Settings;

DotNetEnv.Env.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<GlobalSettings>(
    builder.Configuration.GetSection(GlobalSettings.SectionName)
);

builder.Services.AddTransient<IDatabaseConnectionFactory, DatabaseConnectionFactory>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
    factory.TestOptions();
}

await host.RunAsync();