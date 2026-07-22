using FluentValidation;
using Vigil.Configuration;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Features;

namespace Vigil;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        
        builder.Services
            .Configure<VigilOptions>(builder.Configuration.Bind)
            .AddSingleton<ClientKeyRepository>()
            .AddOpenApi()
            .AddProblemDetails()
            .AddValidatorsFromAssemblyContaining(typeof(Program), includeInternalTypes: true);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();
        
        await app
            .MapEndpoints()
            .RunAsync();
    }
}