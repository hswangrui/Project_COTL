using UnityEngine;

namespace Lamb.UI
{
	public class KeybindItemProxy : MonoBehaviour
	{
		[SerializeField]
		private KeybindItem _keybindItem;

		public KeybindItem KeybindItem
		{
			get
			{
				return _keybindItem;
			}
		}
	}
}
