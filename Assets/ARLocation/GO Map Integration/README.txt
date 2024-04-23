Usage
=====

To test the "GO Map Integration" sample, that provides basic integration of the "AR+GPS Location" plugin
with the "GO Map" asset (https://assetstore.unity.com/packages/tools/integration/go-map-3d-map-for-ar-gaming-68889) to
the following:

- Make sure you are using Unity 2019.2 or newer.
- Import the "GO Map" asset from the Asset Store.
- Next, extract the contents of the "GO Map Integration.zip" archive into this folder.
- Add both scenes in "GO Map Integration/Scenes" to the build, with either being the first scene.
- Build the project an try it on your device!

You can use the script "GO Map Integration/Scripts/GoMapPlaceAtLocations.cs" as a reference to implement
your own custom scripts.


Components
==========

- ARLocationGoMapIntegration: Manages bridging between the ARLocation plugin and the GO Map asset, piping the
                              location provider and handling scene loading. You must insert the names of the
                              AR scene and of the GO Map scene is this component. It is a singleton which
                              will remain active during scene switching.

- ARLocationGoMapWebLoader:   Bridges the `WebMapLoader` from the "AR+GPS Location" plugin
                              with the GO Map asset.  When the map scene is loaded, it
                              will pin the `Map Pin Prefab` from the `PrefabDatabaseGoMap`
                              to the map locations. When the AR Scene is loaded, it will
                              place the `Prefab` in the geolocations.

- GoMapPlaceAtLocations:      Bridges the `PlaceAtLocations` component, in the same manner as the previous component.
