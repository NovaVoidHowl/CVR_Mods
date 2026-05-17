# Reference Notes

Sources inspected:

- `_local_files/CVR-Decompile/Assembly-CSharp/ABI_RC/Core/Savior/CheckVR.cs`
- `_local_files/CVR-Decompile/Assembly-CSharp/ABI_RC/Systems/OSC/OSCServer.cs`
- ChilloutVR launch argument docs: <https://docs.chilloutvr.net/chilloutvr/game/launch-arguments/>
- `References.Items.props`

## Documented Arguments

The official launch-argument docs list:

- `--osc-listener-port=<port>`
- `--osc-sender-port=<port>`
- `--osc-sender-ip=<ip or hostname>`
- `--osc-query-prefix=<prefix>`

Examples in the docs use the same equals-sign syntax:

- `--osc-listener-port=9000`
- `--osc-sender-port=9001`

## CVR Parser Behavior

`CheckVR.Awake()` declares:

- `public const string OscListenerPortCommandPrefix = "--osc-listener-port="`
- `public int oscListenerPort = -1`
- `public const string OscSenderPortCommandPrefix = "--osc-sender-port="`
- `public int oscSenderPort = -1`

The current decompiled parser detects the arguments but parses the entire token:

```csharp
if (text.Contains("--osc-listener-port="))
{
    if (int.TryParse(text, out var result2))
```

and:

```csharp
if (text.Contains("--osc-sender-port="))
{
    if (int.TryParse(text, out var result3))
```

That means `--osc-listener-port=9010` and `--osc-sender-port=9011` will always fail `int.TryParse`.

Nearby `--osc-sender-ip=` parsing does use `Substring("--osc-sender-ip=".Length)`, so the port handling looks like a
regression or copy/paste mistake rather than an intentional API change.

## OSC Startup Behavior

`OSCServer.StartServer()` consumes the `CheckVR` fields:

```csharp
int listenerPort = CheckVR.Instance.oscListenerPort == -1 ? 9000 : CheckVR.Instance.oscListenerPort;
IPAddress senderIp = CheckVR.Instance.OscSenderIp == null ? IPAddress.Loopback : CheckVR.Instance.OscSenderIp;
int senderPort = CheckVR.Instance.oscSenderPort == -1 ? 9001 : CheckVR.Instance.oscSenderPort;
```

It then:

- Creates the listener endpoint with `IPAddress.Any` and the chosen listener port.
- Creates the sender endpoint with the chosen sender IP and sender port.
- Creates `OscQueryServer` with the chosen listener port and loopback address.
- Catches `SocketError.AddressAlreadyInUse` if the listener port is already bound.

## Patch Point

Patch `ABI_RC.Systems.OSC.OSCServer.StartServer()` with a Harmony prefix.

Reasons:

- `CheckVR.Awake()` may have already run before Melon mods patch anything.
- `OSCServer.StartServer()` reads the parsed fields immediately before native OSC startup.
- Updating `CheckVR.Instance.oscListenerPort` and `oscSenderPort` in a prefix preserves CVR's existing startup flow.

## Dependencies

The mod should be able to use existing repo references:

- `Assembly-CSharp`
- `MelonLoader`
- `0Harmony`

No reference to `LucHeart.CoreOSC` or `OSCQuery` should be required for the first implementation pass.

## Risk Notes

- CVR may fix the bug in a future update. The mod should be harmless when CVR already parsed the values correctly.
- Direct references to `CheckVR` and `OSCServer` can break if CVR renames or moves those symbols.
- Prefix should validate port range before writing fields.
- Prefix should avoid repeatedly noisy logs if `StartServer()` can be called more than once.
- If the user supplies invalid values, leave CVR defaults untouched and log a concise warning.
