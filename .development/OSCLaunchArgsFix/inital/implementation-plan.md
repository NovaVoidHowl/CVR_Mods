# Implementation Plan

## Summary

Create a small standalone MelonLoader mod named `OSCLaunchArgsFix`.

The mod patches CVR's native `OSCServer.StartServer()` and applies valid OSC launch-argument port overrides to
`CheckVR.Instance` immediately before CVR reads those fields. The patch should be narrow, defensive, and should not
replace any of CVR's native OSC or OSCQuery behavior.

## Desired Behavior

Given Steam launch options:

```text
--osc-listener-port=9010 --osc-sender-port=9011
```

When CVR starts native OSC, it should use:

- OSC listener port: `9010`
- OSC sender port: `9011`

The existing CVR OSC startup log should then report those ports, and OSCQuery should advertise the corrected listener
port.

Invalid values should be ignored:

- Missing value: `--osc-listener-port=`
- Non-integer: `--osc-listener-port=abc`
- Out of range: `--osc-listener-port=0` or `--osc-listener-port=70000`

## Mod Shape

Expected project folder:

- `OSCLaunchArgsFix/OSCLaunchArgsFix.csproj`
- `OSCLaunchArgsFix/Main.cs`
- `OSCLaunchArgsFix/Properties/AssemblyInfo.cs`

The mod should follow the same broad project conventions as `HRtoCVR` and `THtoCVR`:

- SDK-style `net48` project via shared `Directory.Build.props`
- References imported from `References.Items.props`
- MelonLoader assembly attributes in `Properties/AssemblyInfo.cs`
- Built through the shared `scripts/build_mod.*` and Makefile patterns once added

## Code Changes

### 1. Project File

Create `OSCLaunchArgsFix/OSCLaunchArgsFix.csproj`.

Keep it minimal:

- Reference `Microsoft.NETFramework.ReferenceAssemblies.net48`
- Reference `Microsoft.CSharp` if needed by repo convention
- Rely on shared imports for `MelonLoader`, `0Harmony`, and `Assembly-CSharp`

No ILRepack target should be needed.

### 2. Assembly Info

Create `OSCLaunchArgsFix/Properties/AssemblyInfo.cs`.

Suggested metadata:

- Melon name: `OSCLaunchArgsFix`
- Author: match existing repo author style
- Version: start at `0.1.0`
- Game: ChilloutVR

### 3. Main Mod Class

Create `OSCLaunchArgsFix/Main.cs`.

Responsibilities:

- In `OnInitializeMelon`, create a Harmony instance with a unique ID.
- Patch `OSCServer.StartServer()`.
- Log that the fix is active.
- In `OnDeinitializeMelon`, unpatch the Harmony instance if the repo's MelonLoader version supports that lifecycle.

### 4. Launch Argument Parser

Implement a small parser:

- Iterate `Environment.GetCommandLineArgs()`.
- For each argument, use `StartsWith(prefix, StringComparison.Ordinal)` rather than `Contains`.
- Extract the value with `Substring(prefix.Length)`.
- Use `int.TryParse`.
- Validate `1 <= port <= 65535`.
- Return nullable values for listener and sender ports.

Suggested helper:

```csharp
private static int? ReadPortArg(string prefix)
```

### 5. Harmony Patch

Patch target:

```csharp
ABI_RC.Systems.OSC.OSCServer.StartServer()
```

Prefix behavior:

- If `CheckVR.Instance` is null, log once and do nothing.
- Parse listener and sender launch args.
- If listener value exists, set `CheckVR.Instance.oscListenerPort`.
- If sender value exists, set `CheckVR.Instance.oscSenderPort`.
- Log applied values once.
- Let original `StartServer()` run.

Do not skip the original method.

### 6. Logging

Use clear, concise Melon logs:

- On init: `OSC launch argument fix loaded.`
- Applied listener: `Applied OSC listener port override: 9010`
- Applied sender: `Applied OSC sender port override: 9011`
- Invalid value: `Ignoring invalid --osc-listener-port value: abc`

Avoid logging every frame or every failed parse repeatedly. If `StartServer()` may be invoked multiple times, either:

- Log applied values only the first time, or
- Log only when the parsed value differs from the current `CheckVR` field.

## Build Integration

After implementation:

- Add `OSCLaunchArgsFix` to `NVH_CVR_Mods.sln`.
- Add Makefile targets matching the existing mod pattern:
  - `restore-osclaunchargsfix`
  - `build-osclaunchargsfix`
  - `build-osclaunchargsfix-debug`
  - `build-osclaunchargsfix-release`
  - `osclaunchargsfix`
  - `osclaunchargsfix-debug`
  - `osclaunchargsfix-release`
  - `clean-osclaunchargsfix`
- Include it in `restore-all`, `build-all`, `all-mods`, and `clean-all`.

Decision: `make build-all` should build every mod in the repo, including this one.

## Validation Checklist

Build validation:

- `make build-osclaunchargsfix-debug`
- `make build-osclaunchargsfix-release`
- Confirm Debug emits `OSCLaunchArgsFix.pdb`.
- Confirm Release does not leave `OSCLaunchArgsFix.pdb`.

Runtime validation:

1. Start CVR with:

   ```text
   --osc-listener-port=9010 --osc-sender-port=9011
   ```

2. Enable native OSC in CVR settings.
3. Check `Player.log` for mod logs showing both overrides were applied.
4. Check CVR's native OSC startup log reports:

   ```text
   Listener Port: 9010
   Sender Port: 9011
   ```

5. Confirm an OSC sender can send avatar parameter messages to UDP `9010`.
6. Confirm a local OSC receiver gets CVR outbound messages on UDP `9011`.
7. Confirm DataFeed `/api/v1/osc` reports the corrected inbound/outbound ports when DataFeed is installed.

Conflict validation:

- Bind UDP `9010` with a test tool before starting OSC.
- Start CVR with `--osc-listener-port=9010`.
- Confirm CVR still logs its existing port-in-use error.
- Confirm the fix mod does not hide or replace that CVR behavior.

Invalid input validation:

- `--osc-listener-port=abc`
- `--osc-sender-port=70000`
- Confirm the mod logs warnings and CVR falls back to `9000` / `9001`.

## Non-Goals

- Do not replace CVR's native OSC server.
- Do not alter OSCQuery dynamic port selection.
- Do not patch `CheckVR.Awake()` unless the `StartServer()` prefix proves insufficient.
- Do not change `--osc-sender-ip=` behavior.
- Do not add UI or preferences. This mod is single purpose; removing the DLL is the disable path.

## Decisions

- The mod should be included in `make build-all` and `make all-mods`.
- The mod should not expose a preference/config toggle.
- If CVR fixes the launch-argument parser in a future build, the mod should detect that `CheckVR` already contains a
  valid configured port and log that the native config was already valid. This gives users a clear signal that the fix
  may no longer be needed.

Suggested future-fix log:

```text
CVR already loaded a valid OSC listener port from launch args; OSCLaunchArgsFix may no longer be needed.
```
