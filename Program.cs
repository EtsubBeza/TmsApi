//Module 4 Session 1
//added in m5 s1
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Interfaces;
using TmsApi.Services;
using TmsApi.Exceptions;
using TmsApi.Filters;
//

//added in session 3 exc 7
using Scalar.AspNetCore;
//
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);



// added in session3
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddProblemDetails();

//added in session 3 exc 7
builder.Services.AddOpenApi();

//
// Services
// Authentication + Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Training";
    options.DefaultChallengeScheme = "Training";
})
.AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
    "Training",
    null
);

// added in session 2


builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

//added after session 3 as homework
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IAssessmentService, AssessmentService>();
//

builder.Services.AddScoped<ICourseService, CourseService>();

//added in M6 Session 1
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
//
// added in m5 s1
// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // Log SQL to output window
.EnableSensitiveDataLogging()); // Show parameters in querylogs (dev only)
//

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

//

builder.Services.AddAuthorization();

var app = builder.Build();

//added in session 3 exc 7
// Development: expose OpenAPI and Scalar
// Production: use exception handler
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
}

// Request logging middleware should be first
// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

// TODO 1
app.UseRouting();

// TODO 2
app.UseAuthentication();
app.UseAuthorization();

//added in session 3 exc 6
app.UseStatusCodePages();

// TODO 3
// Protected endpoint
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

//added in session 3
app.MapControllers();

//added in session 3 exc 6
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});




if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}
//
app.Run();