using FluentValidation;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;

namespace Vigil;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services
            .Configure<VigilOptions>(builder.Configuration.Bind)
            .AddSingleton<IValidateOptions<VigilOptions>, VigilOptionsValidator>()
            .AddSingleton<ClientKeyRepository>()
            .AddOpenApi()
            .AddProblemDetails()
            .AddValidatorsFromAssemblyContaining(typeof(Program), includeInternalTypes: true);

        builder.Services
            .AddOptions<VigilOptions>()
            .ValidateOnStart();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();
        
        await app
            .MapEndpoints()
            .RunAsync();
    }
}