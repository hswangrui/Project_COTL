using UnityEngine;

namespace Unify.Utils
{
	internal class PopupGroup : MonoBehaviour
	{
		public void CloseAll()
		{
			PopupMenu[] componentsInChildren = base.gameObject.transform.GetComponentsInChildren<PopupMenu>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: false);
			}
		}
	}
}
