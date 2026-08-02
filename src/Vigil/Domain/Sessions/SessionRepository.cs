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

    public async Task<Result<Session>> HeartbeatAsync(
        ClientKey client,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            var openSession = Entities.Values.FirstOrDefault(
                s => s.ClientKeyId == client.Id && s.CheckedOutAt is null);

            if (openSession is null)
            {
                Logger.LogNoOpenSessionForHeartbeat(client.ClientName);
                return ErrorCatalog.Session.NoOpenSession(client.ClientName);
            }

            var updatedSession = openSession with { LastSeenAt = DateTime.UtcNow };

            Entities[updatedSession.Id] = updatedSession;

            return Result.Success(updatedSession);
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        Logger.LogHeartbeatReceived(result.Value.ClientName, result.Value.Id);

        await PersistAsync(cancellationToken);

        return result;
    }

    public async Task<Result<Session>> ForceCheckOutAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            if (!Entities.TryGetValue(sessionId, out var session))
            {
                Logger.LogSessionNotFoundForForceCheckOut(sessionId);
                return ErrorCatalog.Session.SessionNotFound(sessionId);
            }

            if (session.CheckedOutAt is not null)
            {
                Logger.LogSessionAlreadyClosed(sessionId);
                return ErrorCatalog.Session.AlreadyClosed(sessionId);
            }

            var closedSession = session with { CheckedOutAt = DateTime.UtcNow };

            Entities[closedSession.Id] = closedSession;

            return Result.Success(closedSession);
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        Logger.LogSessionForceClosed(result.Value.ClientName, result.Value.Id);

        await PersistAsync(cancellationToken);

        return result;
    }

    internal bool HasAnyOpenSession() => Entities.Values.Any(s => s.CheckedOutAt is null);
}
