using CashFlow.CashierApi.Validations;
using CashFlow.Domain.Settings;
using CashFlow.Infra.Contexts;
using CashFlow.Infra.Messaging;
using CashFlow.Infra.Repositories;
using CashFlow.Infra.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CashPostingRequestValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CashierDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

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
builder.Services.AddSingleton<QueueProducer>();

builder.Services.AddScoped<ICashPostingService, CashPostingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMainRepository, MainRepository>();

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
