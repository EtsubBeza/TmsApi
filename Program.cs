//Module 4 Session 1
//added in session 3 exc 7
using Scalar.AspNetCore;
//
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);



// added in session3
builder.Services.AddControllers();
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

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

//added after session 3 as homework
builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddSingleton<IAssessmentService, AssessmentService>();
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

//
app.Run();