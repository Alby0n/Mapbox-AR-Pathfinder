The **Unity AR+GPS Location** asset brings the ability to position 3D objects in real-world geographical locations via their GPS coordinates using Unity and Augmented-Reality. It supports both Untiy's **AR Foundation** and **Vuforia**.

  

It works by mixing both GPS data and the AR tracking done by AR Foundation or Vuforia.

  

Check the full documentation for the asset [here](http://docs.unity-ar-gps-location.com/).

  

Download the Totem Capture demo [here](https://play.google.com/store/apps/details?id=com.dmbfm.PoleCaptureDemo). In the demo you can select places on the map to place the 3D totems, and activate them by going near them.

  
  

**▶︎ Main Features**

  

*   **🎉 🆕 Routes and Navigation:** AR navigation powered by the Mapbox [Directions API](https://docs.mapbox.com/help/glossary/directions-api/)!

       • Navigate in AR using routes built by the Mapbox Directions API.

       • Create custom, hand-made routes for places not mapped by mapbox.

       • 3D Signs, arrows and line-renderings to guide the user throughout the route.

       • Completely customizable experience.

       • [Full sample project](https://github.com/dmbfm/unity-ar-gps-location-mapbox-route-sample) using the Mapbox SDK to display a 2D map alongside the AR view

*   Place 3D Objects in geographical positions defined by their latitude, longitude and altitude.
*   AR Hotspots that are activated when the user is near a given location.
*   Place 3D Text markers on real-world points of interest (example using OpenStreetmaps is included.)
*   Smooth movements on device location and heading updates.
*   Move objects or place them along paths (Catmull-rom splines) on the map.
*   Augmented reality floor shadows.
*   General purpose Catmull-rom curves and splines.

  
  

**▶︎ Minimum Requirements**

  

*   For AR Foundation, a iOS device with ARKit support, or a Android device with ARCore support (see the AR Core device list [here](https://developers.google.com/ar/discover/supported-devices)).
*   For Vuforia, a device with ground plane support (see the list of devices [here](https://library.vuforia.com/articles/Solution/vuforia-fusion-supported-devices.html))
*   The device must have functioning magnetic and GPS sensors.
*   For better performance, a working gyroscope is also recommended

  
  

**▶︎ Limitations**

  

*   Altitude information is usually very imprecise so, currently, it's best to use heights relative to the device position or relative to detected ground planes.
*   Landscape mode does not work well on may Android devices, due to a problem with tilt-compensation on the magnetic sensor data.
*   There are limits due to the GPS precision. So, on good conditions precision can range from 2 to 5 meters, and on bad conditions from 10 to 20meters.
*   Does not work well on indoors.

  
  

**▶︎ Contact and support**

  

For bug reports and questions please use this [link](https://argpslocation.freshdesk.com/) or contact us via email at [support@unity-ar-gps-location.com](mailto:support@unity-ar-gps-location.com).

  

_Copyright © 2018-2022 Daniel Fortes._

