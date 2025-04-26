using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenimation;

public static class Utils
{
	public static void SafeFireAndForget(this Task task, bool returnToCallingContext = false, Action<Exception> onException = null)
	{
		async void Awaited(Task t)
		{
			try
			{
				await t.ConfigureAwait(returnToCallingContext);
			}
			catch (Exception ex) when (onException != null)
			{
				onException(ex);
			}
		}
		Awaited(task);
	}

	public static bool HaveSameAspectRatio(SKRect rect1, SKRect rect2)
	{
		float aspectRatio1 = GetRectRatio(rect1);
		float aspectRatio2 = GetRectRatio(rect2);
		return Math.Abs(aspectRatio1 - aspectRatio2) < 0.01f; // Allow some precision tolerance
	}

	public static float GetRectRatio(SKRect rect)
	{
		return rect.Width / rect.Height;
	}
}
