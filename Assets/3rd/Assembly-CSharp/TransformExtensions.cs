using UnityEngine;

public static class TransformExtensions
{
	public static bool IsChildOf(this Transform transform, Transform possibleParent)
	{
		Transform parent = transform.parent;
		while (parent != null)
		{
			parent = parent.parent;
			if (parent == possibleParent)
			{
				return true;
			}
		}
		return false;
	}

	public static void DestroyAllChildren(this Transform transform)
	{
		int num = transform.childCount;
		while (--num >= 0)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(transform.GetChild(num).gameObject);
			}
			else
			{
				Object.DestroyImmediate(transform.GetChild(num).gameObject);
			}
		}
	}
}
