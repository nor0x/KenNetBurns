#define CPU
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

namespace KenNetBurns;

public class KBKeyframe
{
	public float Scale { get; set; }
	public SKPoint Position { get; set; }
	public float Time { get; set; } // 0.0 to 1.0
}

public enum AnimationMode
{
	Loop,
	ReverseAndLoop,
	PlayOnce,
	PlayOnceAndStop
}


#if CPU
public class KBView : SKCanvasView
#else
public class KBView : SKGLView
#endif
{
	private static readonly TimeSpan FrameDelay = TimeSpan.FromMilliseconds(1000 / 60);
	private SKMatrix _matrix = SKMatrix.Identity;
	private bool _paused = false;
	private SKBitmap _currentImage;
	private readonly Stopwatch _stopwatch = new();
	private float _animationDuration = 5000f;
	private List<KBKeyframe> _keyframes = new();
	private bool _isReversing = false;
	public AnimationMode Mode { get; set; } = AnimationMode.ReverseAndLoop;


	public KBView()
	{
		EnableTouchEvents = false;
		PaintSurface += OnPaintSurface;
	}

	public void LoadImage(Stream imageStream)
	{
		_currentImage = SKBitmap.Decode(imageStream);
		InvalidateSurface();
	}

	public void SetKeyframes(List<KBKeyframe> keyframes)
	{
		if (keyframes == null || keyframes.Count == 0)
		{
			// Default keyframes if none provided
			_keyframes = new List<KBKeyframe>
		{
			new KBKeyframe { Scale = 1.0f, Position = new SKPoint(0, 0), Time = 0 },
			new KBKeyframe { Scale = 1.5f, Position = new SKPoint(0.1f, 0.1f), Time = 1 }
		};
		}
		else
		{
			// Validate and clamp keyframe values
			var validatedKeyframes = keyframes.Select(k => new KBKeyframe
			{
				// Ensure scale is at least 1.0 to cover the view
				Scale = Math.Max(1.0f, k.Scale),

				// Clamp position values based on scale to prevent image from going out of bounds
				Position = new SKPoint(
					ClampPosition(k.Position.X, k.Scale),
					ClampPosition(k.Position.Y, k.Scale)
				),
				Time = Math.Clamp(k.Time, 0.0f, 1.0f)
			}).OrderBy(k => k.Time).ToList();

			_keyframes = validatedKeyframes;
		}
		InvalidateSurface();
	}

	private float ClampPosition(float position, float scale)
	{
		// Calculate maximum allowed position based on scale
		// As scale increases, we can move the image further without showing empty space
		float maxOffset = (scale - 1.0f) / 2.0f;

		// Clamp position between -maxOffset and maxOffset
		return Math.Clamp(position, -maxOffset, maxOffset);
	}

	private (float scale, SKPoint position) InterpolateKeyframes(float progress)
	{
		if (_keyframes.Count == 0) return (1.0f, new SKPoint(0, 0));
		if (_keyframes.Count == 1) return (_keyframes[0].Scale, _keyframes[0].Position);

		// Find the keyframes to interpolate between
		var nextFrame = _keyframes.FirstOrDefault(k => k.Time >= progress);
		if (nextFrame == null) return (_keyframes.Last().Scale, _keyframes.Last().Position);

		var prevFrame = _keyframes.Where(k => k.Time <= progress).LastOrDefault() ?? _keyframes.First();

		// Calculate interpolation factor
		float frameDuration = nextFrame.Time - prevFrame.Time;
		float frameProgress = frameDuration > 0 ? (progress - prevFrame.Time) / frameDuration : 0;

		// Interpolate values
		float scale = prevFrame.Scale + (nextFrame.Scale - prevFrame.Scale) * frameProgress;
		float x = prevFrame.Position.X + (nextFrame.Position.X - prevFrame.Position.X) * frameProgress;
		float y = prevFrame.Position.Y + (nextFrame.Position.Y - prevFrame.Position.Y) * frameProgress;

		return (scale, new SKPoint(x, y));
	}

	public void StartAnimation()
	{
		_stopwatch.Start();
		Dispatcher.StartTimer(FrameDelay, () =>
		{
			if (!_paused && _currentImage != null)
			{
				InvalidateSurface();
				return true;
			}
			return !_paused;
		});
	}

#if CPU
	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
#else

	private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
#endif
	{
		if (_currentImage == null) return;

		var canvas = e.Surface.Canvas;
#if CPU
		var info = e.Info;
#else
		var info = new SKImageInfo(e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
#endif

		float elapsed = _stopwatch.ElapsedMilliseconds % (_animationDuration * 2);
		float progress;

		switch (Mode)
		{
			case AnimationMode.ReverseAndLoop:
				_isReversing = elapsed > _animationDuration;
				progress = elapsed / _animationDuration;
				if (_isReversing)
				{
					progress = 2.0f - progress; // Convert 1.0->2.0 to 1.0->0.0
				}
				break;

			case AnimationMode.PlayOnce:
				progress = Math.Min(elapsed / _animationDuration, 1.0f);
				if (progress >= 1.0f)
				{
					_paused = true;
				}
				break;

			case AnimationMode.PlayOnceAndStop:
				progress = Math.Min(elapsed / _animationDuration, 1.0f);
				if (progress >= 1.0f)
				{
					_paused = true;
					progress = 1.0f;
				}
				break;

			case AnimationMode.Loop:
			default:
				progress = (elapsed % _animationDuration) / _animationDuration;
				break;
		}

		var (effectScale, position) = InterpolateKeyframes(progress);


		// Calculate aspect ratios and base scale as before
		float viewAspect = (float)info.Width / info.Height;
		float imageAspect = (float)_currentImage.Width / _currentImage.Height;

		float baseScale;
		if (viewAspect > imageAspect)
		{
			baseScale = (float)info.Width / _currentImage.Width;
		}
		else
		{
			baseScale = (float)info.Height / _currentImage.Height;
		}

		// Apply interpolated scale
		float scale = baseScale * effectScale;

		// Calculate centered position with interpolated offset
		float scaledWidth = _currentImage.Width * scale;
		float scaledHeight = _currentImage.Height * scale;
		float dx = (info.Width - scaledWidth) / 2 + (info.Width * position.X);
		float dy = (info.Height - scaledHeight) / 2 + (info.Height * position.Y);

		_matrix = SKMatrix.CreateScale(scale, scale);
		_matrix = _matrix.PostConcat(SKMatrix.CreateTranslation(dx, dy));

		// Draw the image
		canvas.SetMatrix(_matrix);
		var rect = new SKRect(0, 0, _currentImage.Width, _currentImage.Height);
		canvas.DrawBitmap(_currentImage, rect, new SKPaint
		{
			FilterQuality = SKFilterQuality.High,
			IsAntialias = true
		});
	}


	public void Pause()
	{
		_paused = true;
	}

	public void Resume()
	{
		_paused = false;
		InvalidateSurface();
	}

}
