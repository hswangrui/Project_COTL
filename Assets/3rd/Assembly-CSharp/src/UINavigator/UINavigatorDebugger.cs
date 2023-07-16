using UnityEngine;
using UnityEngine.UI;

namespace src.UINavigator
{
	[RequireComponent(typeof(UINavigatorNew))]
	public class UINavigatorDebugger : MonoBehaviour
	{
		private UINavigatorNew _uiNavigatorNew;

		public Selectable _currentSelectable
		{
			get
			{
				if (_uiNavigatorNew.CurrentSelectable == null)
				{
					return null;
				}
				return _uiNavigatorNew.CurrentSelectable.Selectable;
			}
		}

		public void Awake()
		{
			_uiNavigatorNew = GetComponent<UINavigatorNew>();
		}

		public void Update()
		{
		}
	}
}
