using Vigil.Domain.Errors.ClientKeys;

namespace Vigil.Domain.Errors;

internal static class ErrorCatalog
{
    internal static ClientKeyErrorCatalog ClientKey { get; } = new();

}