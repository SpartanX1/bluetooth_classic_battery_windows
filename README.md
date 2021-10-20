# Windows 10 Bluetooth Classic Battery Level 🔋
An attempt to get battery level from Bluetooth classic device (headsets) using Window's UWP APIs 

## Tested On
OS: `Windows 10 Build 18363`

Devices :headphones: : `Sony WH-CH510` and `FireBolt BH1200`

## Why
Windows wasn't showing battery level of the headphones even though it correctly showed up on a different machine running an updated version of windows. So the Journey began to get it via Windows Runtime APIs.

## WinRT and UWP
Support for accessing bluetooth devices is there in Windows Runtime APIs. It is exposed to C# via UWP in [Windows.Devices.Bluetooth](https://docs.microsoft.com/en-us/uwp/api/windows.devices.bluetooth) namespace. Also, as a bonus, you don't need to make an UWP App in order to use these APIs. You can very well access these WinRT APIs in any WPF/.NET App by including the DLLs

My tool is a simple .NET console app :desktop_computer:

Ref: https://blogs.windows.com/windowsdeveloper/2017/01/25/calling-windows-10-apis-desktop-application/ 

## Bluetooth LE and Classic
The program gets the battery level of a classic bluetooth device, since there is no built in function to get that via WinRT APIs. For BLE devices, you can simply use built in functions [Windows.Devices.Bluetooth.GenericAttributeProfile](https://docs.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.genericattributeprofile)

## How
>We take the classic rfcomm approach to get what we need. A bluetooth device has several rfcomm channels (music, voice) to which we can connect to. Once we establish a connection, the device broadcasts certain AT Commands to which we can listen and respond to.

AT Commands Ref: https://radekp.github.io/qtmoko/api/modememulator-controlandstatus.html

The particular command we are interested in is `IPHONEACCEV` command. Although it is an APPLE specific command, a lot of manufacturers use the same command to emit their battery level info.

Ref: https://developer.apple.com/accessories/Accessory-Design-Guidelines.pdf

## Running the App :
Build and make sure your device is paired and is `disconnected`. 

Thanks.
