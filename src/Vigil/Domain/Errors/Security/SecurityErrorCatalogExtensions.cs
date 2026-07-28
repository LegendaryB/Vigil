using Ardalis.Result;

namespace Vigil.Domain.Errors.Security;

internal static class SecurityErrorCatalogExtensions
{
    extension(SecurityErrorCatalog catalog)
    {
        internal static Result AdminKeyInvalid()
        {
            return Result.Unauthorized(
                SecurityErrorCatalog.AdminKeyInvalidMessage());
        }

        internal static Result ClientKeyInvalid()
        {
            return Result.Unauthorized(
                SecurityErrorCatalog.ClientKeyInvalidMessage());
        }
    }
}
