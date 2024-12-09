using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenNetBurns;
public static class Interpolators
{
	public static float RandomInterpolation(float t)
	{
		return Interpolate(t, (InterpolationType)Random.Shared.Next(0, 12));
	}

	public static float Interpolate(float t, InterpolationType type)
	{
		// A simple interpolation function
		switch (type)
		{
			case InterpolationType.AccelerateDecelerate:
				return AccelerateDecelerate(t);
			case InterpolationType.Accelerate:
				return Accelerate(t);
			case InterpolationType.Decelerate:
				return Decelerate(t);
			case InterpolationType.Linear:
				return Linear(t);
			case InterpolationType.Bounce:
				return Bounce(t);
			case InterpolationType.Elastic:
				return Elastic(t);
			case InterpolationType.Spring:
				return Spring(t);
			case InterpolationType.SmoothStep:
				return SmoothStep(t);
			case InterpolationType.SmootherStep:
				return SmootherStep(t);
			case InterpolationType.Exponential:
				return Exponential(t);
			case InterpolationType.Circular:
				return Circular(t);
			case InterpolationType.Sine:
				return Sine(t);
			case InterpolationType.Back:
				return Back(t);
			default:
				return Linear(t);
		}
	}

	public static float AccelerateDecelerate(float t)
	{
		// A simple ease-in-ease-out function
		return (float)(Math.Cos((t + 1) * Math.PI) / 2.0) + 0.5f;
	}

	public static float Accelerate(float t)
	{
		// A simple ease-in function
		return t * t;
	}

	public static float Decelerate(float t)
	{
		// A simple ease-out function
		return 1 - (1 - t) * (1 - t);
	}

	public static float Linear(float t)
	{
		// A simple linear function
		return t;
	}

	public static float Bounce(float t)
	{
		// A simple bounce function
		return (float)Math.Abs(Math.Sin(6.0 * Math.PI * t) * Math.Pow(0.5, t * 10.0));
	}

	public static float Elastic(float t)
	{
		// A simple elastic function
		return (float)(Math.Sin(20.0 * Math.PI * t) * Math.Pow(0.5, t * 10.0));
	}

	public static float Spring(float t)
	{
		// A simple spring function
		return (float)(Math.Sin(4.5 * Math.PI * t) * Math.Pow(0.5, t * 7.0));
	}

	public static float SmoothStep(float t)
	{
		// A simple smoothstep function
		return t * t * (3 - 2 * t);
	}

	public static float SmootherStep(float t)
	{
		// A simple smootherstep function
		return t * t * t * (t * (t * 6 - 15) + 10);
	}

	public static float Exponential(float t)
	{
		// A simple exponential function
		return (float)Math.Pow(2, 10 * (t - 1));
	}

	public static float Circular(float t)
	{
		// A simple circular function
		return 1 - (float)Math.Sqrt(1 - t * t);
	}

	public static float Sine(float t)
	{
		// A simple sine function
		return (float)Math.Sin(t * Math.PI / 2);
	}

	public static float Back(float t)
	{
		// A simple back function
		return t * t * (2.70158f * t - 1.70158f);
	}

	public static float QuadraticBezier(float t, float p0, float p1, float p2)
	{
		// A simple quadratic bezier function
		return (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
	}

	public static float CubicBezier(float t, float p0, float p1, float p2, float p3)
	{
		// A simple cubic bezier function
		return (1 - t) * (1 - t) * (1 - t) * p0 + 3 * (1 - t) * (1 - t) * t * p1 + 3 * (1 - t) * t * t * p2 + t * t * t * p3;
	}

	public static float Hermite(float t, float p0, float p1, float p2, float p3)
	{
		// A simple hermite function
		return (2 * t * t * t - 3 * t * t + 1) * p0 + (t * t * t - 2 * t * t + t) * p1 + (-2 * t * t * t + 3 * t * t) * p2 + (t * t * t - t * t) * p3;
	}

	public static float CatmullRom(float t, float p0, float p1, float p2, float p3)
	{
		// A simple catmull-rom function
		return 0.5f * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t);
	}

	public static float Bezier(float t, params float[] points)
	{
		// A simple bezier function
		int n = points.Length - 1;
		float result = 0;
		for (int i = 0; i <= n; i++)
		{
			result += points[i] * Bernstein(n, i, t);
		}
		return result;
	}

	private static float Bernstein(int n, int i, float t)
	{
		// A simple bernstein function
		return Factorial(n) / (Factorial(i) * Factorial(n - i)) * (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i);
	}

	private static int Factorial(int n)
	{
		// A simple factorial function
		int result = 1;
		for (int i = 1; i <= n; i++)
		{
			result *= i;
		}
		return result;
	}

	public static float Lerp(float a, float b, float t)
	{
		// A simple lerp function
		return a + (b - a) * t;
	}
}

public enum InterpolationType
{
	AccelerateDecelerate,
	Accelerate,
	Decelerate,
	Linear,
	Bounce,
	Elastic,
	Spring,
	SmoothStep,
	SmootherStep,
	Exponential,
	Circular,
	Sine,
	Back
}
