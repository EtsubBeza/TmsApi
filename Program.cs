//Module 4 Session 1
using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthorization();

var app = builder.Build();

// Request logging middleware should be first
// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

// TODO 1
app.UseRouting();

// TODO 2
app.UseAuthentication();
app.UseAuthorization();

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

app.Run();