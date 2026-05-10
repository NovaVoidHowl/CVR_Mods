# Implementation Plan

## Summary

Add a new DataFeed reader for CVR native OSC status and expose it through REST and WebSocket output.
The implementation should follow the existing DataFeed reader pattern used by network, comms, FPS, BBCC, and MetaPort state.

The likely low-risk approach is a typed reader against CVR's `OSCServer`,
because this repo already references `Assembly-CSharp`, `LucHeart.CoreOSC`, and `OSCQuery`.

Expose only these OSC booleans as avatar parameters:

- `oscEnabled`
- `oscRunning`
- `dataFeedErrorOSC`

All other OSC details should remain API-only.

## Proposed Output Shape

Use stable, explicit field names:

```json
{
  "oscEnabled": true,
  "oscRunning": true,
  "oscVerboseLogging": false,
  "inboundAddress": "0.0.0.0",
  "inboundPort": 9000,
  "outboundAddress": "127.0.0.1",
  "outboundPort": 9001,
  "oscQueryServiceName": "ChilloutVR-GameClient-ABC123",
  "connectedOscClients": 0,
  "oscClients": [],
  "dataFeedErrorOSC": false
}
```

Notes:

- `oscEnabled` should come from CVR settings, not from server runtime state.
- `oscRunning` should come from `OSCServer.IsRunning` or the instance `IsServerRunning`.
- Enabled-but-not-running is a meaningful state, especially if the listener port failed to bind.
- Address fields should be strings to keep JSON simple and avoid leaking .NET endpoint object shape.
- `connectedOscClients` and `oscClients` only represent OSCQuery-discovered clients.
  Plain OSC senders/receivers may not appear here because CVR does not register them through OSCQuery discovery.
- When OSC is not running, runtime endpoints may be unavailable.
  In that case, output the configured/default values where available and keep `oscRunning` false.

## Code Changes

### 1. Add Interface

Create `DataFeed/Interfaces/IOSCDataReader.cs`.

Expected members:

- `bool UpdateOSCState()`
- `bool OSCEnabled { get; }`
- `bool OSCRunning { get; }`
- `bool OSCVerboseLogging { get; }`
- `string InboundAddress { get; }`
- `int InboundPort { get; }`
- `string OutboundAddress { get; }`
- `int OutboundPort { get; }`
- `string OSCQueryServiceName { get; }`
- `int ConnectedOSCClients { get; }`
- OSCQuery client summary collection, using a small model rather than exposing CVR's full `RootNode`
- `bool DataFeedErrorOSC { get; }`

### 2. Add Reader

Create `DataFeed/Services/OSCDataReader.cs`.

Implementation outline:

- Read CVR setting values from `MetaPort.Instance.settings`.
- Read launch/default config from `CheckVR.Instance`:
  - `oscListenerPort == -1 ? 9000 : oscListenerPort`
  - `OscSenderIp == null ? IPAddress.Loopback : OscSenderIp`
  - `oscSenderPort == -1 ? 9001 : oscSenderPort`
- Read runtime state from `OSCServer`:
  - `OSCServer._instance`
  - `OSCServer.IsRunning`
  - `_listenerEndpoint`
  - `_senderEndpoint`
  - `OSCServer._oscQueryServerServiceName`
  - `OSCServer.ConnectedClients.Count`
- Build a lightweight OSCQuery client summary from `OSCServer.ConnectedClients`.
  If `OSCServer.OSCQueryServer.FoundOSCClients` is safely readable, use it to include the client HTTP endpoint too.
- If runtime endpoints exist, prefer them over calculated defaults.
- On any exception, set `DataFeedErrorOSC = true`, keep conservative fallback values, and return whether state changed.

Recommended client summary fields:

```json
{
  "serviceId": "some-client-id",
  "oscAddress": "127.0.0.1",
  "oscPort": 9002,
  "httpAddress": "127.0.0.1",
  "httpPort": 12345,
  "hasAvatarModule": true,
  "hasInputModule": false,
  "hasChatBoxModule": true
}
```

If the HTTP endpoint is not available, use `null` for `httpAddress` and `httpPort`.

### 3. Wire Into DataFeed

Update `DataFeed/Main.cs`:

- Add `_oscReader` field.
- Instantiate `new OSCDataReader()` in the constructor.
- Add `public IOSCDataReader OSCReader => _oscReader;`.
- Call `_oscReader.UpdateOSCState()` in `OnUpdate()` alongside network/comms/FPS.
- Include OSC state in `UpdateDataFeed()` so initial state and setting changes are reflected.

Consider whether OSC state should trigger `StateChanged`.
If it is included in an existing WebSocket that watches `StateChanged`, then yes.

### 4. REST Endpoint

Update `DataFeed/api/DataFeedController.cs`.

Chosen endpoint:

- `GET /api/v1/osc`

Return only OSC status fields. This keeps the feature discoverable and avoids bloating `/realtime`.

Do not add OSC fields to `/api/v1/realtime` in the first implementation pass.

### 5. WebSocket Endpoint

Update:

- `DataFeed/api/DataFeedWebSocket.cs`
- `DataFeed/api/APIServer.cs`
- `DataFeed/api/ApiConstants.cs`

Chosen WebSocket:

- `/api/v1/osc`

Behavior:

- Send initial OSC data on open.
- Respond to message `get_osc`.
- Either send on a timed loop like realtime or subscribe to `StateChanged` if `UpdateOSCState()` contributes to that event.

### 6. Docs

Update `DataFeed/README.md` with:

- REST endpoint `/api/v1/osc`
- WebSocket endpoint `/api/v1/osc`
- Example JSON payload
- Notes explaining enabled vs running
- Avatar parameter availability for `oscEnabled`, `oscRunning`, and `dataFeedErrorOSC`

### 7. Avatar Parameters

Update:

- `DataFeed/Models/AvatarParameters.cs`
- `DataFeed/Services/AvatarParameterManager.cs`
- `DataFeed/Main.cs`

Expose these additional boolean avatar parameters:

- `oscEnabled`
- `oscRunning`
- `dataFeedErrorOSC`

Default behavior when DataFeed or avatar parameter output is disabled:

- `oscEnabled = false`
- `oscRunning = false`
- `dataFeedErrorOSC = false`

Do not expose ports, addresses, OSCQuery service name, or connected OSC client count as avatar parameters.
Those values should remain available through REST and WebSocket APIs only.

## Validation

Build:

- `dotnet build DataFeed/DataFeed.csproj`

Manual CVR checks:

- OSC disabled in CVR settings
- OSC enabled with default ports
- OSC enabled with launch args:
  - `--osc-listener-port=...`
  - `--osc-sender-ip=...`
  - `--osc-sender-port=...`
  - `--osc-query-prefix=...`
- OSC enabled while listener port is already in use

Expected behavior:

- Disabled: `oscEnabled=false`, `oscRunning=false`
- Running default: inbound `0.0.0.0:9000`, outbound `127.0.0.1:9001`
- Custom launch args: custom configured values appear
- Port conflict: `oscEnabled=true`, `oscRunning=false`, `dataFeedErrorOSC=false` unless the reader itself failed

## Open Questions

No open scope questions remain for the initial implementation.

## Decisions

- OSC status gets its own REST and WebSocket endpoint at `/api/v1/osc`.
- OSC status is not added to `/api/v1/realtime` for the initial implementation.
- `oscEnabled`, `oscRunning`, and `dataFeedErrorOSC` are exposed as avatar parameters.
- OSC ports, addresses, OSCQuery service name, and connected OSC client details are API-only.
- Expose both `connectedOscClients` and a lightweight `oscClients` array.
- `connectedOscClients` and `oscClients` only cover OSCQuery-discovered clients, not plain OSC connections.
