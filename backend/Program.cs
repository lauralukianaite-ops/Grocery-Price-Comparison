using backend.Data;
using Microsoft.EntityFrameworkCore;

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
        ?? throw new InvalidOperationException("Data base connection details was not found in .env file!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

const string FrontendCors = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);
app.MapControllers();

app.Run();