using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenNetBurns;

public class RandomTransitionGenerator : ITransitionGenerator
{

	private long _transitionDuration;
	private Func<float, float> _transitionInterpolator;
	private Transition _lastGenTrans;
	private SKRect _lastDrawableBounds;

	public RandomTransitionGenerator()
		: this(Transition.DefaultTransitionDuration, Interpolators.RandomInterpolation)
	{
	}

	public RandomTransitionGenerator(long transitionDuration, Func<float, float> transitionInterpolator)
	{
		Debug.WriteLine("RandomTransitionGenerator: " + transitionDuration + " " + transitionInterpolator.Method.Name);
		SetTransitionDuration(transitionDuration);
		SetTransitionInterpolator(transitionInterpolator);
	}

	public Transition GenerateNextTransition(SKRect drawableBounds, SKRect viewport)
	{
		bool firstTransition = _lastGenTrans == null;
		bool drawableBoundsChanged = !drawableBounds.Equals(_lastDrawableBounds);
		bool viewportRatioChanged = true;

		SKRect srcRect;
		SKRect dstRect = _lastGenTrans?.DestinyRect ?? SKRect.Empty;

		if (!firstTransition)
		{
			drawableBoundsChanged = !drawableBounds.Equals(_lastDrawableBounds);
			viewportRatioChanged = !Utils.HaveSameAspectRatio(dstRect, viewport);
		}

		if (dstRect == SKRect.Empty || drawableBoundsChanged || viewportRatioChanged)
		{
			srcRect = GenerateRandomRect(drawableBounds, viewport);
		}
		else
		{
			srcRect = dstRect;
		}

		dstRect = GenerateRandomRect(drawableBounds, viewport);

		_lastGenTrans = new Transition(srcRect, dstRect, _transitionDuration, _transitionInterpolator);
		_lastDrawableBounds = drawableBounds;

		return _lastGenTrans;
	}

	private SKRect GenerateRandomRect(SKRect drawableBounds, SKRect viewportRect)
	{
		float drawableRatio = Utils.GetRectRatio(drawableBounds);
		float viewportRectRatio = Utils.GetRectRatio(viewportRect);
		SKRect maxCrop;

		if (drawableRatio > viewportRectRatio)
		{
			float r = (drawableBounds.Height / viewportRect.Height) * viewportRect.Width;
			maxCrop = new SKRect(0, 0, r, drawableBounds.Height);
		}
		else
		{
			float b = (drawableBounds.Width / viewportRect.Width) * viewportRect.Height;
			maxCrop = new SKRect(0, 0, drawableBounds.Width, b);
		}

		float randomFactor = (float)Random.Shared.NextDouble();
		float factor = Transition.MinRectFactor + ((1 - Transition.MinRectFactor) * randomFactor);

		float width = factor * maxCrop.Width;
		float height = factor * maxCrop.Height;
		float widthDiff = drawableBounds.Width - width;
		float heightDiff = drawableBounds.Height - height;
		float left = widthDiff > 0 ? (float)Random.Shared.NextDouble() * widthDiff : 0;
		float top = heightDiff > 0 ? (float)Random.Shared.NextDouble() * heightDiff : 0;

		return new SKRect(left, top, left + width, top + height);
	}

	public void SetTransitionDuration(long transitionDuration)
	{
		_transitionDuration = transitionDuration;
	}

	public void SetTransitionInterpolator(Func<float, float> interpolator)
	{
		_transitionInterpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
	}

	public bool IsCroppingImage() => true;
}