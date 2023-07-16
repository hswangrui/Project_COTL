using UnityEngine;

public static class Vector2Extensions
{
	public static string ToResolutionString(this Vector2 vector2)
	{
		return string.Format("{0} x {1}", vector2.x, vector2.y);
	}

	public static Vector2 Abs(this Vector2 vector)
	{
		return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}
}
