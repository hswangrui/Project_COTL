using System;
using System.Collections.Generic;
using UnityEngine;

public class Utils : BaseMonoBehaviour
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}

	public enum DirectionFull
	{
		Up,
		Down,
		Left,
		Right,
		Up_Diagonal,
		Down_Diagonal
	}

	private static Matrix4x4 matrix = Matrix4x4.identity;

	public static bool gizmos = true;

	public static bool Between(float Value, float LowInclusive, float HighExclusive)
	{
		if (Value >= LowInclusive)
		{
			return Value < HighExclusive;
		}
		return false;
	}

	public static float BounceLerp(float Target, float Scale, ref float ScaleSpeed, float Elasticity = 0.3f, float Bounce = 0.8f)
	{
		ScaleSpeed += (Target - Scale) * Elasticity / Time.deltaTime;
		return Scale += (ScaleSpeed *= Bounce) * Time.deltaTime;
	}

	public static float BounceLerpUnscaledDeltaTime(float Target, float Scale, ref float ScaleSpeed, float Elasticity = 0.3f, float Bounce = 0.8f)
	{
		ScaleSpeed += (Target - Scale) * Elasticity / Time.unscaledDeltaTime;
		return Scale += (ScaleSpeed *= Bounce) * Time.unscaledDeltaTime;
	}

	private static void SetColor(Color color)
	{
		if (gizmos && Gizmos.color != color)
		{
			Gizmos.color = color;
		}
	}

	public static void DrawLine(Vector3 a, Vector3 b, Color color)
	{
		SetColor(color);
		if (gizmos)
		{
			Gizmos.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b));
		}
		else
		{
			Debug.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b), color);
		}
	}

	public static void DrawCircleXY(Vector3 center, float radius, Color color, float startAngle = 0f, float endAngle = (float)Math.PI * 2f, int steps = 40)
	{
		while (startAngle > endAngle)
		{
			startAngle -= (float)Math.PI * 2f;
		}
		Vector3 vector = new Vector3(Mathf.Cos(startAngle) * radius, 0f, Mathf.Sin(startAngle) * radius);
		for (int i = 0; i <= steps; i++)
		{
			Vector3 vector2 = new Vector3(Mathf.Cos(Mathf.Lerp(startAngle, endAngle, (float)i / (float)steps)) * radius, Mathf.Sin(Mathf.Lerp(startAngle, endAngle, (float)i / (float)steps)) * radius, 0f);
			DrawLine(center + vector, center + vector2, color);
			vector = vector2;
		}
	}

	public static void DrawCircleXZ(Vector3 center, float radius, Color color, float startAngle = 0f, float endAngle = (float)Math.PI * 2f, int steps = 40)
	{
		while (startAngle > endAngle)
		{
			startAngle -= (float)Math.PI * 2f;
		}
		Vector3 vector = new Vector3(Mathf.Cos(startAngle) * radius, 0f, Mathf.Sin(startAngle) * radius);
		for (int i = 0; i <= steps; i++)
		{
			Vector3 vector2 = new Vector3(Mathf.Cos(Mathf.Lerp(startAngle, endAngle, (float)i / (float)steps)) * radius, 0f, Mathf.Sin(Mathf.Lerp(startAngle, endAngle, (float)i / (float)steps)) * radius);
			DrawLine(center + vector, center + vector2, color);
			vector = vector2;
		}
	}

	public static float GetAngle(Vector3 fromPosition, Vector3 toPosition)
	{
		return Mathf.Repeat(Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x) * 57.29578f, 360f);
	}

	public static float GetAngleR(Vector3 fromPosition, Vector3 toPosition)
	{
		return Mathf.Repeat(Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x), (float)Math.PI * 2f);
	}

	public static Direction GetAngleDirection(float Angle)
	{
		if (Angle >= -45f && Angle < 45f)
		{
			return Direction.Right;
		}
		if (Angle >= 45f && Angle < 135f)
		{
			return Direction.Up;
		}
		if (Angle >= 135f || Angle < -135f)
		{
			return Direction.Left;
		}
		if (Angle >= -135f && Angle < -45f)
		{
			return Direction.Down;
		}
		return Direction.Right;
	}

	public static DirectionFull GetAngleDirectionFull(float Angle)
	{
		Angle = Mathf.Repeat(Angle, 360f);
		if ((Angle > 112.5f && Angle < 157.5f) || (Angle > 22.5f && Angle < 67.5f))
		{
			return DirectionFull.Up_Diagonal;
		}
		if ((Angle > 202.5f && Angle < 247.5f) || (Angle > 292.5f && Angle < 337.5f))
		{
			return DirectionFull.Down_Diagonal;
		}
		if (Angle >= 67.5f && Angle <= 112.5f)
		{
			return DirectionFull.Up;
		}
		if (Angle >= 247.5f && Angle <= 292.5f)
		{
			return DirectionFull.Down;
		}
		if (Angle >= 337.5f && Angle <= 22.5f)
		{
			return DirectionFull.Right;
		}
		if (Angle >= 157.5f && Angle <= 202.5f)
		{
			return DirectionFull.Left;
		}
		return DirectionFull.Right;
	}

	public static int GetRandomWeightedIndex(int[] weights, float multiplier = 1f)
	{
		if (weights == null || weights.Length == 0)
		{
			return -1;
		}
		int num = 0;
		for (int i = 0; i < weights.Length; i++)
		{
			if (weights[i] >= 0)
			{
				num += weights[i];
			}
		}
		float num2 = Mathf.Clamp01(UnityEngine.Random.value * multiplier);
		float num3 = 0f;
		for (int i = 0; i < weights.Length; i++)
		{
			if (!((float)weights[i] <= 0f))
			{
				num3 += (float)weights[i] / (float)num;
				if (num3 >= num2)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static float SmoothAngle(float CurrentAngle, float TargetAngle, float Easing)
	{
		return CurrentAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - CurrentAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - CurrentAngle) * ((float)Math.PI / 180f))) * 57.29578f / Easing * (Time.deltaTime * 60f);
	}

	public static Vector3 RayToPosition(float Radius, Vector3 StartPosition, float Direction, float Distance, LayerMask layerToCheck)
	{
		Direction *= (float)Math.PI / 180f;
		Vector3 vector = StartPosition + new Vector3(Distance * Mathf.Cos(Direction), Distance * Mathf.Sin(Direction));
		RaycastHit2D[] array = Physics2D.CircleCastAll(StartPosition + new Vector3(Radius * Mathf.Cos(Direction), Radius * Mathf.Sin(Direction)), Radius, Vector3.Normalize(vector - StartPosition), Distance, layerToCheck);
		if (array.Length != 0)
		{
			return array[0].centroid;
		}
		return vector;
	}

	public static bool WithinRange(float Value, float Min, float Max)
	{
		if (Value >= Min)
		{
			return Value <= Max;
		}
		return false;
	}

	public static bool WithinRange(float Value, int Min, int Max)
	{
		if (Value >= (float)Min)
		{
			return Value <= (float)Max;
		}
		return false;
	}

	public static bool PointWithinPolygon(Vector3 v, List<Vector3> p)
	{
		int index = p.Count - 1;
		bool flag = false;
		int num = 0;
		while (num < p.Count)
		{
			flag ^= ((p[num].y > v.y) ^ (p[index].y > v.y)) && v.x < (p[index].x - p[num].x) * (v.y - p[num].y) / (p[index].y - p[num].y) + p[num].x;
			index = num++;
		}
		return flag;
	}

	public static Vector2 RadianToVector2(float radian)
	{
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static Vector2 DegreeToVector2(float degree)
	{
		return RadianToVector2(degree * ((float)Math.PI / 180f));
	}

	public static float linear(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}

	public static float clerp(float start, float end, float value)
	{
		float num = 0f;
		float num2 = 360f;
		float num3 = Mathf.Abs((num2 - num) * 0.5f);
		float num4 = 0f;
		float num5 = 0f;
		if (end - start < 0f - num3)
		{
			num5 = (num2 - start + end) * value;
			return start + num5;
		}
		if (end - start > num3)
		{
			num5 = (0f - (num2 - end + start)) * value;
			return start + num5;
		}
		return start + (end - start) * value;
	}

	public static float spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * (float)Math.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	public static float easeInQuad(float start, float end, float value)
	{
		end -= start;
		return end * value * value + start;
	}

	public static float easeOutQuad(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * value * (value - 2f) + start;
	}

	public static float easeInOutQuad(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value + start;
		}
		value -= 1f;
		return (0f - end) * 0.5f * (value * (value - 2f) - 1f) + start;
	}

	public float easeInCubic(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value + start;
	}

	public static float easeOutCubic(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * (value * value * value + 1f) + start;
	}

	public static float easeInOutCubic(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value + start;
		}
		value -= 2f;
		return end * 0.5f * (value * value * value + 2f) + start;
	}

	public static float easeInQuart(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value + start;
	}

	public static float easeOutQuart(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return (0f - end) * (value * value * value * value - 1f) + start;
	}

	public float easeInOutQuart(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value * value + start;
		}
		value -= 2f;
		return (0f - end) * 0.5f * (value * value * value * value - 2f) + start;
	}

	public static float easeInQuint(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value * value + start;
	}

	public static float easeOutQuint(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * (value * value * value * value * value + 1f) + start;
	}

	public static float easeInOutQuint(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value * value * value + start;
		}
		value -= 2f;
		return end * 0.5f * (value * value * value * value * value + 2f) + start;
	}

	public static float easeInSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * Mathf.Cos(value * ((float)Math.PI / 2f)) + end + start;
	}

	public static float easeOutSine(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Sin(value * ((float)Math.PI / 2f)) + start;
	}

	public static float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * 0.5f * (Mathf.Cos((float)Math.PI * value) - 1f) + start;
	}

	public static float easeInExpo(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Pow(2f, 10f * (value - 1f)) + start;
	}

	public static float easeOutExpo(float start, float end, float value)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * value) + 1f) + start;
	}

	public static float easeInOutExpo(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * Mathf.Pow(2f, 10f * (value - 1f)) + start;
		}
		value -= 1f;
		return end * 0.5f * (0f - Mathf.Pow(2f, -10f * value) + 2f) + start;
	}

	public static float easeInCirc(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * (Mathf.Sqrt(1f - value * value) - 1f) + start;
	}

	public static float easeOutCirc(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * Mathf.Sqrt(1f - value * value) + start;
	}

	public static float easeInOutCirc(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return (0f - end) * 0.5f * (Mathf.Sqrt(1f - value * value) - 1f) + start;
		}
		value -= 2f;
		return end * 0.5f * (Mathf.Sqrt(1f - value * value) + 1f) + start;
	}

	public static float easeInBounce(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		return end - easeOutBounce(0f, end, num - value) + start;
	}

	public static float easeOutBounce(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		if (value < 0.36363637f)
		{
			return end * (7.5625f * value * value) + start;
		}
		if (value < 0.72727275f)
		{
			value -= 0.54545456f;
			return end * (7.5625f * value * value + 0.75f) + start;
		}
		if ((double)value < 0.9090909090909091)
		{
			value -= 0.8181818f;
			return end * (7.5625f * value * value + 0.9375f) + start;
		}
		value -= 21f / 22f;
		return end * (7.5625f * value * value + 63f / 64f) + start;
	}

	public static float easeInOutBounce(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		if (value < num * 0.5f)
		{
			return easeInBounce(0f, end, value * 2f) * 0.5f + start;
		}
		return easeOutBounce(0f, end, value * 2f - num) * 0.5f + end * 0.5f + start;
	}

	public static float easeInBack(float start, float end, float value)
	{
		end -= start;
		value /= 1f;
		float num = 1.70158f;
		return end * value * value * ((num + 1f) * value - num) + start;
	}

	public static float easeOutBack(float start, float end, float value)
	{
		float num = 1.70158f;
		end -= start;
		value -= 1f;
		return end * (value * value * ((num + 1f) * value + num) + 1f) + start;
	}

	public static float easeInOutBack(float start, float end, float value)
	{
		float num = 1.70158f;
		end -= start;
		value /= 0.5f;
		if (value < 1f)
		{
			num *= 1.525f;
			return end * 0.5f * (value * value * ((num + 1f) * value - num)) + start;
		}
		value -= 2f;
		num *= 1.525f;
		return end * 0.5f * (value * value * ((num + 1f) * value + num) + 2f) + start;
	}

	public static float punch(float amplitude, float value)
	{
		float num = 9f;
		if (value == 0f)
		{
			return 0f;
		}
		if (value == 1f)
		{
			return 0f;
		}
		float num2 = 0.3f;
		num = num2 / ((float)Math.PI * 2f) * Mathf.Asin(0f);
		return amplitude * Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * 1f - num) * ((float)Math.PI * 2f) / num2);
	}

	public static float easeInElastic(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		float num2 = num * 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= num) == 1f)
		{
			return start + end;
		}
		if (num4 == 0f || num4 < Mathf.Abs(end))
		{
			num4 = end;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(end / num4);
		}
		return 0f - num4 * Mathf.Pow(2f, 10f * (value -= 1f)) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2) + start;
	}

	public static float easeOutElastic(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		float num2 = num * 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= num) == 1f)
		{
			return start + end;
		}
		if (num4 == 0f || num4 < Mathf.Abs(end))
		{
			num4 = end;
			num3 = num2 * 0.25f;
		}
		else
		{
			num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(end / num4);
		}
		return num4 * Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2) + end + start;
	}

	public float easeInOutElastic(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		float num2 = num * 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= num * 0.5f) == 2f)
		{
			return start + end;
		}
		if (num4 == 0f || num4 < Mathf.Abs(end))
		{
			num4 = end;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(end / num4);
		}
		if (value < 1f)
		{
			return -0.5f * (num4 * Mathf.Pow(2f, 10f * (value -= 1f)) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2)) + start;
		}
		return num4 * Mathf.Pow(2f, -10f * (value -= 1f)) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2) * 0.5f + end + start;
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		return Quaternion.Euler(angles) * (point - pivot) + pivot;
	}
}
