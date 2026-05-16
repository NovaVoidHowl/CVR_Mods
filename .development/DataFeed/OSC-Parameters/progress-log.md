# Progress Log

## 2026-05-16

- Created feature planning folder: `.development/DataFeed/OSC-Parameters`.
- Reviewed existing DataFeed `/api/v1/osc` REST and WebSocket structure.
- Reviewed CVR's decompiled `OSCAvatarModule` receive path.
- Confirmed CVR exposes incoming avatar OSC parameter events for float, int, bool, and null values.
- Confirmed those events fire after CVR applies the value to a matching current-avatar parameter.
- Noted that raw packet diagnostics require observing `OSCAvatarModule.HandleIncoming`.
- Captured implementation plan, endpoint shape, validation checklist, and risk notes.
- Decided the recent-message ring buffer size should be configurable through MelonPreferences.
- Decided OSC parameter diagnostic state should be cleared on avatar change.
- Decided recent-message string arguments should be truncated by default.
- Decided verbose recent-message argument output should be dynamically switchable through MelonPreferences.
- Decided raw receive diagnostics should be Stage 2 in the same development cycle, after Stage 1 has been compiled and
  tested in CVR.

## Current Status

Stage 2 implementation is in progress.

Completed so far:

- Added API models for OSC parameter values, raw-message placeholders, and arguments.
- Added `IOSCParameterDataReader`.
- Added `OSCParameterDataReader` using CVR's incoming avatar parameter events.
- Added MelonPreferences entries for recent-message capacity, verbose arguments, and argument string limit.
- Added `/api/v1/osc-parameters` REST endpoint.
- Added `/api/v1/osc-parameters` WebSocket endpoint.
- Added avatar-change clearing for OSC parameter diagnostics.
- Updated the DataFeed README with Stage 1 endpoint documentation.
- Verified Stage 1 with `dotnet build DataFeed/DataFeed.csproj -c Release`.
- Added Harmony prefix/postfix tracking for `OSCAvatarModule.HandleIncoming(OscMessage packet)`.
- Added bounded raw receive diagnostics through `recentMessages`.
- Added JSON-safe argument serialization with dynamic verbose/truncated string output.
- Added best-effort correlation between raw received messages and applied avatar parameter events.
- Updated the DataFeed README with Stage 2 recent-message diagnostics.
- Verified Stage 2 with `dotnet build DataFeed/DataFeed.csproj -c Release`.

## Next Step

Test Stage 2 in CVR with valid parameters, missing parameters, unsupported argument types, and avatar changes.
