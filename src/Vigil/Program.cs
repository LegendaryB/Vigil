using FluentValidation;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Vigil.Configuration;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        builder.Services
            .Configure<VigilOptions>(builder.Configuration.Bind)
            .AddSingleton<IValidateOptions<VigilOptions>, VigilOptionsValidator>()
            .AddSingleton<ClientKeyRepository>()
            .AddSingleton<SessionRepository>()
            .AddOpenApi(options => options
                .AddAdminKeySecurityScheme()
                .AddClientKeySecurityScheme())
            .AddProblemDetails()
            .AddValidatorsFromAssemblyContaining(typeof(Program), includeInternalTypes: true);

        builder.Services
            .AddOptions<VigilOptions>()
            .ValidateOnStart();

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .AddPreferredSecuritySchemes(AdminKeySecurityScheme.SchemeId, ClientKeySecurityScheme.SchemeId)
                .AddApiKeyAuthentication(AdminKeySecurityScheme.SchemeId, apiKey =>
                {
                    apiKey.Value = "";
                })
                .AddApiKeyAuthentication(ClientKeySecurityScheme.SchemeId, apiKey =>
                {
                    apiKey.Value = "";
                }));
        }
        
        await app
            .MapEndpoints()
            .RunAsync();
    }
}