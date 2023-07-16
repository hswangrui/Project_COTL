using UnityEngine;

namespace src.Extensions
{
	public static class RectTransformExtensions
	{
		public static void SetRect(this RectTransform trs, float left, float top, float right, float bottom)
		{
			trs.offsetMin = new Vector2(left, bottom);
			trs.offsetMax = new Vector2(0f - right, 0f - top);
		}
	}
}
