using SkiaSharp;

namespace KenNetBurns;
public class FullToRandomTransitionGenerator : ITransitionGenerator
{
	public static readonly int DefaultTransitionDuration = 10000;
	private static readonly float MinRectFactor = 0.95f;

	private readonly Random _random = new Random((int)DateTime.Now.Ticks);
	private long _transitionDuration;
	private Func<float, float> _transitionInterpolator;
	private Transition _lastGenTrans;
	private SKRect _lastDrawableBounds;

	public FullToRandomTransitionGenerator()
		: this(DefaultTransitionDuration, Interpolators.AccelerateDecelerate)
	{
	}

	public FullToRandomTransitionGenerator(long transitionDuration, Func<float, float> transitionInterpolator)
	{
		SetTransitionDuration(transitionDuration);
		SetTransitionInterpolator(transitionInterpolator);
	}

	public bool IsCroppingImage() => false;

	public Transition GenerateNextTransition(SKRect drawableBounds, SKRect viewport)
	{
		var srcRect = GenerateFullBalancedRect(drawableBounds, viewport);
		var dstRect = GenerateRandomRect(drawableBounds, viewport);

		_lastGenTrans = new Transition(srcRect, dstRect, _transitionDuration, _transitionInterpolator);
		_lastDrawableBounds = drawableBounds;

		return _lastGenTrans;
	}

	private SKRect GenerateRandomRect(SKRect drawableBounds, SKRect viewportRect)
	{
		float widthRatio = viewportRect.Width / drawableBounds.Width;
		float heightRatio = viewportRect.Height / drawableBounds.Height;
		float smallestRatio = Math.Min(widthRatio, heightRatio);

		float randomFactor = (float)_random.NextDouble();
		float factor = MinRectFactor + ((1 - MinRectFactor) * randomFactor);

		float width = drawableBounds.Width * smallestRatio * factor;
		float height = drawableBounds.Height * smallestRatio * factor;
		float widthDiff = drawableBounds.Width - width;
		float heightDiff = drawableBounds.Height - height;
		float left = widthDiff > 0 ? (float)_random.NextDouble() * widthDiff : 0;
		float top = heightDiff > 0 ? (float)_random.NextDouble() * heightDiff : 0;

		return new SKRect(left, top, left + width, top + height);
	}

	private SKRect GenerateFullBalancedRect(SKRect drawableBounds, SKRect viewportRect)
	{
		float widthRatio = viewportRect.Width / drawableBounds.Width;
		float heightRatio = viewportRect.Height / drawableBounds.Height;
		float smallestRatio = Math.Min(widthRatio, heightRatio);

		float width = drawableBounds.Width * smallestRatio;
		float height = drawableBounds.Height * smallestRatio;

		return new SKRect(0, 0, width, height);
	}

	public void SetTransitionDuration(long transitionDuration)
	{
		_transitionDuration = transitionDuration;
	}

	public void SetTransitionInterpolator(Func<float, float> interpolator)
	{
		_transitionInterpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
	}
}