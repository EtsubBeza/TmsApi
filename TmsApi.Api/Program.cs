// Module 4 Session 1
// added in m5 s1

using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

using MediatR;
using FluentValidation;

using Asp.Versioning;

using Scalar.AspNetCore;

using Microsoft.Extensions.Caching.Hybrid;

using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Application.DTOs;
using TmsApi.Application.Options;

using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Exceptions;
using TmsApi.Api.Filters;
using TmsApi.Api.Authentication;
using TmsApi.Api.Middleware;
using TmsApi.Api.RateLimiting;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Services;
using TmsApi.Infrastructure.Persistence.Seed;


var builder = WebApplication.CreateBuilder(args);



// ===============================
// Controllers + Filters
// ===============================

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});


builder.Services.AddProblemDetails();



// ===============================
// CQRS + MediatR
// ===============================

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentCommand).Assembly));



// ===============================
// FluentValidation
// ===============================

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);



// ===============================
// Pipeline Behaviors
// ===============================

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));


builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));



// ===============================
// Global Exception Handler
// ===============================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();



// ===============================
// Versioned OpenAPI
// ===============================

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});


builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});



// ===============================
// API Versioning
// ===============================

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
        new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified =
        true;

    options.ReportApiVersions = true;

    options.ApiVersionReader =
        new UrlSegmentApiVersionReader();

})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});



// ===============================
// Authentication
// ===============================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Training";

    options.DefaultChallengeScheme = "Training";
})
.AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
    "Training",
    null);



// ===============================
// Application Services
// ===============================

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IAssessmentService, AssessmentService>();

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();



// ===============================
// Hybrid Cache
// ===============================

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions =
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10),

            LocalCacheExpiration =
                TimeSpan.FromMinutes(2)
        };
});



// ===============================
// Rate Limiting
// ===============================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var (partitionKey, tier) =
                    ApiKeyResolver.Resolve(httpContext);


                return tier switch
                {
                    ApiKeyTier.Paid =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"paid:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 200,

                                TokensPerPeriod = 100,

                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),

                                QueueLimit = 0,

                                AutoReplenishment = true
                            }),


                    ApiKeyTier.Free =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"free:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 30,

                                TokensPerPeriod = 10,

                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),

                                QueueLimit = 0,

                                AutoReplenishment = true
                            }),


                    _ =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"anon:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 10,

                                TokensPerPeriod = 5,

                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),

                                QueueLimit = 0,

                                AutoReplenishment = true
                            })
                };
            });



    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;



    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";


        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retry))
        {
            retryAfter =
                ((int)retry.TotalSeconds).ToString();
        }


        context.HttpContext.Response.Headers.RetryAfter =
            retryAfter;


        context.HttpContext.Response.ContentType =
            "application/problem+json";


        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                Title = "Rate limit exceeded",

                Detail =
                    $"Too many requests. Retry after {retryAfter} seconds.",

                Status =
                    StatusCodes.Status429TooManyRequests,

                Type =
                    "https://tms.local/errors/rate_limit_exceeded"
            },
            ct);
    };



    // ===============================
    // Transcript Concurrency Limiter
    // ===============================

    options.AddConcurrencyLimiter(
        "transcripts",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;

            limiterOptions.QueueLimit = 20;

            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });
});


// ===============================
// Database
// ===============================

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "TmsDatabase")));



// ===============================
// Options
// ===============================

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();



// ===============================
// DI Validation
// ===============================

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;

    options.ValidateOnBuild = true;
});


builder.Services.AddAuthorization();



var app = builder.Build();



// ===============================
// OpenAPI + Scalar
// ===============================

app.MapOpenApi();


app.MapScalarApiReference(options =>
{
    options.WithTitle("TMS API Reference")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);


    options.AddDocument(
        "v1",
        "API Version 1.0");


    options.AddDocument(
        "v2",
        "API Version 2.0");
});



// ===============================
// Middleware Pipeline
// ===============================

app.UseExceptionHandler();


app.UseMiddleware<RequestLoggingMiddleware>();


app.UseHttpsRedirection();


app.UseRouting();


app.UseRateLimiter();


app.UseMiddleware<V1DeprecationMiddleware>();


app.UseAuthentication();

app.UseAuthorization();


app.UseStatusCodePages();



// ===============================
// Protected Endpoint
// ===============================

app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
})
.RequireAuthorization();



// ===============================
// Controllers
// ===============================

app.MapControllers();



// ===============================
// Error Testing Endpoint
// ===============================

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});



// ===============================
// Development Seeding
// ===============================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();


    var context =
        scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();


    await DataSeeder.SeedAsync(context);
}



app.Run();