# Kenimation 🖼️→📼

<img src="https://raw.githubusercontent.com/nor0x/Kenimation/main/Art/icon.png" width="300px" />

[![Publish NuGet](https://github.com/nor0x/Kenimation/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nor0x/Kenimation/actions/workflows/dotnet.yml)

Kenimation is a customizable SkiaSharp-based view for animating still images. It supports cross-fading, keyframed zoom/pan animations, and several playback modes. Inspired by the [Ken Burns effect](https://en.wikipedia.org/wiki/Ken_Burns_effect).

---

## ✨ Features  
- 📸 Displays a list of images with Ken Burns effects.  
- 🔄 Supports cross-fade transitions between images.  
- 🎞️ Keyframed scaling (zoom) and position (motion) with interpolation.  
- 🔁 Multiple playback modes (Loop, ReverseAndLoop, PlayOnce, PlayOnceAndStop).  

---

## 🚀 Basic Usage  

### 1️⃣ Create the view in code or XAML:  
   ```xaml
   <ContentPage ...>
       <StackLayout>
           <local:KBView x:Name="kenView" />
       </StackLayout>
   </ContentPage>
   ```

### 2️⃣ Load images:  
   ```csharp
   var images = new List<SKBitmap>
   {
       SKBitmap.Decode("image1.jpg"),
       SKBitmap.Decode("image2.jpg")
   };
   kenView.LoadImages(images);
   ```

### 3️⃣ Set keyframes (optional):  
   ```csharp
   var keyframes = new List<KBKeyframe>
   {
       new KBKeyframe { Scale = 1.0f, Position = new SKPoint(0,0), Time = 0 },
       new KBKeyframe { Scale = 1.5f, Position = new SKPoint(0.1f,0.1f), Time = 1 }
   };
   kenView.SetKeyframes(keyframes);
   ```

### 4️⃣ Configure the view (optional):  
   ```csharp
   // Duration of each image’s Ken Burns animation
   kenView.AnimationDuration = 5000; 

   // How two images transition
   kenView.TransitionDuration = 1500; 

   // Control playback mode
   kenView.Mode = AnimationMode.ReverseAndLoop;
   ```

### 5️⃣ Start animation:  
   ```csharp
   kenView.StartAnimation();
   ```

---

## 📚 API Overview  

### 🛠️ Properties  
- **TransitionDuration**: Time (milliseconds) used to cross-fade between images.  
- **AnimationDuration**: Total duration (milliseconds) for a complete Ken Burns cycle on a single image.  
- **Mode**: Defines how the animation progresses (Loop, ReverseAndLoop, PlayOnce, PlayOnceAndStop).  

### 🔧 Methods  
- **LoadImages(IEnumerable<SKBitmap> images)**  
  Sets multiple images to display in sequence.  
- **SetImage(SKBitmap image)**  
  Sets a single image and replaces the current one.  
- **SetKeyframes(List<KBKeyframe> keyframes)**  
  Configures the zoom/pan animation steps.  
- **StartAnimation()**  
  Begins or resumes the Ken Burns animation and cross-fading.  
- **Pause()**  
  Temporarily stops the Ken Burns animation timer.  
- **Resume()**  
  Continues the animation after a pause.  
- **Dispose()**  
  Cleans up internal resources (bitmaps, timers, etc.).  

---

## 🎛️ KBKeyframe  

Defines a single keyframe with:  
- **Scale**: Zoom factor (1.0 = no zoom).  
- **Position**: Pan offset in normalized coordinates (-1 to 1 recommended).  
- **Time**: When the keyframe is reached (0.0 to 1.0), relative to the overall animation.  

### 🔑 Built-In Keyframes  
- **DefaultKeyframes**  
  Starts zoomed in at scale=3.0, then moves to scale=1.0 over three seconds.  
- **FourCornersKeyframes**  
  Moves between the four corners of the image while zooming from scale=1.0 to scale=3.0.  

You can also generate random keyframes with:  
- **GetRandomKeyframes(int count)**  
- **GetRandomSmoothKeyframes(int count)**  

---

## 🖼️ Example  
```csharp
// Simple usage with defaults
var kenView = new KBView();
kenView.LoadImages(new[] { SKBitmap.Decode("image1.jpg"), SKBitmap.Decode("image2.jpg") });
kenView.StartAnimation();
```

---

## 🖥️ CPU Rendering (KBCPURENDERING)  

If you want to switch Kenimation from GPU-based rendering (`SKGLView`) to CPU-based rendering (`SKCanvasView`), simply define `KBCPURENDERING` in `KBView.cs`.  

This setting can be helpful if you encounter device-specific GPU issues or prefer software rendering for debugging or performance evaluations. By toggling this define, you can easily compare rendering fidelity and performance between the two modes.  

---

The KenView will automatically animate each image for 5 seconds, then cross-fade to the next using a 1-second transition, with a reversing playback. Adjust the animation parameters, playback mode, or keyframes as needed.  