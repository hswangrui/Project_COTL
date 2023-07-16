using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class SermonCategoryTextIcon : MonoBehaviour
	{
		[SerializeField]
		private SermonCategory _sermonCategory;

		[SerializeField]
		private TextMeshProUGUI _label;

		public SermonCategory SermonCategory
		{
			get
			{
				return _sermonCategory;
			}
		}

		public void SetLock()
		{
			_label.text = "\uf30d";
		}

		public void SetHidden()
		{
			_label.text = "";
		}
	}
}
