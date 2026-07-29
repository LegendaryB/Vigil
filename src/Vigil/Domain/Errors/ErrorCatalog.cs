using Vigil.Domain.Errors.ClientKeys;
using Vigil.Domain.Errors.EventActions;
using Vigil.Domain.Errors.Sessions;

namespace Vigil.Domain.Errors;

internal static class ErrorCatalog
{
    internal static ClientKeyErrorCatalog ClientKey { get; } = new();
    internal static SessionErrorCatalog Session { get; } = new();
    internal static EventActionErrorCatalog EventAction { get; } = new();
}