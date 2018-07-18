# NatCam API
NatCam provides a clean, functional, and amazingly performant API for accessing and controlling device cameras.

Using NatCam is as simple as calling:
```csharp
NatCam.Play(DeviceCamera.RearCamera);
```

The preview is started and the preview texture becomes available in the `NatCam.OnStart` event. This event is usually used to display the preview texture on a surface:
```csharp
NatCam.OnStart += () => material.mainTexture = NatCam.Preview;
```

## Camera Control
NatCam features a full camera control pipeline for utilizing camera functionality such as focusing, zooming, exposure, and so on. To use this functionality, simply access the properties in the DeviceCamera class:
```csharp
DeviceCamera.RearCamera.ExposureBias = 1.3;
```

Cameras can be set as active using the `NatCam.Camera` property. When a camera is active, calls to `NatCam.Play` would cause the preview to start from that camera. When NatCam is playing, the active camera can be switched by setting `NatCam.Camera` to a different camera. This will automatically start the preview from the newly set camera (so there is no need to call `NatCam.Play`).
```csharp
// Switch cameras while the preview is playing
NatCam.Camera = DeviceCamera.FrontCamera;
```

## Capturing Photos
NatCam also allows for high-resolution photo capture from the camera. To do so, simply call the `CapturePhoto` function with an appropriate callback:
```csharp
NatCam.CapturePhoto(OnPhoto);

void OnPhoto (Texture2D photo) {
    // Do stuff...
    ...
    // Remember to release the texture when you are done with it so as to avoid memory leak
    Texture2D.Destroy(photo); 
}
```

## Accessing the Preview Data
The camera stream data can be accessed using the `CaptureFrame` API. You can use this API to either capture the current preview frame into a `Texture2D`, or to simply copy the data into a `byte[]`.

```csharp
public override void OnFrame () {
    // INCOMPLETE
}
```

## Using NatCam with OpenCV
*INCOMPLETE*

```csharp
public override void OnFrame () {
    
}
```

## Sources
If you need to make changes to the API to better suit your application, please send me an email and I'll share the native sources with you. Please note that the sources for NatCam dependencies like NatCamRenderPipeline **will not be shared** as they are closed source. But all other sources will be shared. I should also note that **we do not provide support for custom builds of NatCam**, so I will not be able to provide any support if you make changes to NatCam.

___

With the simplicity of NatCam, you have the power and speed to create interactive, responsive camera apps. Happy coding!

## Requirements
- On iOS, NatCam requires iOS 7 and up (it requires iOS 8 if you use `DeviceCamera.ExposureBias`).
- On Android, NatCam requires API level 18 and up.

## Tutorials
1. [Starting Off](https://medium.com/@olokobayusuf/natcam-tutorial-series-1-starting-off-dc3990f5dab6)
2. [Controls](https://medium.com/@olokobayusuf/natcam-tutorial-series-2-controls-d2e2d0738223)
3. [Photos](https://medium.com/@olokobayusuf/natcam-tutorial-series-3-photos-e28361b83cf8)
4. [Preview Data](https://medium.com/@olokobayusuf/natcam-tutorial-series-5-preview-data-9ac36eafd1f0)
5. [MoodCam VR]()

## Notes
- On Android, Unity automatically requests camera permissions on app start. This cannot be changed without modifying Unity Android natively.
- On iOS, camera permissions are requested the first time the camera is opened.

## Quick Tips
- Please peruse the included scripting reference in the `Docs` folder.
- To discuss or report an issue, visit Unity forums [here](http://forum.unity3d.com/threads/natcam-device-camera-api.374690/).
- Check out more NatCam examples on Github [here](https://github.com/olokobayusuf?tab=repositories).
- Contact me at [olokobayusuf@gmail.com](mailto:olokobayusuf@gmail.com).

Thank you very much!