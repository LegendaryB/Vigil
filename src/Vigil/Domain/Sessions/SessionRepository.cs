using Ardalis.Result;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Data;
using Vigil.Domain.Errors;
using Vigil.Domain.Errors.Sessions;

namespace Vigil.Domain.Sessions;

internal sealed class SessionRepository : JsonFileRepository<Session>
{
    protected override Func<Session, Guid> PrimaryKeySelector => session => session.Id;

    public SessionRepository(
        ILogger<SessionRepository> logger,
        IOptions<VigilOptions> options)
        : base(logger, options.Value.SessionsFilePath)
    {
    }

    public async Task<Result<Session>> CheckInAsync(
        ClientKey client,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            var hasOpenSession = Entities.Values.Any(
                s => s.ClientKeyId == client.Id && s.CheckedOutAt is null);

            if (hasOpenSession)
            {
                Logger.LogClientAlreadyCheckedIn(client.ClientName);
                return ErrorCatalog.Session.AlreadyCheckedIn(client.ClientName);
            }

            var session = new Session(
                Guid.NewGuid(),
                client.Id,
                client.ClientName,
                DateTime.UtcNow,
                null,
                metadata
            );

            Entities[session.Id] = session;

            return Result.Success(session);
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        Logger.LogClientCheckedIn(result.Value.ClientName, result.Value.Id);

        await PersistAsync(cancellationToken);

        return result;
    }

    public async Task<Result<Session>> CheckOutAsync(
        ClientKey client,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            var openSession = Entities.Values.FirstOrDefault(
                s => s.ClientKeyId == client.Id && s.CheckedOutAt is null);

            if (openSession is null)
            {
                Logger.LogNoOpenSessionForCheckOut(client.ClientName);
                return ErrorCatalog.Session.NoOpenSession(client.ClientName);
            }

            var closedSession = openSession with { CheckedOutAt = DateTime.UtcNow };

            Entities[closedSession.Id] = closedSession;

            return Result.Success(closedSession);
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        Logger.LogClientCheckedOut(result.Value.ClientName, result.Value.Id);

        await PersistAsync(cancellationToken);

        return result;
    }

    internal bool HasAnyOpenSession() => Entities.Values.Any(s => s.CheckedOutAt is null);
}
