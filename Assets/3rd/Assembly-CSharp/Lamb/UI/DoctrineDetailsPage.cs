using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class DoctrineDetailsPage : UISubmenuBase
	{
		[Header("Category Info")]
		[SerializeField]
		private Image _unlockIcon;

		[SerializeField]
		private TextMeshProUGUI _unlockTitle;

		[SerializeField]
		private TextMeshProUGUI _unlockType;

		[SerializeField]
		private TextMeshProUGUI _unlockTypeIcon;

		[SerializeField]
		private TextMeshProUGUI _unlockDescription;

		public void UpdateDetails(DoctrineUpgradeSystem.DoctrineType type)
		{
			_unlockTitle.text = DoctrineUpgradeSystem.GetLocalizedName(type).StripHtml();
			_unlockDescription.text = DoctrineUpgradeSystem.GetLocalizedDescription(type).StripColourHtml();
			_unlockIcon.sprite = DoctrineUpgradeSystem.GetIcon(type);
			_unlockTypeIcon.text = DoctrineUpgradeSystem.GetDoctrineUnlockIcon(type);
			_unlockType.text = DoctrineUpgradeSystem.GetDoctrineUnlockString(type);
		}
	}
}
