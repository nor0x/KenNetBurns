using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenNetBurns;
public class Transition
{
	private SKRect _srcRect;
	private SKRect _dstRect;
	private SKRect _currentRect = new SKRect();
	private float _widthDiff;
	private float _heightDiff;
	private float _centerXDiff;
	private float _centerYDiff;
	private long _duration;
	private Func<float, float> _interpolator;

	public static readonly int DefaultTransitionDuration = 10000;
	public static readonly float MinRectFactor = 0.75f;

	public Transition(SKRect srcRect, SKRect dstRect, long duration, Func<float, float> interpolator)
	{
		Debug.WriteLine("Transition: " + srcRect + " " + dstRect + " " + duration + " " + interpolator.Method.Name);
		if (!Utils.HaveSameAspectRatio(srcRect, dstRect))
		{ 
			throw new InvalidOperationException("Source and destination rectangles do not have the same aspect ratio.");
		}

		_srcRect = srcRect;
		_dstRect = dstRect;
		_duration = duration;
		_interpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));

		_widthDiff = dstRect.Width - srcRect.Width;
		_heightDiff = dstRect.Height - srcRect.Height;
		_centerXDiff = dstRect.MidX - srcRect.MidX;
		_centerYDiff = dstRect.MidY - srcRect.MidY;
	}

	public SKRect SourceRect => _srcRect;

	public SKRect DestinyRect => _dstRect;

	public SKRect GetInterpolatedRect(long elapsedTime)
	{
		float elapsedTimeFraction = elapsedTime / (float)_duration;
		float interpolationProgress = Math.Min(elapsedTimeFraction, 1);
		float interpolation = _interpolator(interpolationProgress);

		float currentWidth = _srcRect.Width + (interpolation * _widthDiff);
		float currentHeight = _srcRect.Height + (interpolation * _heightDiff);

		float currentCenterX = _srcRect.MidX + (interpolation * _centerXDiff);
		float currentCenterY = _srcRect.MidY + (interpolation * _centerYDiff);

		float left = currentCenterX - (currentWidth / 2);
		float top = currentCenterY - (currentHeight / 2);
		float right = left + currentWidth;
		float bottom = top + currentHeight;

		_currentRect = new SKRect(left, top, right, bottom);

		return _currentRect;
	}

	public long Duration => _duration;
}