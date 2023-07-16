using UnityEngine;

namespace src.Extensions
{
	public static class GameObjectExtensions
	{
		public static T Instantiate<T>(this T component) where T : MonoBehaviour
		{
			return Object.Instantiate(component.gameObject).GetComponent<T>();
		}

		public static T Instantiate<T>(this T component, Transform parent, bool worldPositionStays = true) where T : MonoBehaviour
		{
			T val = component.Instantiate();
			val.transform.SetParent(parent, worldPositionStays);
			val.transform.localScale = Vector3.one;
			return val;
		}
	}
}
