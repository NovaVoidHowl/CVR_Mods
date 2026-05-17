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

## Current Status

Planning complete. No code has been implemented yet.

## Next Step

Create the `OSCLaunchArgsFix` MelonLoader project and implement the `OSCServer.StartServer()` Harmony prefix.
