---
page_type: sample
languages:
- csharp
products:
- windows
- windows-uwp
statusNotificationTargets:
- snakechia@live.com
description: "A simple map application that based on Azure Maps features."
---

# Azure Maps for Windows 10 (Unofficial)
Azure Maps for Windows 10 is an unofficial maps app for Windows 10, which based on services from Azure Maps.

Azure Maps for Windows 10 will detect and center the map to user current location. The user can add long press or right click the mouse to place a pin of the location, a pinned locations list will shown on the left. User can set the pinned location as departure/arrival point by right click the mouse when it over the pinned location pin.

User also able to set their travel time. Option for leave now, leave at future time and arrive by is available.

Driving, Bus and walking travel options are available too.

> Note - This sample is targeted and tested for Windows 10, version 1903 (10.0; Build 18362), and Visual Studio 2019. If you prefer, you can use project properties to retarget the project(s) to Windows 10, version 1809 (10.0; Build 17763), and/or open the sample with Visual Studio 2017.

For a description of the goals and challenges of this project, see the [Project Overview](ProjectOverview.md).

## Features
Azure Maps for Windows 10 highlight the following features:
- AzureMapsLite SDK for UWP
- Show Road/Satellite Maps tiles from Azure Maps
- Show Traffic/Traffic Incidents tiles from Azure Maps
- Reverse GeoCoding (Convert latitude and longitude to address)
- Search places, road
- Get directions to location that user selected
- Set your departure or intented arrival times
- Fluent design

## Screenshots
|![Azure Maps for Windows10 screenshot](/Images/azmaps-001.png) | ![Azure Maps for Windows10 screenshot](/Images/azmaps-002.png) |
|---|---|
|![Azure Maps for Windows10 screenshot](/Images/azmaps-003.png) | ![Azure Maps for Windows10 screenshot](/Images/azmaps-004.png) |
|![Azure Maps for Windows10 screenshot](/Images/azmaps-005.png) | ![Azure Maps for Windows10 screenshot](/Images/azmaps-006.png) |

## Universal Windows Platform development

### Prerequisites

- Windows 10. Minimum: Windows 10, version 1809 (10.0; Build 17763), also known as the Windows 10 October 2018 Update.
- [Windows 10 SDK](https://developer.microsoft.com/windows/downloads/windows-10-sdk). Minimum: Windows SDK version 10.0.17763.0 (Windows 10, version 1809).
- [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) (or Visual Studio 2017). You can use the free Visual Studio Community Edition to build and run Windows Universal Platform (UWP) apps.


## Running the sample
Azure Maps for Windows 10 needs a Bing Maps and Azure Maps subcription keys to unlease its potentials.

To obtain your personal Bing Map key, please visit to https://www.bingmapsportal.com/

To obtain your personalized Azure Maps subscription key, please visit to https://portal.azure.com/

For Azure Maps for Windows 10 to able to show Satellite Image tile, you MUST subscribe to S1 instead of S0.

Once you have obtained both keys, open the Mainpage.cs file and search for line 29 and 32 and replace the placeholder for the respective keys. 


## Developer Documentation
- [Azure Maps REST APIs Documentation](https://docs.microsoft.com/en-us/rest/api/maps/)
- [Universal Windows Platform documentation](https://docs.microsoft.com/en-us/windows/uwp/)

## Contributing

You are always welcome to help developing and testing this app. Feel free to [report issues and feature request](https://github.com/snakechia/Azure-Maps-Windows10/issues) here.
