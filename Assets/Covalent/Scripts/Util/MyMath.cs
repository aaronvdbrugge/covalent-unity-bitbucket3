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
		int value) // value to convert
	{
		double scale = (double) (newEnd - newStart) / (originalEnd - originalStart);
		return (int) (newStart + ((value - originalStart) * scale));
	}

	public static float ConvertRange(
		float originalStart, float originalEnd, // original range
		float newStart, float newEnd, // desired range
		float value) // value to convert
	{
		float scale = (newEnd - newStart) / (originalEnd - originalStart);
		return (newStart + ((value - originalStart) * scale));
	}
}