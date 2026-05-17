# Progress Log

## 2026-05-17

- Created feature planning folder: `.development/OSCLaunchArgsFix/inital`.
- Reviewed CVR decompiled `CheckVR.Awake()` launch-argument parsing.
- Confirmed `--osc-listener-port=<port>` and `--osc-sender-port=<port>` are documented but currently parsed incorrectly.
- Reviewed `OSCServer.StartServer()` and confirmed it reads `CheckVR.Instance.oscListenerPort` and `oscSenderPort` before
  creating the native OSC listener/sender endpoints.
- Decided the lowest-risk patch point is a Harmony prefix on `OSCServer.StartServer()`.
- Captured implementation plan, validation checklist, reference notes, and risk notes.
- Resolved open questions:
  - Include the mod in default all-mods/build-all flows.
  - Do not add a config preference; the mod is single purpose.
  - Log when CVR appears to have already loaded valid OSC launch-argument ports, so users can report when the mod may
    no longer be needed.
- Implemented the `OSCLaunchArgsFix` MelonLoader project.
- Added a Harmony prefix on `OSCServer.StartServer()` that applies valid OSC listener/sender launch-argument ports
  before CVR reads `CheckVR.Instance.oscListenerPort` and `oscSenderPort`.
- Added defensive logging for missing args, invalid values, unavailable `CheckVR.Instance`, applied overrides, and
  future CVR builds that already load valid values natively.
- Added `OSCLaunchArgsFix` to the solution, Makefile restore/build/clean targets, `build-all`, and `all-mods`.
- Added a mod README and root README entry.
- Validated `make build-osclaunchargsfix-debug`.
- Validated `make build-osclaunchargsfix-release`.
- Validated `make build-all-release` includes `OSCLaunchArgsFix`.
- Confirmed Debug emits `OSCLaunchArgsFix.pdb` and Release leaves only `OSCLaunchArgsFix.dll`.

## Current Status

Implementation complete. Runtime validation in ChilloutVR is still pending.

## Next Step

Start ChilloutVR with `--osc-listener-port=9010 --osc-sender-port=9011`, enable native OSC, and confirm the player log
shows the applied overrides before CVR starts OSC.
