# DataFeed

## About

This mod provides a Data Feed of additional parameters from ChilloutVR, both to the avatar as parameters in the animation
controller and for other apps over REST and Websocket APIs.

> [!NOTE]
>
> The Websocket and REST APIs only listen on 127.0.0.1 (local host), if you want to use the data over the network,
> you will need to use a proxy (such as haproxy) to expose the APIs
>
> It is recommend to use ssl (https/wss) if you do this.
>

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

## Controlling and Configuring the mod

### Mod Settings Menu

|             Option              | Description                                                              |
| :-----------------------------: | :----------------------------------------------------------------------- |
|             Enable              | Turns the data feed on/off                                               |
| Avatar Parameter Output Enabled | Enables/Disables sending the booleans to the avatar controller variables |
|           API Enable            | turns on/off the websocket and REST API endpoints                        |
|          REST API Port          | The REST API endpoint port on your system                                |
|       Websocket API Port        | The websocket API endpoint port on your system                           |

### API Key

In addition to the above MellonLoader variables there is `API Key` this is used with both the REST and Websocket API endpoints.

> [!IMPORTANT]
>
> The API key will not show up in the Mod Settings Menu, this is intentional to reduce the likelihood of accidentally
> leaking keys (in screenshots etc.)
\
> [!TIP]
>
> Your API key for the endpoints will show up in the MellonLoader preferences file after you have run the game for the
> first time with the mod installed
>

for REST API you connect to [http://127.0.0.1:8080/api/state](http://127.0.0.1:8080/api/state)
for Websocket API you connect to [ws://127.0.0.1:8081/DataFeed](ws://127.0.0.1:8081/DataFeed)
to authenticate to either you use the header `X-API-Key` with the value of that header being the `API Key` value from
the MellonLoader preferences file in the `DataFeed` section

> [!TIP]
>
> You can find the MellonLoader preferences file under your ChilloutVR install folder in `/UserData/MelonPreferences.cfg`
>

## Future goals

Add OSC output for external apps to read from
