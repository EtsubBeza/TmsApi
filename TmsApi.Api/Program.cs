// Module 4 Session 1
// added in m5 s1

using MediatR;
using FluentValidation;

using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;

using TmsApi.Api.ExceptionHandlers;

using Microsoft.EntityFrameworkCore;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
using TmsApi.Api.Exceptions;
using TmsApi.Api.Filters;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Persistence.Services;
using TmsApi.Api.Authentication;
using TmsApi.Application.Options;
using TmsApi.Api.Middleware;
using TmsApi.Infrastructure.Persistence.Seed;

// Scalar
using Scalar.AspNetCore;

using Microsoft.AspNetCore.Authentication;
using Asp.Versioning;


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


// FluentValidation

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);


// Pipeline Behaviors
// IMPORTANT: Logging must be registered first

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


// Global Exception Handler

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
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

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



// ===============================
// Database
// ===============================

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")));



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


// V1 Deprecation headers

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


    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();


    await DataSeeder.SeedAsync(context);
}



app.Run();