using SkiaSharp;

namespace KenNetBurns;
public class RectTransitionGenerator : ITransitionGenerator
{
	//transition from top left to top right to bottom right to bottom left to top left
	private long _transitionDuration;
	private Func<float, float> _transitionInterpolator;
	private Transition _lastGenTrans;
	private SKRect _lastDrawableBounds;


	private float _scale = 0.5f;
	int _currentStep = 0;

	public float Scale
	{
		get => _scale;
		set => _scale = value;
	}


	public RectTransitionGenerator(float scale) : this(Transition.DefaultTransitionDuration, Interpolators.Bounce)
	{
		_scale = scale;
	}

	public RectTransitionGenerator(long transitionDuration, Func<float, float> transitionInterpolator)
	{
		SetTransitionDuration(transitionDuration);
		SetTransitionInterpolator(transitionInterpolator);
	}

	public Transition GenerateNextTransition(SKRect drawableBounds, SKRect viewportRect)
	{
		bool firstTransition = _lastGenTrans == null;

		SKRect srcRect;
		SKRect dstRect = _lastGenTrans?.DestinyRect ?? SKRect.Empty;

		//_currentStep = 0 => top left
		//_currentStep = 1 => top right
		//_currentStep = 2 => bottom right
		//_currentStep = 3 => bottom left
		//_currentStep = 4 => top left

		if (_currentStep == 0)
		{
			srcRect = GenerateTopLeftRect(drawableBounds, viewportRect);
			dstRect = GenerateTopRightRect(drawableBounds, viewportRect);
			_currentStep++;
		}
		else if (_currentStep == 1)
		{
			srcRect = GenerateTopRightRect(drawableBounds, viewportRect);
			dstRect = GenerateBottomRightRect(drawableBounds, viewportRect);
			_currentStep++;
		}
		else if (_currentStep == 2)
		{
			srcRect = GenerateBottomRightRect(drawableBounds, viewportRect);
			dstRect = GenerateBottomLeftRect(drawableBounds, viewportRect);
			_currentStep++;
		}
		else if (_currentStep == 3)
		{
			srcRect = GenerateBottomLeftRect(drawableBounds, viewportRect);
			dstRect = GenerateTopLeftRect(drawableBounds, viewportRect);
			_currentStep++;
		}
		else
		{
			srcRect = GenerateTopLeftRect(drawableBounds, viewportRect);
			dstRect = GenerateTopRightRect(drawableBounds, viewportRect);
			_currentStep = 0;
		}

		_lastGenTrans = new Transition(srcRect, dstRect, _transitionDuration, _transitionInterpolator);
		return _lastGenTrans;
	}

	//top left rectangle of drawablebounds with scale
	private SKRect GenerateTopLeftRect(SKRect drawableBounds, SKRect viewportRect)
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

		float width = _scale * maxCrop.Width;
		float height = _scale * maxCrop.Height;

		return new SKRect(0, 0, width, height);
	}

	//top right rectangle of drawablebounds with scale
	private SKRect GenerateTopRightRect(SKRect drawableBounds, SKRect viewportRect)
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
		float width = _scale * maxCrop.Width;
		float height = _scale * maxCrop.Height;
		return new SKRect(drawableBounds.Width - width, 0, drawableBounds.Width, height);
	}

	//bottom right rectangle of drawablebounds with scale
	private SKRect GenerateBottomRightRect(SKRect drawableBounds, SKRect viewportRect)
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
		float width = _scale * maxCrop.Width;
		float height = _scale * maxCrop.Height;
		return new SKRect(drawableBounds.Width - width, drawableBounds.Height - height, drawableBounds.Width, drawableBounds.Height);
	}

	//bottom left rectangle of drawablebounds with scale
	private SKRect GenerateBottomLeftRect(SKRect drawableBounds, SKRect viewportRect)
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
		float width = _scale * maxCrop.Width;
		float height = _scale * maxCrop.Height;
		return new SKRect(0, drawableBounds.Height - height, width, drawableBounds.Height);
	}


	public void SetTransitionDuration(long duration)
	{
		_transitionDuration = duration;
	}

	public void SetTransitionInterpolator(Func<float, float> interpolator)
	{
		_transitionInterpolator = interpolator;
	}

	public bool IsCroppingImage() => false;
}
