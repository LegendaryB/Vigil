using Ardalis.Result;

namespace Vigil.Domain.Errors.Security;

internal static class SecurityErrorCatalogExtensions
{
    extension(SecurityErrorCatalog catalog)
    {
        internal static Result AdminKeyInvalid()
        {
            return Result.Unauthorized(
                "A valid admin key is required to perform this action.");
        }
    }
}
