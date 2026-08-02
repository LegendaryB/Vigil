# Vigil

Vigil keeps track of clients that check in when they start something and
check out when they're done, so you know at a glance who's running, who's
finished, and who's gone quiet.

It started as a fix for a backup server that had no way of knowing whether
all its clients had actually finished. The same pattern works for any job,
scheduled task, or worker process that needs to report "I started" / "I
finished" to a central place.

## Running Vigil

With Docker Compose (the primary way to run it). There's no published
image — `compose.yaml` builds it from source, so you need a local clone.

### Clone the repository

```bash
git clone https://git.throwingbits.de/LegendaryB/Vigil.git
cd Vigil
```

### Set the admin key as a Docker secret

```bash
mkdir -p secrets
echo -n "your-admin-key-here" > secrets/admin_key.txt
```

### Start it

```bash
docker compose up
```

### Without Docker

```bash
AdminKey=your-admin-key-here dotnet run --project src/Vigil
```

Vigil listens on `http://localhost:8080`. See
[Configuration](#configuration) below.

## How it works

Each client gets its own API key from an admin-only API. It uses that key
to check in and check out; each pair is tracked as a session. A client can
only have one open session at a time. A client can attach its own metadata
(a flat string map, e.g. job ID, host, version) when it checks in; it's
stored with the session and carried into every event for it.

Optionally, a webhook or local command can fire on check-in, check-out, or
when a client is overdue (see below).

## Event actions

Vigil can trigger a webhook or a local command on:

| Event                  | Fires when                                        |
|------------------------|---------------------------------------------------|
| `ClientCheckedIn`      | A client checks in                                |
| `ClientCheckedOut`     | A client checks out                               |
| `AllClientsCheckedOut` | A check-out leaves no client with an open session |
| `ClientOverdue`        | A session goes unheard-from longer than `CheckInTimeout` |

They're managed through the admin API. Create one with a `Target` of
either a webhook or a command:

```json
{
  "Event": "ClientCheckedOut",
  "Target": {
    "$type": "webhook",
    "Url": "https://example.com/hook"
  }
}
```

```json
{
  "Event": "ClientOverdue",
  "Target": {
    "$type": "command",
    "Command": "notify-send",
    "Arguments": [
      "A client is overdue"
    ]
  }
}
```

`Name`, `Description`, and `Priority` (lower fires first, default `0`) are
optional. `ClientOverdue` is disabled by default — see
`EventActions:CheckInTimeout` under [Configuration](#configuration). The
timeout is measured from whichever is more recent: check-in, or the last
`POST /sessions/heartbeat` call — a long-running client can call
`heartbeat` periodically to avoid being flagged overdue while it's still
legitimately working.

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

Commands run as a plain OS process (no shell). A command target can also
take `Environment`: static environment variables set on every dispatch,
for scripts that need their own config/secrets — same idea as a webhook's
`Headers`. These can't override the `VIGIL_*` variables below; a
colliding key is ignored in favor of the real event data.

Event data itself is passed via environment variables rather than
substituted into arguments:

| Variable               | Value                                                                                                             |
|------------------------|-------------------------------------------------------------------------------------------------------------------|
| `VIGIL_EVENT`          | Event name (e.g. `ClientCheckedIn`)                                                                               |
| `VIGIL_CLIENT_NAME`    | Client name, empty if not applicable                                                                              |
| `VIGIL_CLIENT_KEY_ID`  | Client key ID, empty if not applicable                                                                            |
| `VIGIL_SESSION_ID`     | Session ID, empty if not applicable                                                                               |
| `VIGIL_OCCURRED_AT`    | Timestamp the event occurred, ISO 8601                                                                            |
| `VIGIL_METADATA_<KEY>` | One per session metadata entry (see below), key uppercased with any character outside `[A-Z0-9_]` replaced by `_` |

Dispatch is queued and handled by a background worker, so it never blocks
the request that triggered it. Webhooks get 3 attempts with exponential
backoff (honoring a `Retry-After` header if the receiver sends one) and a
10s per-attempt timeout; only transient failures (connection errors,
timeouts, 5xx) are retried, not 4xx. Commands aren't retried. A failed
webhook or command is logged and otherwise doesn't affect anything else.

## API

Everything's versioned under `/api/v1`. Admin endpoints need an `Admin-Key`
header; client endpoints need a `Client-Key` header.

### Client Keys

| Method | Route                      | Auth  | Description             |
|--------|----------------------------|-------|-------------------------|
| POST   | `/api/v1/client-keys/`     | Admin | Create a client API key |
| GET    | `/api/v1/client-keys/`     | Admin | List client API keys    |
| DELETE | `/api/v1/client-keys/{id}` | Admin | Delete a client API key |

### Sessions

| Method | Route                         | Auth   | Description                                        |
|--------|--------------------------------|--------|------------------------------------------------------|
| POST   | `/api/v1/sessions/check-in`   | Client | Open a session                                     |
| POST   | `/api/v1/sessions/check-out`  | Client | Close the open session                             |
| POST   | `/api/v1/sessions/heartbeat`  | Client | Push back the overdue deadline without closing it  |
| GET    | `/api/v1/sessions/`           | Admin  | List all sessions                                  |

### Event Actions

| Method | Route                        | Auth  | Description            |
|--------|------------------------------|-------|------------------------|
| POST   | `/api/v1/event-actions/`     | Admin | Create an event action |
| GET    | `/api/v1/event-actions/`     | Admin | List event actions     |
| DELETE | `/api/v1/event-actions/{id}` | Admin | Delete an event action |

`check-in` takes an optional body:

```json
{
  "Metadata": {
    "jobId": "123"
  }
}
```

Up to 20 entries, keys up to 100 characters, values up to 500 — an empty
body still works, `Metadata` just comes back `null`. It's echoed on the
check-in/check-out responses and on `GET /sessions`, included in the
webhook body as `metadata`, and passed to commands as
`VIGIL_METADATA_<KEY>` environment variables (keys are uppercased, with
any character outside `[A-Z0-9_]` replaced by `_`).

In `Development`, there's a Scalar UI at `/scalar/v1` (OpenAPI JSON at
`/openapi/v1.json`) where you can enter your admin/client key once and have
it sent automatically.

## Configuration

Standard ASP.NET Core configuration (`appsettings.json`, environment
variables, etc.):

| Setting                       | Description                                                                                     | Default      |
|-------------------------------|-------------------------------------------------------------------------------------------------|--------------|
| `DataDirectory`               | Where Vigil stores `client-keys.json`, `sessions.json`, `event-actions.json`                    | `./data`     |
| `AdminKey`                    | Shared secret for admin endpoints                                                               | *(required)* |
| `EventActions:CheckInTimeout` | How long a session can stay open before `ClientOverdue` fires; `null` disables overdue checking | `null`       |

There's no fallback `AdminKey`. Vigil refuses to start without one set.

Any setting can also come from a Docker secret: files under
`/run/secrets/<Key>` (e.g. `/run/secrets/AdminKey`) are read as config
values, so a secret can be mounted instead of set as a plain environment
variable. `compose.yaml` does this for `AdminKey` already; create
`secrets/admin_key.txt` (gitignored) with the raw key before running
`docker compose up`. This is a no-op outside Docker (no `/run/secrets`,
no effect).

## Logging

Serilog, to console and to rolling daily files under `logs/` (14-day
retention, 10 MB per file). Configured under `Serilog` in
`appsettings.json`.
