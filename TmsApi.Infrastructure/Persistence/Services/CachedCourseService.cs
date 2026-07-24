using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Caching;
using TmsApi.Infrastructure.Persistence;


namespace TmsApi.Infrastructure.Persistence.Services;

public class CachedCourseService(
    HybridCache cache,
    TmsDbContext context,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        var key =
            $"{CacheKeys.CoursesAll}:page:{request.Page}:size:{request.PageSize}";

        var dbHit = false;


        var result = await cache.GetOrCreateAsync(
            key,
            request,
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);


                IQueryable<Course> query =
                    context.Courses.AsNoTracking();


                var totalCount =
                    await query.CountAsync(token);


                var items =
                    await query
                        .OrderBy(c => c.Title)
                        .Skip(
                            (state.Page - 1)
                            * state.PageSize)
                        .Take(state.PageSize)
                        .Select(c => new CourseResponseDto(
                            c.Id,
                            c.Code,
                            c.Title,
                            c.MaxCapacity,
                            c.Enrollments.Count))
                        .ToListAsync(token);


                return new PagedResponse<CourseResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = state.Page,
                    PageSize = state.PageSize
                };
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);


        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return result;
    }


    public async Task<CourseResponseDto?> GetCourseAsync(
        string code,
        CancellationToken ct)
    {
        var key = CacheKeys.Course(code);

        var dbHit = false;


        var result = await cache.GetOrCreateAsync(
            key,
            code,
            async (courseCode, token) =>
            {
                dbHit = true;


                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);


                return await context.Courses
                    .AsNoTracking()
                    .Where(c => c.Code == courseCode)
                    .Select(c => new CourseResponseDto(
                        c.Id,
                        c.Code,
                        c.Title,
                        c.MaxCapacity,
                        c.Enrollments.Count))
                    .FirstOrDefaultAsync(token);
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);


        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return result;
    }


    public async Task InvalidateCourseCacheAsync(
        CancellationToken ct)
    {
        logger.LogInformation(
            "Invalidating cache tag {Tag}",
            CacheKeys.CoursesTag);


        await cache.RemoveByTagAsync(
            CacheKeys.CoursesTag,
            ct);
    }
}

public interface ICachedCourseService
{
    Task<CourseResponseDto?> GetCourseAsync(
        string code,
        CancellationToken ct);


    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct);


    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}