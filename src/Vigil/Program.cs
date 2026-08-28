using System.Text.Json.Serialization;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Vigil.Configuration;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Events.Groups;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        builder.Services
            .Configure<VigilOptions>(builder.Configuration.Bind)
            .AddSingleton<IValidateOptions<VigilOptions>, VigilOptionsValidator>()
            .AddSingleton<ClientKeyRepository>()
            .AddSingleton<SessionRepository>()
            .Configure<EventActionsOptions>(builder.Configuration.GetSection(EventActionsOptions.ConfigurationKey))
            .AddSingleton<EventActionQueue>()
            .AddSingleton<EventActionRepository>()
            .AddSingleton<DispatchLogRepository>()
            .AddSingleton<GroupCompletionTracker>()
            .AddHostedService<EventActionDispatchService>()
            .AddHostedService<SessionOverdueMonitor>()
            .AddHostedService<GroupCompletionTimeoutMonitor>()
            .AddHttpClient(nameof(EventActionDispatchService))
            .AddWebhookRetryHandler()
            .Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        builder.Services
            .AddOpenApi(options => options
                .AddAdminKeySecurityScheme()
                .AddClientKeySecurityScheme()
                .AddUniqueNestedTypeSchemaIds())
            .AddExceptionHandler<JsonRequestExceptionHandler>()
            .AddProblemDetails()
            .AddValidatorsFromAssemblyContaining(typeof(Program), includeInternalTypes: true)
            .ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services
            .AddOptions<VigilOptions>()
            .ValidateOnStart();

        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = UiRoutes.Login;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
                options.SlidingExpiration = true;
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseExceptionHandler();

        app.UseSerilogRequestLogging();

        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

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