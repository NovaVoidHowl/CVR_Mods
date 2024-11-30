# Temperature/Humidity to ChilloutVR

## About

This mod provides temperature/humidity sensor info to avatar animator parameters.

## Avatar Parameters

This mod exposes the following main mod parameter(s) to the avatar animator

| Type  |      Value      | Description                                                               |
| :---: | :-------------: | :------------------------------------------------------------------------ |
| bool  | THtoCVRDisabled | Returns whether the mod's data feed is disabled or not                    |

Other that, there can then be any number of temperature feeds, these are configured by defining them in the config json
file, the structure for which is as follows

```json
{
  "connections":[
    {
      "baseUrl":"http://192.168.0.20/",
      "temperatureEndpoint":"",
      "temperatureEndpointEnable": true,
      "temperatureEndpointAnimatorParameter":"t1",
      "humidityEndpoint":"",
      "humidityEndpointEnable":true,
      "humidityEndpointAnimatorParameter":"h1",
      "polingRate":10
    }
  ]
}
```

## API connection

The temperatureEndpoint and humidityEndpoint are expected to return json blocks like the following. Note this was made
to work with my [temperature/humidity monitoring project](https://github.com/NovaVoidHowl/Temperature-and-Humidity-Sensor)
but should work with other http APIs provided they have a 'Value' feed.

temperatureEndpoint

```json
{
  "App":"NVH_TEMP/HUM",
  "Version":"0.5.1",
  "Description":"Temperature Endpoint",
  "Value":"19.80"
}
```

humidityEndpoint

```json
{
  "App":"NVH_TEMP/HUM",
  "Version":"0.5.1",
  "Description":"Humidity Endpoint",
  "Value":"52.30"
}
```

### Notes

- if the `THtoCVRDisabled` is true you should disregard the values passed by the other parameters
