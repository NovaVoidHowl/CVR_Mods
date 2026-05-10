# Progress Log

## 2026-05-10

- Created feature planning folder: `.development/DataFeed/OSC-Info`.
- Reviewed existing DataFeed reader/API structure.
- Reviewed decompiled CVR OSC references.
- Confirmed native OSC status is feasible to read from existing referenced assemblies.
- Captured implementation plan and reference notes.
- Decided OSC status should use its own `/api/v1/osc` REST and WebSocket endpoint, rather than being added to
  `/api/v1/realtime` for the first pass.
- Added a DataFeed README availability matrix documenting which current values are exposed through avatar parameters,
  REST, and WebSocket endpoints.
- Decided `oscEnabled`, `oscRunning`, and `dataFeedErrorOSC` should be exposed as avatar parameters. All other OSC
  fields remain API-only.
- Decided the OSC API should expose both `connectedOscClients` and a lightweight `oscClients` array with service ID,
  endpoints, and advertised module flags.
- Noted that OSC client details only cover OSCQuery-discovered clients; plain OSC connections may not appear.
- Implemented `OSCDataReader`, `IOSCDataReader`, and lightweight `OSCClientInfo` API model.
- Added OSC status wiring to DataFeed update state and avatar parameters.
- Added `/api/v1/osc` REST and WebSocket endpoints.
- Updated `DataFeed/README.md` with OSC endpoint data availability and example output.
- Verified with `dotnet build NVH_CVR_Mods.sln`; build and ILRepack completed successfully.

## Current Status

Initial implementation complete.

## Next Step

Test in ChilloutVR with native OSC disabled, enabled, and with an OSCQuery-capable client connected.
