# Reference Notes

Sources inspected:

- `_local_files/CVR-App-REF/ABI_RC.Systems.OSC/ABI_RC.Systems.OSC.OSCServer.cs`
- `_local_files/CVR-API-REF/ABI_RC.Core.Savior.CheckVR.cs`
- `_local_files/CVR-API-REF/ABI_RC.Core.Savior.CVRSettings.cs`
- `References.Items.props`

## CVR OSC Server

`OSCServer` appears to live in `Assembly-CSharp` and is referenced by this repo.

Relevant symbols:

- `public const string OSCEnabledSettingName = "ImplementationOSCServerEnabled"`
- `public const string OSCVerboseLoggingEnabledSettingName = "ImplementationOSCVerboseLoggingEnabled"`
- `public static OSCServer _instance`
- `public static bool IsRunning`
- `public bool IsServerRunning { get; private set; }`
- `public IPEndPoint _listenerEndpoint`
- `public IPEndPoint _senderEndpoint`
- `public static string _oscQueryServerServiceName`
- `public static readonly ConcurrentDictionary<string, ConnectedClientInfo> ConnectedClients`

## Runtime Endpoint Behavior

In `OSCServer.StartServer()`:

- Listener port:
  - `CheckVR.Instance.oscListenerPort == -1 ? 9000 : CheckVR.Instance.oscListenerPort`
- Sender IP:
  - `CheckVR.Instance.OscSenderIp == null ? IPAddress.Loopback : CheckVR.Instance.OscSenderIp`
- Sender port:
  - `CheckVR.Instance.oscSenderPort == -1 ? 9001 : CheckVR.Instance.oscSenderPort`
- Listener endpoint:
  - `new IPEndPoint(IPAddress.Any, listenerPort)`
- Sender endpoint:
  - `new IPEndPoint(senderIp, senderPort)`
- OSCQuery service name:
  - default prefix `ChilloutVR-GameClient`
  - can be overridden with `CheckVR.Instance.oscQueryPrefix`
  - suffix is generated hex

## Settings

`CVRSettings` declares:

- `ImplementationOSCServerEnabled`, default `false`
- `ImplementationOSCVerboseLoggingEnabled`, default `false`

`OSCServer.Start()` watches these settings:

- If safe mode is active and OSC is enabled, CVR disables OSC.
- If OSC is enabled on start, `StartServer()` is called.
- Changes to `ImplementationOSCServerEnabled` start or stop the server.
- Changes to `ImplementationOSCVerboseLoggingEnabled` update verbose logging.

## Feasibility

This should be straightforward. The desired data is exposed either through public fields/properties on `OSCServer`,
CVR settings, or `CheckVR` launch-argument fields.

No additional assemblies appear necessary for the core feature because `References.Items.props` already includes:

- `Assembly-CSharp`
- `LucHeart.CoreOSC`
- `OSCQuery`

## Risk Notes

- Direct references to CVR internal public fields can break if CVR renames them.
- `OSCServer._instance` may be null before player setup or if native OSC has not initialized.
- Endpoints are cleared on `StopServer()`, so fallback/default calculation is needed for disabled or stopped states.
- `oscEnabled` and `oscRunning` intentionally differ.
- `OSCServer.ConnectedClients` only tracks clients discovered through OSCQuery.
  Plain OSC-only tools can still send to or receive from CVR, but may not appear in the connected client list.
