using UnityEngine;

public static class RectTransformUItilities
{
	public static Vector3[] GetWorldCorners(this RectTransform rectTransform)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		return array;
	}

	public static Bounds GetWorldSpaceBounds(this RectTransform rectTransform)
	{
		Vector3[] worldCorners = rectTransform.GetWorldCorners();
		Bounds result = new Bounds(worldCorners[1], Vector3.zero);
		for (int i = 1; i < worldCorners.Length; i++)
		{
			result.Encapsulate(worldCorners[i]);
		}
		return result;
	}
}
