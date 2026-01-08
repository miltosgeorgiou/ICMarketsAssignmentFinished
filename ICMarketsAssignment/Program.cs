using FluentValidation;
using ICMarketsAssignment.AppDatabaseContext;
using ICMarketsAssignment.HttpClients;
using ICMarketsAssignment.HttpClients.impl;
using ICMarketsAssignment.Repositories;
using ICMarketsAssignment.Repositories.impl;
using ICMarketsAssignment.Services;
using ICMarketsAssignment.Services.impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;
using System.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// here i register my DatabaseContext and SQlite and i use the connection string by a name that is located in my appsettings.json file.
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("ICMarketsDefaultDatabaseConnection")));

// here i register the HttpClient, the default URL is in the appsettings.json file.I also use a retryer of 5 times increasing the retry period expotentialy
builder.Services.AddHttpClient<IBlockCypherClient, BlockCypherClientService>((sp, http) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["BlockCypher:BaseUrl"];

    http.BaseAddress = new Uri(baseUrl!);
    http.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
    .AddResilienceHandler("blockcypher", (resilience, context) =>
    {
        var logger = context.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("BlockCypherRetry");

        resilience.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential, // expotential is 1, 2,4,8 16 
            UseJitter = true,

            // Retry transient failures + 429
            ShouldHandle = args =>
            {
                var isTransient = HttpClientResiliencePredicates.IsTransient(args.Outcome); // here i am checking for 5XX status codes.

                var is429 = args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests; //429 is too many requests

                return ValueTask.FromResult(isTransient || is429);
            },

            OnRetry = args =>
            {
                var statusCode = args.Outcome.Result?.StatusCode;
                var exception = args.Outcome.Exception;

                logger.LogWarning(
                    exception,
                    "BlockCypher call retry {Attempt}/{Max}. Delay: {Delay}. Status: {StatusCode}",
                    args.AttemptNumber,
                    5,
                    args.RetryDelay,
                    statusCode);

                return ValueTask.CompletedTask;
            }
        });
    });

builder.Services.AddScoped<ISymbolsService, SymbolsService>();
builder.Services.AddScoped<ISymbolRepository, SymbolRepository>();

//i have added the HealthCheck and a check for the database connectivity.
builder.Services.AddHealthChecks().AddDbContextCheck<DatabaseContext>("sqlite");


//Here i will add the CORS policies.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsForTheICMarketsAssignment", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//Validation of my requests dtos.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


var app = builder.Build();

app.UseCors("DefaultCorsForTheICMarketsAssignment");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
public partial class Program { }
