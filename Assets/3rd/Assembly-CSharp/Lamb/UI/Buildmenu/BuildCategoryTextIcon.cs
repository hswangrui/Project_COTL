using Lamb.UI.BuildMenu;
using TMPro;
using UnityEngine;

namespace Lamb.UI.Buildmenu
{
	[ExecuteInEditMode]
	public class BuildCategoryTextIcon : MonoBehaviour
	{
		[SerializeField]
		private UIBuildMenuController.Category _category;

		[SerializeField]
		private TextMeshProUGUI _label;

		public UIBuildMenuController.Category Category
		{
			get
			{
				return _category;
			}
		}
	}
}
