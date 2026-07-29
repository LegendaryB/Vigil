using Ardalis.Result;

namespace Vigil.Domain.Errors.EventActions;

internal static class EventActionErrorCatalogExtensions
{
    extension(EventActionErrorCatalog catalog)
    {
        internal Result EventActionNotFound(Guid id)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(id),
                ErrorMessage = EventActionErrorCatalog.EventActionNotFoundMessage(id),
                ErrorCode = catalog.EntityNotFound,
                Severity = ValidationSeverity.Error
            });
        }
    }
}
