# Implementation Plan

## Summary

Add an OSC avatar-parameter diagnostics reader to DataFeed and expose it through a new REST and WebSocket endpoint.

The feature should complement the existing `/api/v1/osc` endpoint. `/api/v1/osc` answers "is OSC configured and
running?", while `/api/v1/osc-parameters` should answer "which OSC avatar parameters has CVR received or applied?".

Use two implementation layers:

1. Applied parameter tracking through CVR's existing incoming avatar parameter events.
2. Raw receive tracking through a Harmony hook on `OSCAvatarModule.HandleIncoming`.

Both layers are part of this development cycle, but they should be implemented in stages. Stage 1 should be compiled and
tested in CVR before Stage 2 starts. The first layer is lower risk and proves the endpoint, API shape, event
subscriptions, avatar-change clearing, and preference wiring. The second layer is more useful for debugging bad
addresses, bad argument types, and parameters missing from the current avatar.

## Staging

### Stage 1: Applied Parameter Diagnostics

Implement the endpoint using CVR's incoming avatar parameter events only.

This stage should include:

- Models and reader interface
- `OSCParameterDataReader`
- MelonPreferences for ring buffer capacity, verbose arguments, and truncation length
- REST endpoint `/api/v1/osc-parameters`
- WebSocket endpoint `/api/v1/osc-parameters`
- Avatar-change clearing
- README/API docs

Stage 1 validation should include a successful compile, loading the mod in CVR, sending valid OSC values to the current
avatar, and confirming applied values appear in the endpoint.

### Stage 2: Raw Receive Diagnostics

After Stage 1 has been compiled and tested in CVR, add the raw receive hook.

This stage should include:

- Harmony prefix/postfix for `OSCAvatarModule.HandleIncoming`
- Recent raw message ring buffer
- Handled/rejected result capture
- Best-effort correlation between raw received messages and later applied parameter events
- Validation for bad parameter names, unsupported argument types, and messages that reach CVR but do not apply

## Proposed Output Shape

Use stable, explicit field names:

```json
{
  "oscEnabled": true,
  "oscRunning": true,
  "currentAvatarId": "avtr-example",
  "knownParameterCount": 2,
  "recentMessageCount": 3,
  "parameters": [
    {
      "name": "HeartRate",
      "address": "/avatar/parameters/HeartRate",
      "valueType": "float",
      "lastValue": 82.0,
      "lastReceivedAt": "2026-05-16T14:20:12.533Z",
      "receivedCount": 42,
      "appliedToAvatar": true
    },
    {
      "name": "ToggleThing",
      "address": "/avatar/parameters/ToggleThing",
      "valueType": "bool",
      "lastValue": true,
      "lastReceivedAt": "2026-05-16T14:20:14.120Z",
      "receivedCount": 3,
      "appliedToAvatar": true
    }
  ],
  "recentMessages": [
    {
      "address": "/avatar/parameters/BadName",
      "parameterName": "BadName",
      "argumentCount": 1,
      "arguments": [
        {
          "valueType": "int",
          "value": 1
        }
      ],
      "receivedAt": "2026-05-16T14:20:15.000Z",
      "handledByCVR": true,
      "appliedToAvatar": false,
      "reason": "No matching current avatar parameter"
    }
  ],
  "dataFeedErrorOSCParameters": false
}
```

Notes:

- `parameters` should be keyed internally by parameter name.
- `lastReceivedAt` and `receivedAt` should use UTC ISO-8601 strings.
- `lastValue` should preserve bool/int/float/null values as JSON-native values.
- `arguments` in `recentMessages` should be bounded and JSON-safe.
- `recentMessages` should be a ring buffer, not an unbounded list.
- `currentAvatarId` should be included so clients can reset their own view when the avatar changes.
- Parameter state should be cleared on avatar change so each avatar debug session starts with a clean slate.
- Recent message string arguments should be truncated by default.
- Full verbose argument output should be controlled by a MelonPreferences setting that can be changed while the mod is
  running.

## Code Changes

### 1. Add Models

Create small API models under `DataFeed/Models`.

Suggested models:

- `OSCParameterInfo`
- `OSCParameterMessageInfo`
- `OSCParameterArgumentInfo`

Expected `OSCParameterInfo` fields:

- `string Name`
- `string Address`
- `string ValueType`
- `object LastValue`
- `DateTime LastReceivedAt`
- `long ReceivedCount`
- `bool AppliedToAvatar`

Expected `OSCParameterMessageInfo` fields:

- `string Address`
- `string ParameterName`
- `int ArgumentCount`
- `IReadOnlyList<OSCParameterArgumentInfo> Arguments`
- `DateTime ReceivedAt`
- `bool HandledByCVR`
- `bool AppliedToAvatar`
- `string Reason`

### 2. Add Interface

Create `DataFeed/Interfaces/IOSCParameterDataReader.cs`.

Expected members:

- `void Initialize()`
- `void Dispose()`
- `void Clear()`
- `bool UpdateOSCParameterState()`
- `IReadOnlyList<OSCParameterInfo> Parameters { get; }`
- `IReadOnlyList<OSCParameterMessageInfo> RecentMessages { get; }`
- `int KnownParameterCount { get; }`
- `int RecentMessageCount { get; }`
- `bool DataFeedErrorOSCParameters { get; }`

`Initialize()` should subscribe to CVR events and install optional Harmony patches.
`Dispose()` should unsubscribe from CVR events and remove any owned Harmony patches.

### 3. Add Reader

Create `DataFeed/Services/OSCParameterDataReader.cs`.

Initial implementation:

- Subscribe to:
  - `OSCAvatarModule.OnIncomingAvatarFloatOSCParameter`
  - `OSCAvatarModule.OnIncomingAvatarIntOSCParameter`
  - `OSCAvatarModule.OnIncomingAvatarBoolOSCParameter`
  - `OSCAvatarModule.OnIncomingAvatarNullOSCParameter`
- For each callback:
  - Record parameter name.
  - Build address as `/avatar/parameters/{name}`.
  - Store value type and last value.
  - Increment receive count.
  - Set `AppliedToAvatar = true`.
  - Update `LastReceivedAt`.
- Use a private lock around dictionaries and buffers.
- Return cloned/snapshot lists from public properties so API serialization does not enumerate mutable state.

The DataFeed mod may receive callbacks from CVR job queues rather than from DataFeed's update path, so the reader should
avoid touching Unity objects inside the callback.

### 4. Add Preferences

Add MelonPreferences entries for the diagnostics reader.

Suggested entries:

- `OSC Parameter Recent Message Capacity`
  - Type: `int`
  - Default: `128`
  - Purpose: maximum number of raw/recent OSC parameter messages retained.
- `OSC Parameter Verbose Arguments`
  - Type: `bool`
  - Default: `false`
  - Purpose: controls whether recent-message arguments include full string values or truncated/debug-safe values.
- `OSC Parameter Argument String Limit`
  - Type: `int`
  - Default: `128`
  - Purpose: maximum string length when verbose argument output is disabled.

The reader should re-read these preference values during update or through preference-change callbacks if available, so
the verbose output behavior can be switched without restarting CVR.

### 5. Stage 2 Raw Receive Hook

Add a Harmony patch for:

- `ABI_RC.Systems.OSC.Modules.OSCAvatarModule.HandleIncoming(OscMessage packet)`

Recommended behavior:

- Prefix:
  - Capture address, arguments, parameter name, and timestamp for `/avatar/parameters/...`.
- Postfix:
  - Capture CVR's returned `bool __result`.
  - Mark whether CVR considered the packet handled.
  - Mark `appliedToAvatar` later if an incoming parameter event fires for the same parameter/value shortly after.

This hook is what makes the endpoint useful for distinguishing:

- Packet did not reach CVR at all.
- Packet reached CVR but used a wrong OSC address.
- Packet reached CVR but used an unsupported argument type.
- Packet reached CVR and was queued, but the current avatar did not have a matching parameter.
- Packet reached CVR and was applied to the current avatar.

Keep the raw-message buffer bounded using the configured MelonPreferences capacity.

### 6. Wire Into DataFeed

Update `DataFeed/Main.cs`:

- Add `_oscParameterReader` field.
- Instantiate `new OSCParameterDataReader()` in the constructor.
- Add `public IOSCParameterDataReader OSCParameterReader => _oscParameterReader;`.
- Call `_oscParameterReader.Initialize()` during `OnInitializeMelon()`.
- Call `_oscParameterReader.Dispose()` during unload/quit if the mod has an existing lifecycle hook for cleanup.
- Clear parameter state when the local avatar changes.
- Include `UpdateOSCParameterState()` in the state update path if the reader needs polling.
- Pass or expose the OSC parameter diagnostic preferences to the reader.

For the first event-driven pass, `UpdateOSCParameterState()` may simply report whether the reader has observed changes
since the last update.

### 7. REST Endpoint

Update `DataFeed/api/DataFeedController.cs`.

Chosen endpoint:

- `GET /api/v1/osc-parameters`

Return only OSC parameter diagnostic data. Keep the existing API key requirement consistent with the other DataFeed
endpoints.

### 8. WebSocket Endpoint

Update:

- `DataFeed/api/DataFeedWebSocket.cs`
- `DataFeed/api/APIServer.cs`
- `DataFeed/api/ApiConstants.cs`

Chosen WebSocket:

- `/api/v1/osc-parameters`

Behavior:

- Send initial data on open.
- Respond to message `get_osc_parameters`.
- Send periodic updates on a one second loop, matching the existing `/api/v1/osc` pattern.

### 9. Docs

Update `DataFeed/README.md` with:

- REST endpoint `/api/v1/osc-parameters`
- WebSocket endpoint `/api/v1/osc-parameters`
- Example JSON payload
- Explanation of `appliedToAvatar`
- Note that the initial implementation only sees parameters CVR applies to the current avatar
- Note that raw receive diagnostics require the optional `HandleIncoming` hook
- Preference documentation for recent-message capacity, verbose argument output, and default string truncation

## Validation

Build:

- `dotnet build DataFeed/DataFeed.csproj`
- `make build-datafeed`

Manual CVR checks:

Stage 1:

- OSC disabled in CVR settings
- OSC enabled with default inbound port
- Send a float to a valid avatar parameter
- Send an int to a valid avatar parameter
- Send a bool to a valid avatar parameter
- Send a null/no-argument trigger to a valid avatar parameter
- Change avatars and verify old parameter state is cleared
- Change the recent-message capacity preference and verify the setting is accepted
- Toggle verbose argument output while CVR is running and verify the setting is accepted

Stage 2:

- Send to a misspelled avatar parameter
- Send a valid address with an unsupported argument type
- Verify the recent-message ring buffer respects the configured capacity
- Toggle verbose argument output while CVR is running and verify string arguments switch between truncated and full output

Expected behavior:

- Applied valid parameters appear in `parameters`.
- `receivedCount` increments as repeat values arrive.
- `lastValue` updates when values change.
- `currentAvatarId` changes when the local avatar changes.
- Parameter and recent-message state is cleared when the local avatar changes.
- Bad or missing parameters appear in `recentMessages` after Stage 2 is implemented.
- Recent message strings are truncated when verbose argument output is disabled.
- Recent message strings are preserved when verbose argument output is enabled.
- The endpoint returns an API-key error when called without a valid key.

## Open Questions

No open scope questions remain for the staged implementation.

## Decisions

- Use `/api/v1/osc-parameters` for both REST and WebSocket.
- Keep `/api/v1/osc` focused on native OSC server status.
- Stage 1 uses CVR's incoming avatar parameter events for applied-parameter diagnostics.
- Stage 1 should be compiled and tested in CVR before Stage 2 starts.
- Stage 2 adds raw receive diagnostics through `OSCAvatarModule.HandleIncoming`.
- Use a bounded recent-message buffer for raw receive diagnostics in Stage 2.
- Make the recent-message ring buffer size configurable through MelonPreferences.
- Clear OSC parameter diagnostic state on avatar change.
- Truncate recent-message string argument values by default.
- Allow verbose recent-message argument output to be toggled dynamically through MelonPreferences.
- Do not expose OSC parameter diagnostics as avatar parameters.
- Do not add these fields to `/api/v1/realtime` in the first implementation.
