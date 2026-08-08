using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Vigil.Endpoints;

internal static class MapEndpointsExtension
{
    extension(WebApplication app)
    {
        public WebApplication MapEndpoints()
        {
            return app
                .MapApiEndpoints()
                .MapUiEndpoints();
        }

        private WebApplication MapApiEndpoints()
        {
            var versionedApi = app.NewVersionedApi()
                .MapGroup(Routes.ApiBaseRoute)
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

        private WebApplication MapUiEndpoints()
        {
            var uiEndpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Contains(typeof(IUiEndpoint)));

            foreach (var type in uiEndpointTypes)
            {
                var mapMethod = type.GetMethod(nameof(IUiEndpoint.MapEndpoint), BindingFlags.Public | BindingFlags.Static);
                mapMethod?.Invoke(null, [app]);
            }

            return app;
        }
    }
}
