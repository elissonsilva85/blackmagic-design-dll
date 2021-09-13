# Introduction to ATEM Switcher SDK

## Samples

This package consists of the "Simple Switcher" sample from the Introduction to ATEM Switcher SDK presentation.  The sample is written in C# and targets Windows only

## Prerequisites

* Visual Studio 2017 (Community Edition is fine), with .NET desktop development installed
* CMake 3.12.1 or later (download installer from cmake.org)

### ATEM Switchers Software

* Download ATEM Switchers 8.1 Update from https://www.blackmagicdesign.com/developer/product/atem.  Follow steps to install software.
* Run ATEM Setup to ensure that your ATEM hardware is up to date.
* The Simple Switcher sample has been verified against 8.1.

### ATEM Switchers SDK

* Download ATEM Switchers 8.1 SDK from https://www.blackmagicdesign.com/developer/product/atem and extract archive under project directory
* If ATEM Switchers SDK is extracted outside of the project directory, update the -DSWITCHERS_SDK_INCLUDE_DIR in the cmake command below.

### ATEM Hardware

* Any ATEM Switcher e.g. ATEM Constellation 8K, ATEM 4 M/E Broadcast Studio, ATEM TV Studio HD, ATEM Mini

### Modifying sample

Prior to building the Simple Switcher sample, 2 changes will be required:
* Change the IP address for the target switcher:
```csharp
discovery.ConnectTo("192.168.10.240", out IBMDSwitcher switcher, out _BMDSwitcherConnectToFailure failureReason);
```
* The sample auto-transitions to external input 3, this number is zero-based, so it corresponds to input connector 4.  You can change the connector index in the .ElementAt query
```csharp
IBMDSwitcherInput input3 = atem.SwitcherInputs
                                .Where((i, ret) => {
                                    i.GetPortType(out _BMDSwitcherPortType type);
                                    return type == _BMDSwitcherPortType.bmdSwitcherPortTypeExternal;
                                })
                                .ElementAt(3); 
```

## Building Samples

Refer to BuildWin.bat batch file.

    mkdir build
    cd build
    cmake -G "Visual Studio 15 2017" -A x64 -DSWITCHERS_SDK_INCLUDE_DIR="%cd%\..\Blackmagic ATEM Switchers SDK 8.1\Windows\include" ..
    cmake --build . --config Release
