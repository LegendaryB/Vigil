# Vigil

Vigil keeps track of clients that check in when they start something and
check out when they're done, so you know at a glance who's running, who's
finished, and who's gone quiet.

It started as a fix for a backup server that had no way of knowing whether
all its clients had actually finished. The same pattern works for any job,
scheduled task, or worker process that needs to report "I started" / "I
finished" to a central place.

## How it works

Each client gets its own API key from an admin-only API. It uses that key
to check in and check out; each pair is tracked as a session. A client can
only have one open session at a time.

Optionally, a webhook or local command can fire on check-in, check-out, or
when a client is overdue (see below).

## Event actions

Vigil can trigger a webhook or a local command on:

| Event                  | Fires when                                        |
|-------------------------|----------------------------------------------------|
| `ClientCheckedIn`      | A client checks in                                 |
| `ClientCheckedOut`     | A client checks out                                |
| `AllClientsCheckedOut` | A check-out leaves no client with an open session  |
| `ClientOverdue`        | A session stays open longer than `CheckInTimeout`  |

They're managed through the admin API. Create one with a `Target` of
either a webhook or a command:

```json
{
  "Event": "ClientCheckedOut",
  "Target": { "$type": "webhook", "Url": "https://example.com/hook" }
}
```

```json
{
  "Event": "ClientOverdue",
  "Target": { "$type": "command", "Command": "notify-send", "Arguments": ["A client is overdue"] }
}
```

`Name`, `Description`, and `Priority` (lower fires first, default `0`) are
optional. `CheckInTimeout` lives under `EventActions` in `appsettings.json`
and is `null` (disabled) by default.

Webhooks get a small JSON body (`event`, `clientName`, `clientKeyId`,
`sessionId`, `occurredAt`). A webhook target can also take:

- `Secret`: signs the request with `X-Vigil-Timestamp` and
  `X-Vigil-Signature: sha256=<hex>`, where the hex is
  `HMAC-SHA256(Secret, "{timestamp}.{body}")`, so the receiver can verify
  it actually came from Vigil.
- `Headers`: static headers sent with the request, for endpoints that
  need their own auth (e.g. `Authorization: Bearer ...`).

Both are returned in full on `GET`/`POST`, same as `ClientKey.ApiKey`
elsewhere in the API. Nothing here is redacted.

Commands run as a plain OS process (no shell), with event data passed via
environment variables (`VIGIL_EVENT`, `VIGIL_CLIENT_NAME`, etc.) rather
than substituted into arguments.

Dispatch is queued and handled by a background worker, so it never blocks
the request that triggered it. A failed webhook or command is logged and
otherwise doesn't affect anything else (there's no retry yet, see
Roadmap).

## API

Everything's versioned under `/api/v1`. Admin endpoints need an `Admin-Key`
header; client endpoints need a `Client-Key` header.

| Method | Route                         | Auth    | Description                   |
|--------|-------------------------------|---------|--------------------------------|
| POST   | `/api/v1/client-keys/`        | Admin   | Create a client API key       |
| GET    | `/api/v1/client-keys/`        | Admin   | List client API keys          |
| DELETE | `/api/v1/client-keys/{id}`    | Admin   | Delete a client API key       |
| POST   | `/api/v1/sessions/check-in`   | Client  | Open a session                |
| POST   | `/api/v1/sessions/check-out`  | Client  | Close the open session        |
| GET    | `/api/v1/sessions/`           | Admin   | List all sessions             |
| POST   | `/api/v1/event-actions/`      | Admin   | Create an event action        |
| GET    | `/api/v1/event-actions/`      | Admin   | List event actions            |
| DELETE | `/api/v1/event-actions/{id}`  | Admin   | Delete an event action        |

In `Development`, there's a Scalar UI at `/scalar/v1` (OpenAPI JSON at
`/openapi/v1.json`) where you can enter your admin/client key once and have
it sent automatically.

## Configuration

Standard ASP.NET Core configuration (`appsettings.json`, environment
variables, etc.):

| Setting         | Description                                | Default      |
|-----------------|----------------------------------------------|--------------|
| `DataDirectory` | Where Vigil stores its JSON data files       | `./data`     |
| `AdminKey`      | Shared secret for admin endpoints             | *(required)* |

There's no fallback `AdminKey`. Vigil refuses to start without one set.

## Logging

Serilog, to console and to rolling daily files under `logs/` (14-day
retention, 10 MB per file). Configured under `Serilog` in
`appsettings.json`.

## Roadmap

- Retry/backoff and an explicit timeout for webhook dispatch
- SSRF hardening on webhook URLs
- Dependencies between event actions (run B only after A)
- Session metadata: let a client attach arbitrary data (job ID, host,
  version) on check-in, surfaced in `GET /sessions` and passed to event
  actions

Not planned: per-service payload formats (Discord, Slack, etc.) built into
Vigil. A relay like [Apprise](https://github.com/caronc/apprise) fits
better in front of the generic webhook.
