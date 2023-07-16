using UnityEngine;

public static class TrailRendererExtensions
{
	public static float GetTrailLength(this TrailRenderer trailRenderer)
	{
		Vector3[] array = new Vector3[trailRenderer.positionCount];
		int positions = trailRenderer.GetPositions(array);
		if (positions < 2)
		{
			return 0f;
		}
		float num = 0f;
		Vector3 a = array[0];
		for (int i = 1; i < positions; i++)
		{
			Vector3 vector = array[i];
			num += Vector3.Distance(a, vector);
			a = vector;
		}
		return num;
	}
}
