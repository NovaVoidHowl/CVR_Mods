# DataFeed

## About

This mod provides a Data Feed of additional parameters from ChilloutVR.

## Avatar Parameters

This mod exposes the following booleans to the avatar animator, based on the restrictions set on the current world

- flyingAllowed
- propsAllowed;
- portalsAllowed;
- nameplatesEnabled;

The following booleans are exposed so that you can react to the current state of the mod

- dataFeedErrorBBCC - true if the mod is unable to read the BetterBetterCharacterController data set
- dataFeedErrorMetaPort - true if the mod is unable to read the MetaPort data set
- dataFeedDisabled - set to true if the whole mod or the avatar parameter output settings are disabled, other wise false

NOTE: if the `dataFeedDisabled` is true you should disregard the values passed by the other booleans

## Future goals

Add OSC/websocket/RESTApi output for external apps to read from
