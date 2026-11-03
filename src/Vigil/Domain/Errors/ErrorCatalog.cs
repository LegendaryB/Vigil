using Vigil.Domain.Errors.Sessions;

namespace Vigil.Domain.Errors;

internal static class ErrorCatalog
{
    internal static SessionErrorCatalog Session { get; } = new();
}
