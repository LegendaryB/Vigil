# Vigil

Vigil is a check-in/check-out service for backup clients.

## The problem

A backup server can run jobs on many clients, but it has no reliable way of
knowing whether *all* of them actually finished. Vigil closes that gap: each
client reports in when it starts and again when it's done, so the server
always has a clear, current picture of which clients are still running,
which have finished, and which never showed up at all.

## How it works

- Each client is issued its own API key (a "client key"), managed through an
  admin-only API.
- Clients use that key to check in when a backup starts and check out when
  it completes.
- Vigil tracks the state of every client so you can tell at a glance whether
  the whole fleet is done.

## Planned: event actions

Down the line, Vigil should be able to trigger actions on events such as:

- All clients have checked out (the full run is complete)
- A client checks in
- A client checks out
- A client fails to check in/out within an expected window

This will make it possible to, for example, kick off a follow-up job only
once every client has finished, or alert when a client goes silent.

## API

All admin endpoints require an `X-Admin-Key` header matching the configured
admin key.

| Method | Route                     | Description                    |
|--------|---------------------------|---------------------------------|
| POST   | `/api/client-keys/`       | Create a new client API key    |
| GET    | `/api/client-keys/`       | List all client API keys       |
| DELETE | `/api/client-keys/{id}`   | Delete a client API key        |

Check-in/check-out endpoints for clients are not implemented yet.

### Interactive API docs

In the `Development` environment, Vigil serves an OpenAPI document at
`/openapi/v1.json` and an interactive Scalar UI at `/scalar/v1`. The admin
key can be entered once in Scalar's "Authentication" panel and it's then
sent automatically as `X-Admin-Key` on every request you try from there.

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
logging in each feature (client key created/deleted, rejected admin-key
attempts, etc.). Log level and sinks are configured under the `Serilog`
section in `appsettings.json`.
