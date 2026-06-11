// created in module 4 session 2
// Inside EnrollmentWorker.cs...

using Microsoft.Extensions.DependencyInjection;

public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        // TODO2: Create a short-lived scope using the injected factory.
        using var scope = scopeFactory.CreateScope();

        // TODO3: Resolve the scoped service from the new scope's provider.
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // TODO4: Use the service, then let the 'using' block dispose the scope
        // and its scoped services automatically.
        var enrollments = svc.GetAllAsync().Result;

        // Example: process each enrollment
        foreach (var enrollment in enrollments)
        {
            Console.WriteLine(
                $"Processing {enrollment.Id} - {enrollment.StudentId} - {enrollment.CourseCode}");
        }
    }
}