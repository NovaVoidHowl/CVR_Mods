# OSCLaunchArgsFix

Small ChilloutVR MelonLoader mod that fixes CVR's native OSC launch argument port parsing.

CVR currently documents these launch options:

```text
--osc-listener-port=9010 --osc-sender-port=9011
```

On affected CVR builds, the native parser detects the arguments but tries to parse the whole argument string as an
integer. This mod patches `OSCServer.StartServer()` and applies valid parsed ports to `CheckVR.Instance` immediately
before CVR starts its native OSC server.

Upstream bug report: <https://github.com/ChilloutVR-Team/ChilloutVR-Issues/issues/2040>

## Behavior

- Applies `--osc-listener-port=<port>` to CVR's native OSC listener port.
- Applies `--osc-sender-port=<port>` to CVR's native OSC sender port.
- Ignores invalid values and lets CVR keep its default behavior.
- Logs when CVR already appears to have loaded a valid value itself, which may mean the mod is no longer needed.

Valid ports are `1` through `65535`.

## Build

```shell
make build-osclaunchargsfix
```

Debug and Release shortcuts are also available:

```shell
make build-osclaunchargsfix-debug
make build-osclaunchargsfix-release
```
