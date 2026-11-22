using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using backend.Repositories;
using backend.Services.Interfaces;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
builder.Services.AddScoped<IStoresRepository, StoresRepository>();
builder.Services.AddScoped<IItemPriceHistoryService, ItemPriceHistoryService>();
builder.Services.AddScoped<IItemSimilarityService, ItemSimilarityService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
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
builder.Services.AddHttpClient<IDistanceProvider, GoogleDistanceProvider>();
var auth0Domain = builder.Configuration["Auth0:Domain"] ?? throw new InvalidOperationException("Auth0 Domain is missing.");
var auth0Audience = builder.Configuration["Auth0:Audience"] ?? throw new InvalidOperationException("Auth0 Audience is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{auth0Domain}/";
        options.Audience = auth0Audience;
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IItemsRepository>();
    var index = scope.ServiceProvider.GetRequiredService<IItemSimilarityIndex>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await StoreLocationSeeder.SeedAsync(context);

    var items = await repo.GetItemsWithPriceAndStoreAsync();
    index.BuildOrUpdateIndex(items);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();