# Heart Rate to ChilloutVR

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

### Notes

- if the `HRtoCVRDisabled` is true you should disregard the values passed by the other parameters
- Min and Max HR values used to calculate the output of HRPercent can be set via the mod settings

## Supported Heart Rate Clients

### Simulated

This Client outputs random heart rates to allow you to test the animator logic of your avatar without need to use any
heart rate hardware

### TextFile

This Client reads its input heart rate from a text file at the polling rate configured, note you need to have a text
file at the path defined in the mod config that just contains the rate you want to use nothing else 
(ie no new lines at the end of the file)

### Pulsoid

This Client allows you to feed heart rate data in from the Pulsoid API. Note you will need a payed subscription to
Pulsoid to get an API key\
Ref [https://docs.pulsoid.net/access-token-management/manual-token-issuing](https://docs.pulsoid.net/access-token-management/manual-token-issuing)

To use this client you need to create an API key via [https://pulsoid.net/ui/keys](https://pulsoid.net/ui/keys), and put
it in the "Pulsoid Key" section in the MelonPreferences.cfg file
(there is no option to enter this via the in game UI to reduce the chance of your key being exposed publicly,
ie if you were streaming and happened to look at that menu section)
Note you will need to have run the game once with the mod installed for the `HRtoCVR` section to be added to the
MelonPreferences file.
