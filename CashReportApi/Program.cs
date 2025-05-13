using CashFlow.Domain.Settings;
using CashFlow.Infra.Contexts;
using CashFlow.Infra.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CashierDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));
var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>();
if (redisSettings != null)
{
    var redisConfigString = string.IsNullOrEmpty(redisSettings.Password)
        ? $"{redisSettings.Host}:{redisSettings.Port},abortConnect=false"
        : $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password},abortConnect=false";

    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfigString));
}
else
{
    Console.WriteLine("Warning: Redis settings not found. Service may have limited functionality.");
}

builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
