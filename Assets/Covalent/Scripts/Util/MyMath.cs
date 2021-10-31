using System;
using UnityEngine;

/// <summary>
/// Put any custom math functions here
/// </summary>
public static class MyMath
{
	/// <summary>
	/// This is a much more practical number that PI to use for most applications.
	/// Google it if you don't believe me!  :)
	/// </summary>
	public static float TAU = Mathf.PI * 2;

	public static int ConvertRange(
		int originalStart, int originalEnd, // original range
		int newStart, int newEnd, // desired range
		int value, // value to convert
		bool notBelowZero) // does not return a negative number
	{
		double scale = (double) (newEnd - newStart) / (originalEnd - originalStart);
		int result = (int) (newStart + ((value - originalStart) * scale));
		if (result < 0 && notBelowZero)
			result = 0;
		return result;
	}

	public static float ConvertRange(
		float originalStart, float originalEnd, // original range
		float newStart, float newEnd, // desired range
		float value, // value to convert
		bool notBelowZero) // does not return a negative number
	{
		float scale = (newEnd - newStart) / (originalEnd - originalStart);
		float result = (newStart + ((value - originalStart) * scale));
		if (result < 0 && notBelowZero)
			result = 0;
		return result;
	}

	public static float Pulse(float time, float frequency)
	{
		float pi = 3.14f;
		return (float) (0.5 * (1 + Mathf.Sin(2 * pi * frequency * time)));
	}
}