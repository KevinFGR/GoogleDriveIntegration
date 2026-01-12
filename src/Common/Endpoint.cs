using System.ComponentModel.DataAnnotations;
using GoogleDriveIntegration.src.Controllers;

namespace GoogleDriveIntegration.src.Common;
public static class Endpoint
{
    public static void MapEndpoints(this WebApplication app)
    {
        RouteGroupBuilder endpoints = app.MapGroup("");

        endpoints.MapGroup("/").WithTags("Health Check").MapGet("/", () => new { success = true });
        endpoints.MapGroup("api/drive").WithTags("drive").MapEndpoint<DriveController>();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
    where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}