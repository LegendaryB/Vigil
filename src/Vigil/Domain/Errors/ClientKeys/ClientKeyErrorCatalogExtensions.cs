using Ardalis.Result;
using Vigil.Domain.ClientKeys;

namespace Vigil.Domain.Errors.ClientKeys;

internal static class ClientKeyErrorCatalogExtensions
{
    extension(ClientKeyErrorCatalog catalog)
    {
        internal Result<ClientKey> ClientNameMustBeUnique()
        {
            return Result<ClientKey>.Invalid(new ValidationError
            {
                Identifier = nameof(ClientKey.ClientName),
                ErrorMessage = ClientKeyErrorCatalog.ClientNameMustBeUniqueMessage(),
                ErrorCode = catalog.PropertyValueMustBeUnique,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result NotFound(Guid id)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(id),
                ErrorMessage = ClientKeyErrorCatalog.ClientKeyNotFoundMessage(id),
                ErrorCode = catalog.EntityNotFound,
                Severity = ValidationSeverity.Error
            });
        }
    }
}