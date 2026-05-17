# OSCLaunchArgsFix Mod

Feature workspace for a small ChilloutVR mod that fixes native OSC launch-argument port parsing.

## Goal

Restore the documented behavior of these ChilloutVR launch arguments:

- `--osc-listener-port=<port>`
- `--osc-sender-port=<port>`

Current CVR builds detect those arguments but attempt to parse the full argument token as an integer. For example,
`--osc-listener-port=9010` is passed to `int.TryParse(...)`, so the override is rejected and CVR falls back to the
default OSC ports.

The mod should apply the parsed values before CVR starts its native OSC server, then allow CVR's existing OSC startup,
OSCQuery, logging, and port-conflict handling to continue normally.

## Files

- `implementation-plan.md` - implementation approach and task breakdown
- `reference-notes.md` - CVR symbols and behavior discovered from decompiled references
- `progress-log.md` - running notes as work is completed
