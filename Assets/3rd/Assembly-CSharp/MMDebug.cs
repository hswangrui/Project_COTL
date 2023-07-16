using UnityEngine;

public class MMDebug
{
	public static void DrawCube(Vector3 center, Vector3 size, Color color, float duration = 0f)
	{
		DrawCube(center, size, Quaternion.identity, color, duration);
	}

	public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation, float duration = 0f)
	{
		DrawCube(center, size, rotation, Color.red, duration);
	}

	public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 0f)
	{
		size /= 2f;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		vector.x = size.x;
		vector.y = size.y;
		vector.z = size.z;
		vector2.x = 0f - size.x;
		vector2.y = size.y;
		vector2.z = size.z;
		vector3.x = size.x;
		vector3.y = size.y;
		vector3.z = 0f - size.z;
		vector4.x = 0f - size.x;
		vector4.y = size.y;
		vector4.z = 0f - size.z;
		Vector3 vector5 = default(Vector3);
		Vector3 vector6 = default(Vector3);
		Vector3 vector7 = default(Vector3);
		Vector3 vector8 = default(Vector3);
		vector5 = vector;
		vector5.y = 0f - size.y;
		vector6 = vector2;
		vector6.y = 0f - size.y;
		vector7 = vector3;
		vector7.y = 0f - size.y;
		vector8 = vector4;
		vector8.y = 0f - size.y;
		vector = rotation * vector;
		vector2 = rotation * vector2;
		vector3 = rotation * vector3;
		vector4 = rotation * vector4;
		vector5 = rotation * vector5;
		vector6 = rotation * vector6;
		vector7 = rotation * vector7;
		vector8 = rotation * vector8;
		vector = center + vector;
		vector2 = center + vector2;
		vector3 = center + vector3;
		vector4 = center + vector4;
		vector5 = center + vector5;
		vector6 = center + vector6;
		vector7 = center + vector7;
		vector8 = center + vector8;
		Debug.DrawLine(vector, vector2, color, duration);
		Debug.DrawLine(vector2, vector4, color, duration);
		Debug.DrawLine(vector3, vector4, color, duration);
		Debug.DrawLine(vector, vector3, color, duration);
		Debug.DrawLine(vector5, vector6, color, duration);
		Debug.DrawLine(vector6, vector8, color, duration);
		Debug.DrawLine(vector7, vector8, color, duration);
		Debug.DrawLine(vector5, vector7, color, duration);
		Debug.DrawLine(vector, vector5, color, duration);
		Debug.DrawLine(vector2, vector6, color, duration);
		Debug.DrawLine(vector3, vector7, color, duration);
		Debug.DrawLine(vector4, vector8, color, duration);
	}

	public static void DrawArrow(Vector3 position, Vector3 dir, Color colour, float size = 1f)
	{
		Vector3 vector = Quaternion.LookRotation(dir) * Quaternion.Euler(new Vector3(0f, -135f, 0f)) * Vector3.forward;
		Vector3 vector2 = Quaternion.LookRotation(dir) * Quaternion.Euler(new Vector3(0f, 135f, 0f)) * Vector3.forward;
		vector = position + vector * size;
		vector2 = position + vector2 * size;
		Debug.DrawLine(position, vector, colour);
		Debug.DrawLine(position, vector2, colour);
	}

	public static void DrawCross(Vector3 position, Color colour, float size = 1f, float duration = 0f)
	{
		Vector3 vector = new Vector3(size, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, size, 0f);
		Debug.DrawLine(position - vector, position + vector, colour, duration);
		Debug.DrawLine(position - vector2, position + vector2, colour, duration);
	}
}
