# DataFeed

## About

This mod provides a Hart Rate info to avatar animator parameters.

## Avatar Parameters

This mod exposes the following to the avatar animator

| Type  |      Value      | Description                                                               |
| :---: | :-------------: | :------------------------------------------------------------------------ |
| bool  | HRtoCVRDisabled | Returns whether the mod's data feed is disabled or not                    |
|  int  |     onesHR      | Ones spot in the Heart Rate reading                                       |
|  int  |     tensHR      | Tens spot in the Heart Rate reading                                       |
|  int  |   hundredsHR    | Hundreds spot in the Heart Rate reading                                   |
| bool  |  isHRConnected  | Returns whether the device's connection is valid or not                   |
| bool  |   isHRActive    | Returns whether the connection is valid or not                            |
| bool  |    isHRBeat     | Estimation on when the heart is beating                                   |
| float |    HRPercent    | Range of HR between the MinHR and MaxHR config value on a scale of 0 to 1 |
|  int  |       HR        | Returns the raw HR, ranged from 0 - 255.                                  |

NOTE: if the `HRtoCVRDisabled` is true you should disregard the values passed by the other parameters
