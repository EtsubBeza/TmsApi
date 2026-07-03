//Module 4 Session 1
//added in m5 s1
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Interfaces;
using TmsApi.Services;
using TmsApi.Exceptions;
//

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




//added in m5 s1
// Apply migrations and seed test data that verifies if database tables are empty at application startup and populates them
// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
    
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact

var report = await context.Students
    .AsNoTracking()
    .Select(s => new
    {
        s.Name,
        EnrollmentCount = s.Enrollments.Count
    })
    .ToListAsync();

foreach (var r in report)
{
    Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");
}

if (!context.Students.Any())
{
var students = new List<Student>
{
new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
};
context.Students.AddRange(students);
var courses = new List<Course>
{
new() { Code = "CS-101", Title = "Introduction to ComputerScience", MaxCapacity = 30 },
new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity =40 }
};
context.Courses.AddRange(courses);
context.SaveChanges();
var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};
context.Enrollments.AddRange(enrollments);
context.SaveChanges();
}
}
//
app.Run();