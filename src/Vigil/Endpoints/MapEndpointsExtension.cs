using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Vigil.Endpoints;

internal static class MapEndpointsExtension
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var versionedApi = app.NewVersionedApi()
            .MapGroup("/api/v{version:apiVersion}")
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions();

        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Contains(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            var mapMethod = type.GetMethod(nameof(IEndpoint.MapEndpoint), BindingFlags.Public | BindingFlags.Static);
            mapMethod?.Invoke(null, [versionedApi]);
        }

        return app;
    }
}
