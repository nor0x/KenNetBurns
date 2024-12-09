using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

namespace KenNetBurns;

public class KBView : SKCanvasView
{
	private static readonly TimeSpan FrameDelay = TimeSpan.FromMilliseconds(1000 / 60);
	private SKMatrix _matrix = SKMatrix.Identity;
	private Transition _currentTransition;
	private SKRect _viewportRect;
	private SKRect _drawableRect;
	private long _elapsedTime;
	private long _lastFrameTime;
	private bool _paused;
	private bool _initialized;

	public event EventHandler<Transition> TransitionStart;
	public event EventHandler<Transition> TransitionEnd;

	public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
		nameof(ImageSource), typeof(ImageSource), typeof(KBView), null, propertyChanged: OnImageSourceChanged);

	public ImageSource ImageSource
	{
		get => (ImageSource)GetValue(ImageSourceProperty);
		set => SetValue(ImageSourceProperty, value);
	}

	public static readonly BindableProperty TransitionGeneratorProperty = BindableProperty.Create(
		nameof(TransitionGenerator), typeof(ITransitionGenerator), typeof(KBView), new RandomTransitionGenerator());

	public ITransitionGenerator TransitionGenerator
	{
		get => (ITransitionGenerator)GetValue(TransitionGeneratorProperty);
		set => SetValue(TransitionGeneratorProperty, value);
	}

	public KBView()
	{
		_initialized = true;
		PaintSurface += OnPaintSurface;
	}

	private static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var view = (KBView)bindable;
		view.HandleImageChanged();
	}

	private void HandleImageChanged()
	{
		LoadImage(ImageSource).ContinueWith((task) =>
		{
			if (task.Result is not null)
			{
				_currentBitmap = task.Result;

				_drawableRect = new SKRect(0, 0, _currentBitmap.Width, _currentBitmap.Height);
				if(Width > 0 && Height > 0)
				{
					_viewportRect = new SKRect(0, 0, (float)Width, (float)Height);
					StartNewTransition();
					InvalidateSurface();
				}
			}
		});
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		if (_initialized)
		{
			_viewportRect = new SKRect(0, 0, (float)width, (float)height);
			if (_currentBitmap is not null)
			{
				StartNewTransition();
				InvalidateSurface();
			}
		}
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		try
		{
			var displayScale = DeviceDisplay.MainDisplayInfo.Density;
			Debug.WriteLine($"Display Scale: {displayScale}");

			if (!_paused && _drawableRect != SKRect.Empty)
			{
				if (_currentTransition == null)
				{
					StartNewTransition();
				}

				if (_currentTransition?.DestinyRect != null)
				{
					var now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
					_elapsedTime += now - _lastFrameTime;
					_lastFrameTime = now;

					var currentRect = _currentTransition.GetInterpolatedRect(_elapsedTime);

					float widthScale = _drawableRect.Width / currentRect.Width;
					float heightScale = _drawableRect.Height / currentRect.Height;
					float currRectToDrwScale = Math.Min(widthScale, heightScale);

					float vpWidthScale = _viewportRect.Width / currentRect.Width;
					float vpHeightScale = _viewportRect.Height / currentRect.Height;
					float currRectToVpScale = Math.Min(vpWidthScale, vpHeightScale);

					/*
					 *float vpWidthScale = mViewportRect.width() / currentRect.width();
                    float vpHeightScale = mViewportRect.height() / currentRect.height();
                    float currRectToVpScale = Math.min(vpWidthScale, vpHeightScale); 
					 */


					float totalScale = currRectToDrwScale * currRectToVpScale;

					float translX = totalScale * (_drawableRect.MidX - currentRect.Left);
					float translY = totalScale * (_drawableRect.MidY - currentRect.Top);

					/* Performs matrix transformations to fit the content
                       of the current rect into the entire view. */
					//mMatrix.reset();
					//mMatrix.postTranslate(-mDrawableRect.width() / 2, -mDrawableRect.height() / 2);
					//mMatrix.postScale(totalScale, totalScale);
					//mMatrix.postTranslate(translX, translY);

					//totalScale *= (float)displayScale;

					e.Surface.Canvas.ResetMatrix();
					_matrix = e.Surface.Canvas.TotalMatrix;
					_matrix = _matrix.PostConcat(SKMatrix.CreateTranslation(-_drawableRect.Width / 2, -_drawableRect.Height / 2));

					/*
					 *if (!mTransGen.isCroppingImage()) {
                        mMatrix.postTranslate((mViewportRect.width() - mDrawableRect.width()) / 2,
                                (mViewportRect.height() - mDrawableRect.height()) / 2);
                    } 
					 */

					if (!TransitionGenerator.IsCroppingImage())
					{
						_matrix = _matrix.PostConcat(SKMatrix.CreateTranslation((_viewportRect.Width - _drawableRect.Width) / 2,
								(_viewportRect.Height - _drawableRect.Height) / 2));
					}

					_matrix = _matrix.PostConcat(SKMatrix.CreateScale(totalScale, totalScale));
					_matrix = _matrix.PostConcat(SKMatrix.CreateTranslation(translX, translY));



					e.Surface.Canvas.SetMatrix(_matrix);
					e.Surface.Canvas.Clear(SKColors.Orange);

					DrawImageSource(e.Surface.Canvas, ImageSource);

					if (_elapsedTime >= _currentTransition.Duration)
					{
						FireTransitionEnd(_currentTransition);
						StartNewTransition();
					}
				}
			}
		}
		catch(Exception ex)
		{
			Debug.WriteLine(ex);
		}
	}

	private void StartNewTransition()
	{
		if (_drawableRect == SKRect.Empty || _viewportRect == SKRect.Empty)
			return;

		_currentTransition = TransitionGenerator.GenerateNextTransition(_drawableRect, _viewportRect);
		_elapsedTime = 0;
		_lastFrameTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
		FireTransitionStart(_currentTransition);
	}

	SKBitmap _currentBitmap;
	private void DrawImageSource(SKCanvas canvas, ImageSource imageSource)
	{
		if(_currentBitmap is not null)
		{
			//if _currentTransition is cropping image draw cropped image centered
			//else draw image centered
			if (TransitionGenerator.IsCroppingImage())
			{
				canvas.DrawBitmap(_currentBitmap, _currentTransition.DestinyRect, _drawableRect);
			}
			else
			{
				canvas.DrawBitmap(_currentBitmap, _drawableRect, _drawableRect);
			}
		}
	}

	private async Task<SKBitmap> LoadImage(ImageSource imageSource)
	{
		SKBitmap result = null;
		if (imageSource is FileImageSource fileImageSource)
		{
			result = SKBitmap.Decode(fileImageSource.File);
		}
		else if (imageSource is StreamImageSource streamImageSource)
		{
			var stream = streamImageSource.Stream.Invoke(new CancellationToken());
			if (stream != null)
			{
				result = SKBitmap.Decode(await stream);
			}

		}
		else if (imageSource is UriImageSource uriImageSource)
		{
			var uri = uriImageSource.Uri;
			var bytes = await new HttpClient().GetByteArrayAsync(uri);
			result = SKBitmap.Decode(bytes);
		}
		return result;
	}

	private void FireTransitionStart(Transition transition)
	{
		TransitionStart?.Invoke(this, transition);
	}

	private void FireTransitionEnd(Transition transition)
	{
		TransitionEnd?.Invoke(this, transition);
	}

	public void Pause()
	{
		_paused = true;
	}

	public void Resume()
	{
		_paused = false;
		_lastFrameTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
		InvalidateSurface();
	}
	public void StartAnimation()
	{
		Dispatcher.StartTimer(FrameDelay, () =>
		{
			InvalidateSurface();
			return true;
		});
	}
}
