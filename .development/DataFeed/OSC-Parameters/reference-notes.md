# Reference Notes

Sources inspected:

- `mods/cvr-decompile/Assembly-CSharp/ABI_RC/Systems/OSC/Modules/OSCAvatarModule.cs`
- `mods/cvr-decompile/Assembly-CSharp/ABI_RC/Systems/OSC/OSCServer.cs`
- `mods/CVR_Mods_NVH/DataFeed/Main.cs`
- `mods/CVR_Mods_NVH/DataFeed/api/DataFeedController.cs`
- `mods/CVR_Mods_NVH/DataFeed/api/DataFeedWebSocket.cs`
- `mods/CVR_Mods_NVH/DataFeed/api/APIServer.cs`
- `mods/CVR_Mods_NVH/References.Items.props`

## Existing DataFeed OSC Endpoint

DataFeed already exposes native OSC server status at:

- REST: `/api/v1/osc`
- WebSocket: `/api/v1/osc`

Current OSC status includes:

- `oscEnabled`
- `oscRunning`
- `oscVerboseLogging`
- inbound/listener endpoint
- outbound/sender endpoint
- OSCQuery service name
- OSCQuery-discovered client summaries
- `dataFeedErrorOSC`

This endpoint should remain status-focused.

## CVR Avatar OSC Receive Path

`OSCAvatarModule.HandleIncoming(OscMessage packet)` handles these incoming addresses:

- `/avatar/parameters/{name}`
- `/avatar/change`
- `/avatar/profile/change`

For `/avatar/parameters/{name}`:

- One `bool` argument queues an avatar bool parameter payload.
- One `float` argument queues an avatar float parameter payload and a face-tracking payload.
- One `int` argument queues an avatar int parameter payload.
- Zero arguments queues a null parameter payload.
- Unsupported argument types return `false`.

The method returns `true` when the message is accepted and queued by the OSC module.
That does not guarantee the current avatar has a matching parameter.

## CVR Applied Parameter Events

`OSCAvatarModule` exposes static events:

- `OnIncomingAvatarFloatOSCParameter`
- `OnIncomingAvatarIntOSCParameter`
- `OnIncomingAvatarBoolOSCParameter`
- `OnIncomingAvatarNullOSCParameter`
- `OnIncomingFaceTrackingOSCParameter`

The avatar parameter events fire after CVR checks `AvatarAnimatorManager.ParametersHash` and calls
`PlayerSetup.Instance.ChangeAnimatorParam(..., PlayerSetup.ParameterChangeSource.OSC)`.

These events are the best low-risk source for "CVR applied this OSC value to the current avatar".

## Why Raw Receive Tracking Is Different

The static incoming avatar parameter events do not fire for every received OSC packet.

They will not capture cases such as:

- Wrong OSC path
- Unsupported argument type
- Parameter name does not exist on the current avatar
- Packet handled by another OSC module
- Packet rejected before avatar parameter application

To debug those cases, DataFeed needs to observe `OSCAvatarModule.HandleIncoming` directly, preferably with a Harmony
prefix/postfix owned by DataFeed. This should be implemented as Stage 2, after the applied-parameter event path has been
compiled and tested in CVR.

## Existing References

`References.Items.props` already includes:

- `0Harmony`
- `Assembly-CSharp`
- `LucHeart.CoreOSC`
- `OSCQuery`

No new assembly reference should be needed for the planned event subscription or Harmony hook.

## Risk Notes

- CVR internal method names and public fields can change between CVR updates.
- Static event subscriptions must be removed during cleanup to avoid stale references.
- Harmony patches should use a unique ID owned by DataFeed so they can be unpatched safely.
- OSC callbacks may happen outside DataFeed's normal update flow, so shared state should be lock-protected.
- The recent-message buffer must be bounded to avoid memory growth if an OSC sender is noisy.
- Raw argument values should be serialized carefully so unusual object types do not break the API response.
- Raw string argument values should be truncated by default to keep normal endpoint output readable.
- Verbose raw argument output should be controlled by MelonPreferences and should be safe to toggle at runtime.
