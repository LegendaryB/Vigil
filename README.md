# Vigil

Vigil is a coordination service. Clients report their state — checking in
when they start and checking out when they finish — so you always have a
clear, current picture of who's running, who's finished, and who's gone
silent.

## How it works

- Each client is issued its own API key (a "client key"), managed through an
  admin-only API.
- Clients use that key to check in when they start and check out when
  they're done. Each check-in/check-out pair is tracked as a "session".
- A client can only have one open session at a time — checking in again
  before checking out is rejected, and checking out closes that client's
  current open session automatically.
- Vigil tracks the state of every client so you can tell at a glance whether
  the whole fleet is done.

## Motivation

Vigil started as a fix for a specific problem: a backup server running jobs
on many clients had no reliable way of knowing whether *all* of them had
actually finished. The check-in/check-out model generalizes beyond that —
anything that needs to report "I started" and "I finished" against a
central tracker (backup jobs, scheduled tasks, worker processes, etc.) fits
the same shape.

## Planned: event actions

Down the line, Vigil should be able to trigger actions on events such as:

- All clients have checked out (everyone's finished)
- A client checks in
- A client checks out
- A client fails to check in/out within an expected window

This will make it possible to, for example, kick off a follow-up job only
once every client has finished, or alert when a client goes silent.

## API

All routes are versioned under `/api/v1`. Admin endpoints require an
`Admin-Key` header matching the configured admin key. Client endpoints
require a `Client-Key` header matching a client's own API key.

### Client Keys (admin-only)

| Method | Route                        | Description                    |
|--------|------------------------------|---------------------------------|
| POST   | `/api/v1/client-keys/`       | Create a new client API key    |
| GET    | `/api/v1/client-keys/`       | List all client API keys       |
| DELETE | `/api/v1/client-keys/{id}`   | Delete a client API key        |

### Sessions

| Method | Route                        | Auth        | Description                              |
|--------|------------------------------|-------------|-------------------------------------------|
| POST   | `/api/v1/sessions/check-in`  | Client key  | Opens a session for the calling client    |
| POST   | `/api/v1/sessions/check-out` | Client key  | Closes the calling client's open session  |
| GET    | `/api/v1/sessions/`          | Admin key   | Lists all sessions (open and closed)      |

### Interactive API docs

In the `Development` environment, Vigil serves an OpenAPI document at
`/openapi/v1.json` and an interactive Scalar UI at `/scalar/v1`, with
endpoints grouped by area ("Client Keys", "Sessions"). Admin and client keys
can each be entered once in Scalar's "Authentication" panel and are then
sent automatically as `Admin-Key`/`Client-Key` on every request you try
from there.

## Configuration

Vigil is configured via standard ASP.NET Core configuration (e.g.
environment variables, `appsettings.json`):

| Setting         | Description                                      | Default      |
|-----------------|---------------------------------------------------|--------------|
| `DataDirectory` | Directory where Vigil stores its JSON data files  | `./data`     |
| `AdminKey`      | Shared secret required to call admin endpoints    | *(required)* |

`AdminKey` has no default and startup fails immediately if it isn't set —
admin-protected endpoints must never run with a fallback key.

## Logging

Vigil logs via Serilog to both the console and rolling daily files under
`logs/` (14-day retention, capped at 10 MB per file before rolling again).
Every HTTP request is logged, alongside the more detailed domain-level
logging in each feature (client key created/deleted, session checked
in/out, rejected auth attempts, etc.). Log level and sinks are configured
under the `Serilog` section in `appsettings.json`.
