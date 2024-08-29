# SOURCES

## Multiplayer:
```        
Setup:
    https://youtu.be/3yuBOB3VrCk
    https://youtu.be/dUqLShvBIsM
Relay: 
    https://youtu.be/fRJlb4t_TXc
    https://stackoverflow.com/questions/51975799/how-to-get-ip-address-of-device-in-unity-2018
```
## UI:
```
https://youtu.be/tWUyEfD0kV0
https://youtu.be/RsgiYqLID-U
https://youtu.be/AyuQXfgVk3U
```
## Input:
```
https://youtu.be/VAM3Ve7ARwc
```
## Mechanics:
```
Attack:
        https://youtu.be/ktGJstDvEmU
```

# Interesting things
```
float value = (float)hp.Value / (float)maxHp.Value;
```
must be "(float)" couse hp & maxHp is INT so value would turn out 0

```
public void Copy()
{
    GUIUtility.systemCopyBuffer = connectionManager.codeText.text;
}
```
UnityEngine method to copy from variable