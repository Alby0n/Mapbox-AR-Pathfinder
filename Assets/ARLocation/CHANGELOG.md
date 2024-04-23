# 3.8.0
- Added `ARLocationManager.GetGameObjectPositionForLocation` method.
- Fixed locale issue with mapbox requests.

# 3.7.1
- Added: `RenderPathLine.SetLocationPath` method.
- Fixed: Route path renderer not working

# 3.7.0
- Added `DeactivateOnLeave` property to reset the hotspot on leave.
- Fixed error with `ARPlaneManager.requestedDetectionMode` in Unity 2019.4.

# 3.6.1
- Fixed "GameObject -> AR+GPS -> Mapbox Route" game object context menu item not setting the on-screen indicator arrow sprite.
- Fixed "GameObject -> AR+GPS -> Mapbox Route" game object context menu item not setting the path route renderere "Line Material".
- Fixed "Can't calculate tangents, because mesh 'Widget' doesn't contain normals." warning.
- Fixed corrupted "jet.mp3" file.

# 3.6.0
- New Major Feature: Routes and Navigation powered by the Mapbox Directions API! Check our [documentation pages](https://docs.unity-ar-gps-location.com/routes/) for more information on this feature.
- Fixed "'UnityWebRequest.isNetworkError' is obsolete:..." warnings.
- Fixed "SceneDistance" now returns the 2D distance (that is, the distance on the xz plane).
- Fixed "Assets/ARLocation/Scripts/Utils/Misc.cs(37,13) warning CS0618 'ARPlaneManager.detectionMode' is obsolete 'Use requestedDetectionMode or currentDetectionMode instead" warnings.

# 3.5.5
- Fixed warnings and compatibility issues with Unity 2020.3 and AR Foundation 4.

# 3.5.4
- Hability to use both raw and filtered GPS data in the `Hotspot` component.

# 3.5.2
- Experimental feature "World Builder": Allows the user to place objects on locations interactivelly that will perstist between sessions.
- Experimental feature "World Voxels": Persistent GPS-based voxel sandbox experiment.
- Fixed error in `GetLocationForWorldPosition` calculations
- Fixed issue when using `PlaceAtLocation.CreatePlacedInstance`

# 3.5.1
- New feature: calculate geographical location from Unity world-position.
- Fixed bug when using `PlaceAtLocation` in prefabs.

# 3.5.0
- Fixed bug in "ARLocationDevCamera". 
- Genaral improved geo-location calculation methods, specially long-distance objects.
- Added possibility of using user-provided, custom geo-calculation methods.

# 3.4.1
- Fixed corrupted 'GO Map Integration.zip'file.

# 3.4.0
- Fixed object orientation issue when placing objects at runtime.
- Fixed possible crash in `ARLocationOrientation#Restart`.
- Added integration with "GO Map 3D" asset, with sample scene.
- Added `Show Objects After This Many Updates` option so you can control how many location updates to wait before showing the placed object.
- Added `Instances` getter to `PlaceAtLocations` so you can access created instances.
- Added `OnHotspotLeave`  event to `Hotspot`.
- Ground-plane detection on ARFoundation now listens for plane changes.


# 3.3.2
- Fixed xml-parsing issue in "Web Map Loader" component.
- Fixed issue with "GroundHeight" mode when using movement smoothing.
- Added "Speed" property getter for the "MoveAlongPath" component.

# 3.3.1
- Fixed `AR Floor` prefab not rendering correctly on 2019.2+.
- Fixed erros when running on Unity 2019.3b.

# 3.3.0
- Added `Web Map Loader` component to load data from the Web Map Editor (https://editor.unity-ar-gps-location.com). For
  details check the docs (https://docs.unity-ar-gps-location.com/map/).

# 3.2.1
- Fixed bug in `PlaceAtLocation#Location` setter.
- Fixed event listeners not properly cleaned-up on some components.
	
# 3.2.0
- Improved the Debug Mode for the `PlaceAtLocation` component.

  Now, when Debug Mode is enabled, a line is rendered from the camera to
  the object, indicating it's position, and the current distance from the
  user to the object is displayed as a TextMesh.
  
 - Added the `ARLocationManager#CurrentGroundY` variable, which returns the Y coordinate
   of the detected plane which is nearest to the user/camera. 

# 3.1.1
- Fixed `mainCamera` null reference on Vuforia `GroundHeight`

# 3.1.0
- Implemented native tilt-compensated compass on Android
- Fixed coroutines not being stopped in SmoothMove
- Fixed PlaceAtLocation#Location setter not updating sometimes

# 3.0.4
- Fixed null reference error when switching scenes
- Fixed ground relative altitude issue

# 3.0.3
- Moved `MagneticDeclination.jar` to ARLocation plugins folder

# 3.0.2
- Changed AltitudeMode on sample scene

# 3.0.1
- Fixed `3D Text` sample scene

# 3.0.0
- Added `HelpURL` linking to documentation in components
- Added `Walking Zombie` prefab
- Improved AR Floor's `FollowCameraPosition` script
- Adjusted default values of properties

# 3.0.0-beta.4
- Fixed warnings on multiple Unity versions
- Fixed positioning issue on `MoveAlongPath`
- Fixed ground height issue on `MoveAlongPath`
- Refactored `MoveAlongPath` and `PathLineRenderer`
- Added `PlaceAlongPath#AltitudeMode` property

# 3.0.0-beta.3
- Improved restart methods
- PlaceAtLocation restarts with LocationProvider
- Added `ARLocationProvider#OnProviderRestartEvent`
- Fixed `PlaceAtLocation#Location` setter to work before `Start` is called
- Fixed bug on initial placement on `PlaceAtLocation`
- Added `SmoothMove#Precision` property



# 3.0.0-beta.2
- Added `ARLocationOrientation#OnBeforeOrientationUpdated` event
- Added custom location providers via ARGPS_CUSTOM_PROVIDER define symbol
- Added `Hotspot#CurrentDistance` property
- Updated documentation

# 3.0.0-beta.1
- Added `ISessionManager` class to manage the ARSession, with implementations for Vuforia and ARFoundation.
- Added `Restart` methods to ARLocationProvider, ARLocationOrientation and ARLocationManager. They will reset
the components to their initial state. In particular, calling `ARLocationManager#Restart` will restart the location
and orientation, and update all the objects positions.
- Added `ARLocationManager#WaitForARTrackingToStart` property. When this is enabled, any location and orientation 
updates will only happen when the AR tracking has started.
- Added `ARLocationManager#RestartWhenARTrackingIsRestored`. This will restart the AR+GPS system whenever the AR 
tracking is lost and regained.
- Added `OnTrackingStarted`, `OnTrackingLost` and `OnTrackingRestarted` unity events to `ARLocationManager`
- Added `ARLocationManager#ResetARSession` to reset both the ARSession and the AR+GPS system.

# 3.0.0-alpha.3
- Added `PlaceAtLocation#Restart`
- Added debug mode to `Hotspot`
- Added `DebugMode` to `PlaceAlongPath`
- Added `DebugMode` to MoveAlongPath
- Added `DebugMode` to `PlaceAtLocations`
- Added debug mode to `PlaceAtLocation`
- Added `MoveAlongPath#Reset` method
- Added `DisallowMultipleComponent` to components
- Added ground height to MoveAlongPath
- Removed Object button on ARLocationInfo
- Small changes on RenderPathLine
- Minor refactoring on PlaceAlongPath
- Refactor state fields on `MoveAlongPath`
- Refactored Properties on MoveAlongPath
- Fixed property names on `LocationPathInspector`
- Fixed bug with LocationPathInspector
- Fixed MaxNumberOfUpdates issue in MoveAlongPath

# 3.0.0-alpha.2
- Added `Hotspot` component feature
- Major refactoring to remove warnings
- Added native Android module to calculate true north/magnetic declination
- Major improvements on PlateAtLocation and PlaceAtLocations
- Added Events to PlaceAtLocation, Hotspot, ARLocationProvider, and ARLocationOrientation
- Added Hotspot sample scene
- Added easier interface to create PlaceAt objects via code
- Changed how SmoothMove works; now all Smooth Factors go from 0 to 1

# 2.7.0
- Fixed error due to wrong constructor name on `PlaneManager` when using Vuforia

# 2.6.0

- Updated samples to work with AR Foundation 1.5

# 2.5.0

- AR Foundation 1.5/2.0 compability. Not compatible with AR Foundation 1.0 anymore
- Removed automatic session reset
- Added null check for arLocationPlaneManager

# 2.4.0

- Added automatic height/altitude setting via plane detection (`UseNearestDetectedPlaneHeight` option)
- Added a public `enabled` flag to enable/disable positioning in ARLocationPlaceAtLocation enhancement
- Added ARLocationManager#Remove(entry)
- Added enabled/disabled flag do ARLocationManager Entry
- Added `offset` option to `ARLocationMoveAlongCurve` enhancement
- Added exponential weighted moving average filtering enhancement
- Added `LocationData` scriptable object to store geo locations enhancement
- Added `MaxNumberOfMeasurements` option to `ARLocationProvider` enhancement
- Added `Pause` and `Resume` methods for `ILocationProvider` enhancement
- Added default value to location in `ARLocationPlaceAtLocation`
- Added `Distance` and `GPSHorizontalDistance` methods `ARLocationManagerEntry`
- Added `ARLocationManager#UpdatePositions`
- Added `ARLocationManager#Clear`
- Modified `ARLocationManager` to use System.Guuid as entry IDs
- Modified `ARLocationManager#Restart` to be public
- Modified `Manager#Remove` to destroy instances when `createInstance` is true
- Fixed Reloading scene issues with Singletons bug
- Removed native location modules for now
- Fixed `ARLocationPlaceAtLocation#SetLocation` bug
- Fixed `ARLocationDebugInfo` bug on entry removal
- Fixed `ARLocationManager` setting position of `ARLocationRoot` instead of entry
- Fixed `MaxNumberOfMeasurements` behaviour on `ARLocationProvider`

# 2.3.0

- Fixed wrong compass rotation pivot point 

# 2.2.0

- Fixed mock location and dev-mode camera for in-editor development
- Moved LocationProvider instantiation to `Awake`
- Changed `ARLocationManager` and `ARLocationProvider` to be singleton classes
- Added Linear spline interpolation for paths
- Fixed compass rotation in ARLocationInfo component


# 2.1.0

- Fixed issue where location authorization was not being requested on Android
- Fixed issue where location was only enabled after request the next time the application
  was executed
- Fixed issue where ARLocationPlateAtLocation#SetLocation was not updating positions

# 2.0.0

- Added native GPS module for Android
- Added native GPS module for iOS
- Added global package configuration in resources folder
- Added easy Vuforia setup by clicking a checkbox in configuration
- Added option for custom magnetic declination/offset
- Added option for custom earth radius
- Added option selecting distance functions
- Added more filtering options for ARLocationProvider
- Fixed compass tilt bug on iOS native GPS module
- Added Vuforia samples package

# 1.2.0

- Added support for using Vuforia as the AR framework

  - With this Vuforia can be used instead of AR Foundation. For that
	it is necessary to add am entry `ARGPS_USE_VUFORIA` in the 'Player
	Settings' -> 'Scripting Define Symbols' list.

  - As far as the scene structure is concerned, we don't have a 'AR
	Session Origin' from AR Foundation, anymore so the
	'ARLocationRoot' object is placed directly in the root of the
	scene in this cas.e

- Fixed NullReferenceException throw when creating/editing an empty ARLocationPath


# 1.1.0

- Fixed error when there is no debug canvas.
- Improved error handling and debug logging when searching for objects and components.

# 1.0.1

- Fixed ShaderDrawer shader not working on Unity-2018.1.0.
- Added API Reference and Guide PDF files.
- Cleaned up unused variables in some classes.

# 1.0.0

Initial Release 🎉
