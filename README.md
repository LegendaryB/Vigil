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

## Configuration

Vigil is configured via standard ASP.NET Core configuration (e.g.
environment variables, `appsettings.json`):

| Setting         | Description                                      | Default     |
|------------------|--------------------------------------------------|-------------|
| `DataDirectory`  | Directory where Vigil stores its JSON data files | `./data`    |
| `AdminKey`       | Shared secret required to call admin endpoints   | *(required)* |
