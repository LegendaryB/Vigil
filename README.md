# Vigil

Vigil answers one question: is a job, script, or scheduled task actually
running, and did it actually finish? A client "checks in" when it starts
and "checks out" when it's done. If it checks in but then goes quiet
without checking out (crashes, hangs), Vigil notices and can call a
webhook or run a local script to react. It only tracks clients that have
checked in at least once; a job that never runs at all isn't detected.

For developers: it's a small self-hosted HTTP API with two static API
keys (admin and client), file-based storage, and a background dispatcher
that fires webhooks/commands on session events.

## Run it

**Docker Compose** (no published image, builds from source):

```bash
git clone https://git.throwingbits.de/LegendaryB/Vigil.git
cd Vigil
mkdir -p secrets
echo -n "your-admin-key-here" > secrets/admin_key.txt
docker compose up
```

Listens on `http://localhost:8080`.

**Without Docker** (.NET 10 SDK required):

```bash
AdminKey=your-admin-key-here dotnet run --project src/Vigil
```

Dev mode exposes a Scalar UI at `/scalar/v1` and the OpenAPI doc at
`/openapi/v1.json`.

## Dashboard

A small browser dashboard is available at `/ui` (always on, not dev-only).
Log in with the admin key; it sets an `HttpOnly`, `SameSite=Strict`
session cookie, separate from the `Admin-Key` header the JSON API uses.

Covers Sessions (see who's checked in, filter/close stuck sessions, view
metadata), Client Keys (create/delete), and Event Actions (create/delete
webhook or command targets).

## Configuration

| Setting                       | Description                                                  | Default      |
|--------------------------------|----------------------------------------------------------------|--------------|
| `AdminKey`                     | Shared secret for admin endpoints. No default; startup fails without it. | *(required)* |
| `DataDirectory`                | Where `client-keys.json`, `sessions.json`, `event-actions.json` are stored | `./data`     |
| `EventActions:CheckInTimeout`  | `TimeSpan` before an unresponsive session fires `ClientOverdue`. `null` disables it. | `null`       |

Standard ASP.NET Core configuration sources (`appsettings.json`, environment variables). Any
value can also be provided as a Docker secret file at `/run/secrets/<Key>`
(e.g. `/run/secrets/AdminKey`); this is a no-op if that path doesn't exist.

Logging: Serilog, console + rolling file (`logs/`, 14-day retention, 10 MB/file).

## Auth

Two static API-key schemes, checked via SHA-256 hash comparison
(`CryptographicOperations.FixedTimeEquals`):

| Header       | Scope                                            |
|--------------|---------------------------------------------------|
| `Admin-Key`  | Admin endpoints (client-key mgmt, sessions list, event actions) |
| `Client-Key` | Per-client key issued via the admin API; used for check-in/check-out/heartbeat |

## API

All routes are under `/api/v1`.

### Client Keys (`Admin-Key`)

| Method | Route                       | Body                              | Notes                          |
|--------|-----------------------------|------------------------------------|---------------------------------|
| POST   | `/client-keys/`             | `{ "ClientName": string }`         | `ClientName` required, unique (case-insensitive) |
| GET    | `/client-keys/`             | none                                | Returns `ApiKey` in full        |
| DELETE | `/client-keys/{id}`         | none                                 |                                  |

`ApiKey` is a base64-encoded 32-byte random value (`RandomNumberGenerator`), returned in full. It's never regenerated or masked.

`GET /client-keys/` also returns `LastUsedAt`: the last time that key was used to check in or check out (not updated by `heartbeat`). `null` if the key has never been used, useful for spotting keys nobody's using anymore.

### Sessions

| Method | Route                          | Auth        | Body                                   | Notes                                  |
|--------|----------------------------------|------------|------------------------------------------|------------------------------------------|
| POST   | `/sessions/check-in`            | `Client-Key`| `{ "Metadata": { [key]: string } }?`     | Fails if a session is already open      |
| POST   | `/sessions/check-out`           | `Client-Key`| none                                       | Fails if no open session                |
| POST   | `/sessions/heartbeat`           | `Client-Key`| none                                       | Updates `LastSeenAt`; fails if no open session |
| GET    | `/sessions/`                    | `Admin-Key` | none                                       | Lists all sessions (open + closed)      |
| POST   | `/sessions/{id}/close`          | `Admin-Key` | none                                       | Force-closes an open session; doesn't require the client's own key. Fails if the session doesn't exist or is already closed |

`Metadata` constraints: max 20 entries, keys ≤100 chars, values ≤500 chars (400 if exceeded). Stored with the session, echoed on check-in/check-out/`GET /sessions`, and forwarded into every event for that session (webhook `metadata` field / `VIGIL_METADATA_*` env vars).

`ClientOverdue` timeout is measured from `max(CheckedInAt, LastSeenAt)`, checked every 30s. Calling `heartbeat` before the timeout elapses prevents (and resets) the overdue flag.

### Event Actions (`Admin-Key`)

| Method | Route                        | Notes                    |
|--------|------------------------------|----------------------------|
| POST   | `/event-actions/`            | Create                    |
| GET    | `/event-actions/`            | List                      |
| DELETE | `/event-actions/{id}`        |                            |

Request/response body fields:

| Field         | Type                                                                   | Required |
|----------------|-------------------------------------------------------------------------|----------|
| `Event`        | `ClientCheckedIn`, `ClientCheckedOut`, `AllClientsCheckedOut`, `ClientOverdue`, or `ClientForceCheckedOut` | yes      |
| `Target`       | A webhook or command object, see below                                 | yes      |
| `Name`         | string                                                                  | no       |
| `Description`  | string                                                                  | no       |
| `Priority`     | int, default `0`                                                       | no       |

`Priority`: lower fires first (default `0`). Multiple actions can target the same event; they run in priority order.

**Webhook target:**

```json
{
  "$type": "webhook",
  "Url": "https://example.com/hook",
  "Secret": "string?",
  "Headers": {
    "key": "value"
  }
}
```

- `Secret`: if set, request is signed:
  `X-Vigil-Timestamp: <unix-seconds>`, `X-Vigil-Signature: sha256=<hex>` where `hex = HMAC-SHA256(Secret, "{timestamp}.{rawBody}")`.
- `Headers`: static headers merged into every request (e.g. `Authorization`).
- Both returned unredacted on `GET`/`POST`.
- POST body: `{ "event", "clientName", "clientKeyId", "sessionId", "occurredAt", "metadata" }`.
- Retry: 3 attempts, exponential backoff (1s base, jittered), honors a `Retry-After` response header, 10s timeout per attempt. Only transient failures (connection errors, timeouts, 5xx) retry; 4xx does not.

**Command target:**

```json
{
  "$type": "command",
  "Command": "notify-send",
  "Arguments": [
    "A client is overdue"
  ],
  "Environment": {
    "KEY": "value"
  }
}
```

- Runs as a plain OS process, no shell (`Process.Start`, `UseShellExecute: false`).
- `Environment`: static env vars merged in first; cannot override the `VIGIL_*` vars below (real event data always wins on key collision). Not retried, no timeout.
- Vigil-injected environment variables:

  | Variable                 | Value                                              |
  |---------------------------|------------------------------------------------------|
  | `VIGIL_EVENT`             | Event name                                           |
  | `VIGIL_CLIENT_NAME`       | Client name, empty if not applicable                 |
  | `VIGIL_CLIENT_KEY_ID`     | Client key ID, empty if not applicable               |
  | `VIGIL_SESSION_ID`        | Session ID, empty if not applicable                  |
  | `VIGIL_OCCURRED_AT`       | ISO 8601 timestamp                                   |
  | `VIGIL_METADATA_<KEY>`    | One per session metadata entry; key uppercased, non-`[A-Z0-9_]` chars replaced with `_` |

Dispatch runs on a background queue and never blocks the triggering request. A failed webhook/command is logged only; it doesn't affect other actions or the request that triggered it.

## Notes & limitations

- Single-instance, file-backed storage (`ConcurrentDictionary` + JSON files under `DataDirectory`, guarded by an in-process semaphore). No clustering; running multiple instances against the same `DataDirectory` will corrupt state.
- Command targets have no execution timeout and no retry; a hung script blocks that event's dispatch indefinitely.
- API keys (`AdminKey`, client keys) don't expire or rotate. Revocation is delete-and-reissue (client keys) or change-and-restart (`AdminKey`). `LastUsedAt` on client keys helps spot unused ones manually.
