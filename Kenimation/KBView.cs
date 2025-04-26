#define CPU
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

namespace Kenimation;

public class KBKeyframe
{
	public float Scale { get; set; }
	public SKPoint Position { get; set; }
	public float Time { get; set; } // 0.0 to 1.0

	public override string ToString()
	{
		return $"Scale: {Scale}, Position: ({Position.X}, {Position.Y}), Time: {Time}";
	}
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
	private IDispatcherTimer _timer;
	private int _animationDuration = 5000;
	public int AnimationDuration
	{
		get => _animationDuration;
		set => _animationDuration = value;
	}
	private List<KBKeyframe> _keyframes = new();
	private bool _isReversing = false;
	public AnimationMode Mode { get; set; } = AnimationMode.ReverseAndLoop;

	public static List<KBKeyframe> DefaultKeyframes { get; } = new List<KBKeyframe>
	{
		new KBKeyframe { Scale = 3.0f, Position = new SKPoint(0, 0), Time = 0 },    // Start zoomed in at center
		new KBKeyframe { Scale = 1.0f, Position = new SKPoint(0, 0), Time = 3.0f }  // End at full view
	};

	public static List<KBKeyframe> FourCornersKeyframes { get; } = new List<KBKeyframe>
	{
		new KBKeyframe { Scale = 1.0f, Position = new SKPoint(-1, -1), Time = 0 }, // Start zoomed in at top-left corner
		new KBKeyframe { Scale = 1.5f, Position = new SKPoint(1, -1), Time = 0.25f }, // Move to top-right corner
		new KBKeyframe { Scale = 2.0f, Position = new SKPoint(1, 1), Time = 0.5f }, // Move to bottom-right corner
		new KBKeyframe { Scale = 2.5f, Position = new SKPoint(-1, 1), Time = 0.75f }, // Move to bottom-left corner
		new KBKeyframe { Scale = 3.0f, Position = new SKPoint(-1, -1), Time = 1.0f } // End at top-left corner
	};

	public static List<KBKeyframe> GetRandomKeyframes(int count)
	{
		var keyframes = new List<KBKeyframe>();
		for (int i = 0; i < count; i++)
		{
			//scale from 1 to 5
			//position from 0 to 1
			//time from 0.5 to 5 and unique
			var time = (float)Random.Shared.NextDouble() * 4 + 0.5f;

			while (keyframes.Any(k => Math.Abs(k.Time - time) < 0.1f))
			{
				time = (float)Random.Shared.NextDouble() * 4 + 0.5f;
			}

			keyframes.Add(new KBKeyframe
			{
				Scale = (float)Random.Shared.NextDouble() * 4 + 1,
				Position = new SKPoint((float)Random.Shared.NextDouble() * 2 - 1, (float)Random.Shared.NextDouble() * 2 - 1),
				Time = time
			});
		}
		return keyframes;
	}

	public static List<KBKeyframe> GetRandomSmoothKeyframes(int count)
	{
		var keyframes = new List<KBKeyframe>();
		for (int i = 0; i < count; i++)
		{
			keyframes.Add(new KBKeyframe
			{
				Scale = (float)Random.Shared.NextDouble() * 4 + 1,
				Position = new SKPoint((float)Random.Shared.NextDouble() * 2 - 1, (float)Random.Shared.NextDouble() * 2 - 1),
				Time = i * 0.5f // Increment time by 0.5 seconds for each keyframe
			});
		}
		return keyframes;
	}

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

	public void SetImage(SKBitmap image)
	{
		_currentImage?.Dispose();
		_currentImage = image;
		InvalidateSurface();
	}


	public bool SetKeyframes(List<KBKeyframe> keyframes)
	{
		if (keyframes is null || keyframes.Count == 0)
		{
			keyframes = DefaultKeyframes;
		}
		_keyframes = keyframes.OrderBy(k => k.Time).ToList();

		InvalidateSurface();
		return true;
	}

	private (float scale, SKPoint position) InterpolateKeyframes(float progress, SKImageInfo info)
	{
		if (_keyframes.Count == 0) return (1.0f, new SKPoint(0, 0));
		if (_keyframes.Count == 1) return (_keyframes[0].Scale, _keyframes[0].Position);

		float minTime = _keyframes.Min(k => k.Time);
		float maxTime = _keyframes.Max(k => k.Time);
		float normalizedProgress = minTime + (maxTime - minTime) * progress;

		var nextFrame = _keyframes.FirstOrDefault(k => k.Time >= normalizedProgress);
		if (nextFrame == null) return (_keyframes.Last().Scale, _keyframes.Last().Position);

		var prevFrame = _keyframes.Where(k => k.Time <= normalizedProgress).LastOrDefault() ?? _keyframes.First();

		float frameDuration = nextFrame.Time - prevFrame.Time;
		float frameProgress = frameDuration > 0 ? (normalizedProgress - prevFrame.Time) / frameDuration : 0;

		float scale = prevFrame.Scale + (nextFrame.Scale - prevFrame.Scale) * frameProgress;
		float x = prevFrame.Position.X + (nextFrame.Position.X - prevFrame.Position.X) * frameProgress;
		float y = prevFrame.Position.Y + (nextFrame.Position.Y - prevFrame.Position.Y) * frameProgress;

		// Clamp so the image won't go off-canvas
		float baseScale = GetBaseScale(info, _currentImage);
		float scaledWidth = _currentImage.Width * baseScale * scale;
		float scaledHeight = _currentImage.Height * baseScale * scale;

		float halfWidth = (info.Width - scaledWidth) / 2f;
		float halfHeight = (info.Height - scaledHeight) / 2f;

		// If the image would move out of view, clamp
		// (for a real bounce, track velocity and invert it here)
		float maxX = (info.Width - scaledWidth) / (float)info.Width;
		float maxY = (info.Height - scaledHeight) / (float)info.Height;

		x = Math.Clamp(x, maxX / 2f, -maxX / 2f);
		y = Math.Clamp(y, maxY / 2f, -maxY / 2f);

		return (scale, new SKPoint(x, y));
	}

	private float GetBaseScale(SKImageInfo info, SKBitmap bmp)
	{
		float viewAspect = (float)info.Width / info.Height;
		float imageAspect = (float)bmp.Width / bmp.Height;
		return viewAspect > imageAspect
			? (float)info.Width / bmp.Width
			: (float)info.Height / bmp.Height;
	}

	public void StartAnimation()
	{
		_stopwatch.Start();
		_paused = false;
		_timer = Dispatcher.CreateTimer();
		_timer.Interval = FrameDelay;
		_timer.Tick += _timer_Tick;
		_timer.Start();
	}

	private void _timer_Tick(object? sender, EventArgs e)
	{
		if (_paused || _currentImage == null)
		{
			_timer.Stop();
			return;
		}
		InvalidateSurface();
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

		canvas.Clear(SKColors.Purple);

		var (effectScale, position) = InterpolateKeyframes(progress, info);


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
			IsAntialias = true
		});

		//Debug.WriteLine($"Progress: {progress}, Scale: {scale}, Position: ({position.X}, {position.Y}), Elapsed: {_stopwatch.ElapsedMilliseconds}ms, Mode: {Mode}");
	}

	public void Dispose()
	{
		_currentImage?.Dispose();
		_stopwatch.Stop();
		_stopwatch.Reset();
		_timer?.Stop();
	}

	public void Pause()
	{
		_paused = true;
		_stopwatch.Stop();
	}

	public void Resume()
	{
		_paused = false;
		_stopwatch.Start();
	}

}
