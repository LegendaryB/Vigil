using Ardalis.Result;
using Vigil.Domain.Sessions;

namespace Vigil.Domain.Errors.Sessions;

internal static class SessionErrorCatalogExtensions
{
    extension(SessionErrorCatalog catalog)
    {
        internal Result<Session> AlreadyCheckedIn(string clientName)
        {
            return Result<Session>.Invalid(new ValidationError
            {
                Identifier = nameof(Session.ClientKeyId),
                ErrorMessage = SessionErrorCatalog.AlreadyCheckedInMessage(clientName),
                ErrorCode = catalog.AlreadyCheckedInCode,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result<Session> NoOpenSession(string clientName)
        {
            return Result<Session>.Invalid(new ValidationError
            {
                Identifier = nameof(Session.ClientKeyId),
                ErrorMessage = SessionErrorCatalog.NoOpenSessionMessage(clientName),
                ErrorCode = catalog.NoOpenSessionCode,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result<Session> InvalidMetadata(string reason)
        {
            return Result<Session>.Invalid(new ValidationError
            {
                Identifier = nameof(Session.Metadata),
                ErrorMessage = SessionErrorCatalog.InvalidMetadataMessage(reason),
                ErrorCode = catalog.InvalidMetadataCode,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result<Session> SessionNotFound(Guid id)
        {
            return Result<Session>.Invalid(new ValidationError
            {
                Identifier = nameof(Session.Id),
                ErrorMessage = SessionErrorCatalog.SessionNotFoundMessage(id),
                ErrorCode = catalog.EntityNotFound,
                Severity = ValidationSeverity.Error
            });
        }

        internal Result<Session> AlreadyClosed(Guid id)
        {
            return Result<Session>.Invalid(new ValidationError
            {
                Identifier = nameof(Session.CheckedOutAt),
                ErrorMessage = SessionErrorCatalog.AlreadyClosedMessage(id),
                ErrorCode = catalog.AlreadyClosedCode,
                Severity = ValidationSeverity.Error
            });
        }
    }
}
