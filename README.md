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
metadata), Client Keys (create/delete, optionally grouped), and Event
Actions (create/delete webhook or command targets, optionally scoped to a
group). Every table column with meaningful distinct values (Status, Type,
Event, Group) has an Excel-style filter dropdown; a "Reset filters" button
clears all of them at once.

## Configuration

| Setting                       | Description                                                  | Default      |
|--------------------------------|----------------------------------------------------------------|--------------|
| `AdminKey`                     | Shared secret for admin endpoints. No default; startup fails without it. | *(required)* |
| `DataDirectory`                | Where `client-keys.json`, `sessions.json`, `event-actions.json` are stored | `./data`     |
| `EventActions:CheckInTimeout`  | `TimeSpan` before an unresponsive session fires `ClientOverdue`. `null` disables it. | `null`       |
| `EventActions:GroupCompletionTimeout` | `TimeSpan` a client group can stay incomplete before firing `GroupCompletionTimedOut`. Checked every 30s. `null` disables it. | `null`       |
| `EventActions:CommandTimeout`  | `TimeSpan` a command is allowed to run before being killed and the attempt counted as failed (subject to retry). `null` disables it — scripts can run indefinitely. | `null`       |

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
| POST   | `/client-keys/`             | `{ "ClientName": string, "Group": string? }` | `ClientName` required, unique (case-insensitive); `Group` optional |
| GET    | `/client-keys/`             | none                                | Returns `ApiKey` in full        |
| DELETE | `/client-keys/{id}`         | none                                 |                                  |

`ApiKey` is a base64-encoded 32-byte random value (`RandomNumberGenerator`), returned in full. It's never regenerated or masked.

`GET /client-keys/` also returns `LastUsedAt`: the last time that key was used to check in or check out (not updated by `heartbeat`). `null` if the key has never been used, useful for spotting keys nobody's using anymore.

`Group` is a free-text label with no server-side list of valid values; it's only used to scope `GroupCheckedOut`/`GroupCompletionTimedOut` event actions (see below) to a subset of clients, and to power the Group filter/grouping in the dashboard.

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
| `Event`        | `ClientCheckedIn`, `ClientCheckedOut`, `AllClientsCheckedOut`, `ClientOverdue`, `ClientForceCheckedOut`, `GroupCheckedOut`, or `GroupCompletionTimedOut` | yes      |
| `Target`       | A webhook or command object, see below                                 | yes      |
| `Priority`     | int, `>= 1`                                                             | yes      |
| `Group`        | string                                                                  | required if `Event` is `GroupCheckedOut`/`GroupCompletionTimedOut`, otherwise must be omitted |

`Priority`: lower fires first, must be `>= 1` (no usable default — omitting it, or sending `0`, is rejected). Multiple actions can target the same event; they run in priority order.

`GroupCheckedOut` fires once every client key tagged with `Group` has checked out. `GroupCompletionTimedOut` fires instead if `EventActions:GroupCompletionTimeout` elapses before that happens — the two are mutually exclusive per "cycle", so a webhook/command can tell success from timeout apart by which event it received. Both need `Group` set on the event action so dispatch knows which group's completions to react to; every other event type must leave `Group` unset.

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
- POST body: `{ "event", "clientName", "clientKeyId", "sessionId", "occurredAt", "metadata", "group" }`. `clientName`/`clientKeyId`/`sessionId` are `null` for group/system-wide events (`GroupCheckedOut`, `GroupCompletionTimedOut`, `AllClientsCheckedOut`); `group` is only set for the two group events.
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
- `Environment`: static env vars merged in first; cannot override the `VIGIL_*` vars below (real event data always wins on key collision).
- Retry: 3 attempts, exponential backoff (1s base, jittered) — same knobs as webhooks. Any failure retries (nonzero exit code, the process failing to start, or a thrown exception); unlike webhooks there's no 4xx-equivalent "permanent failure" signal in an exit code to key off of, so every failure is treated as potentially transient.
- Timeout: governed by `EventActions:CommandTimeout` (see Configuration), applied per attempt. If it elapses, the process (and its whole tree) is killed before the attempt is retried. Disabled by default — a command can run indefinitely unless configured.
- Vigil-injected environment variables:

  | Variable                 | Value                                              |
  |---------------------------|------------------------------------------------------|
  | `VIGIL_EVENT`             | Event name                                           |
  | `VIGIL_CLIENT_NAME`       | Client name, empty if not applicable                 |
  | `VIGIL_CLIENT_KEY_ID`     | Client key ID, empty if not applicable               |
  | `VIGIL_SESSION_ID`        | Session ID, empty if not applicable                  |
  | `VIGIL_OCCURRED_AT`       | ISO 8601 timestamp                                   |
  | `VIGIL_GROUP`             | Group name, empty if not applicable                  |
  | `VIGIL_METADATA_<KEY>`    | One per session metadata entry; key uppercased, non-`[A-Z0-9_]` chars replaced with `_` |

Dispatch runs on a background queue and never blocks the triggering request. A failed webhook/command is logged only; it doesn't affect other actions or the request that triggered it.

## Notes & limitations

- Single-instance, file-backed storage (`ConcurrentDictionary` + JSON files under `DataDirectory`, guarded by an in-process semaphore). No clustering; running multiple instances against the same `DataDirectory` will corrupt state.
- API keys (`AdminKey`, client keys) don't expire or rotate. Revocation is delete-and-reissue (client keys) or change-and-restart (`AdminKey`). `LastUsedAt` on client keys helps spot unused ones manually.
- Dispatch is a single sequential queue — one event action is awaited fully before the next one starts, across *all* events, not just the one currently firing. A command with retries now enabled against a large `EventActions:CommandTimeout` can block everything else waiting in the queue for up to `attempts × (timeout + backoff)`, worse than a single hung attempt used to block for. Configure `CommandTimeout` conservatively if this matters to you.
