# DataFeed OSC Info Feature

Feature workspace for adding native ChilloutVR OSC status output to DataFeed.

## Goal

Expose the current state of CVR's native OSC implementation through DataFeed, including:

- Whether OSC is enabled in CVR settings
- Whether the native OSC server is currently running
- Inbound/listener bind address and port
- Outbound/sender address and port
- OSC verbose logging state
- OSCQuery service name when available
- Connected OSCQuery clients count and lightweight client summaries when available
- A DataFeed error flag for OSC reader failures

Avatar parameter output should include only:

- `oscEnabled`
- `oscRunning`
- `dataFeedErrorOSC`

Client summaries only represent OSCQuery-discovered clients.
Plain OSC-only clients may not appear in the client list.

## Files

- `implementation-plan.md` - implementation approach and task breakdown
- `reference-notes.md` - CVR symbols and behavior discovered from decompiled references
- `progress-log.md` - running notes as work is completed
