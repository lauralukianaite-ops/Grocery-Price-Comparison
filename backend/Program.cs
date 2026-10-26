using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using backend.Repositories;
using backend.Services.Interfaces;
using System.Text.Json.Serialization;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var host = builder.Configuration["GlobalSettings__DatabaseHost"] ?? Environment.GetEnvironmentVariable("GlobalSettings__DatabaseHost");
var port = builder.Configuration["GlobalSettings__DatabasePort"] ?? Environment.GetEnvironmentVariable("GlobalSettings__DatabasePort");
var db = builder.Configuration["GlobalSettings__DatabaseName"] ?? Environment.GetEnvironmentVariable("GlobalSettings__DatabaseName");
var user = builder.Configuration["GlobalSettings__DatabaseUser"] ?? Environment.GetEnvironmentVariable("GlobalSettings__DatabaseUser");
var pass = builder.Configuration["GlobalSettings__DatabasePassword"] ?? Environment.GetEnvironmentVariable("GlobalSettings__DatabasePassword");

string connectionString;

if (!string.IsNullOrEmpty(host))
{
    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SslMode=Require;";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["DATABASE_URL"]
        ?? throw new InvalidOperationException("Database connection details not found in .env file!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddSingleton<IItemSimilarityIndex, ItemSimilarityIndex>();
builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IBasketsRepository, BasketsRepository>();
builder.Services.AddScoped<IItemPriceHistoryService, ItemPriceHistoryService>();
builder.Services.AddScoped<IItemSimilarityService, ItemSimilarityService>();
builder.Services.AddScoped<IFakeDiscountService, FakeDiscountService>();
builder.Services.AddScoped<IBasketService, BasketService>();

const string FrontendCors = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IItemsRepository>();
    var index = scope.ServiceProvider.GetRequiredService<IItemSimilarityIndex>();

    var items = repo.GetItemsWithPriceAndStoreAsync().GetAwaiter().GetResult();
    index.BuildOrUpdateIndex(items);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);
app.MapControllers();

app.Run();