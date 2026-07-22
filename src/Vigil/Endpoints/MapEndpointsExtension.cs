using System.Reflection;

namespace Vigil.Endpoints;

internal static class MapEndpointsExtension
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Contains(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            var mapMethod = type.GetMethod(nameof(IEndpoint.MapEndpoint), BindingFlags.Public | BindingFlags.Static);
            mapMethod?.Invoke(null, [app]);
        }

        return app;
    }
}