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

        internal Result InvalidPriority()
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = "priority",
                ErrorMessage = EventActionErrorCatalog.InvalidPriorityMessage,
                ErrorCode = catalog.InvalidPriority,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result GroupRequired()
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = "group",
                ErrorMessage = EventActionErrorCatalog.GroupRequiredMessage,
                ErrorCode = catalog.GroupRequired,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result GroupNotAllowed()
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = "group",
                ErrorMessage = EventActionErrorCatalog.GroupNotAllowedMessage,
                ErrorCode = catalog.GroupNotAllowed,
                Severity = ValidationSeverity.Error
            });
        }
    }
}
