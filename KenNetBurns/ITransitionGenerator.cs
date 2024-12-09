using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenNetBurns;
public interface ITransitionGenerator
{
	/// <summary>
	/// Generates the next transition to be played in the KBView.
	/// </summary>
	/// <param name="drawableBounds">The bounds of the drawable to be shown in the KBView.</param>
	/// <param name="viewport">The rectangle that represents the viewport where the transition will be played. This is usually the bounds of the KBView.</param>
	/// <returns>A Transition object to be played by the KBView.</returns>
	Transition GenerateNextTransition(SKRect drawableBounds, SKRect viewport);

	void SetTransitionDuration(long duration);
	void SetTransitionInterpolator(Func<float, float> interpolator);

	bool IsCroppingImage();
}