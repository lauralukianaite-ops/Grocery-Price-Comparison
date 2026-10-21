using System.Text.Json.Serialization;
using backend.Data;
using backend.Repositories;
using backend.Services;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("Database connection details not found in .env file!");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);
app.MapControllers();

app.Run();