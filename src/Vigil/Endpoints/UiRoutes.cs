namespace Vigil.Endpoints;

internal static class UiRoutes
{
    private const string BaseRoute = "/ui";

    internal const string Root = "/";
    internal const string Login = $"{BaseRoute}/login";
    internal const string Logout = $"{BaseRoute}/logout";
    internal const string Sessions = $"{BaseRoute}/sessions";
    internal const string SessionsTable = $"{Sessions}/table";
    internal const string SessionCloseTemplate = $"{Sessions}/{{id:guid}}/close";

    internal const string ClientKeys = $"{BaseRoute}/client-keys";
    internal const string ClientKeysTable = $"{ClientKeys}/table";
    internal const string ClientKeyDeleteTemplate = $"{ClientKeys}/{{id:guid}}/delete";
    internal const string ClientKeyUpdateTemplate = $"{ClientKeys}/{{id:guid}}/update";

    internal const string EventActions = $"{BaseRoute}/event-actions";
    internal const string EventActionsTable = $"{EventActions}/table";
    internal const string EventActionDeleteTemplate = $"{EventActions}/{{id:guid}}/delete";
    internal const string EventActionUpdateTemplate = $"{EventActions}/{{id:guid}}/update";

    internal static string SessionClose(Guid id) => $"{Sessions}/{id}/close";
    internal static string ClientKeyDelete(Guid id) => $"{ClientKeys}/{id}/delete";
    internal static string ClientKeyUpdate(Guid id) => $"{ClientKeys}/{id}/update";
    internal static string EventActionDelete(Guid id) => $"{EventActions}/{id}/delete";
    internal static string EventActionUpdate(Guid id) => $"{EventActions}/{id}/update";
}
