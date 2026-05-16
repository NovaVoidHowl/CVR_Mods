# DataFeed OSC Parameters Feature

Feature workspace for adding OSC avatar-parameter diagnostics to DataFeed.

## Goal

Expose enough information to debug whether OSC avatar parameter data is reaching ChilloutVR and whether ChilloutVR is
actually applying it to the current avatar.

The intended user questions are:

- Is CVR's native OSC server running?
- Is CVR receiving messages at `/avatar/parameters/...`?
- Which avatar parameters has CVR applied from OSC recently?
- What was the last value, type, and receive time for each parameter?
- Did a parameter fail because the packet arrived but did not match the current avatar?

## Proposed Endpoints

- REST: `GET /api/v1/osc-parameters`
- WebSocket: `/api/v1/osc-parameters`

The existing `/api/v1/osc` endpoint should remain focused on OSC server status, ports, and OSCQuery client state.

## Scope

Initial implementation should capture applied avatar OSC parameter values using CVR's existing incoming parameter
events.

Raw receive diagnostics should be added as Stage 2 in the same development cycle, after Stage 1 has been compiled and
tested in CVR. Stage 2 should observe `OSCAvatarModule.HandleIncoming`, so DataFeed can report packets that reached CVR
but were rejected, ignored, or not applied to the avatar.

## Configuration Decisions

- The recent-message ring buffer size should be configurable through MelonPreferences.
- OSC parameter diagnostic state should be cleared on avatar change.
- Recent message argument strings should be truncated by default.
- Verbose recent message argument output should be switchable at runtime through MelonPreferences.

## Files

- `implementation-plan.md` - implementation approach and task breakdown
- `reference-notes.md` - CVR symbols and behavior discovered from decompiled references
- `progress-log.md` - running notes as work is completed
