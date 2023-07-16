using I2.Loc;
using Lamb.UI;
using Lamb.UI.PauseDetails;
using TMPro;
using UnityEngine;

namespace src.UI.InfoCards
{
	public class DoctrineInfoCard : UIInfoCardBase<int>
	{
		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private DoctrineFragmentsItem _doctrineFragmentsItem;

		public override void Configure(int pieces)
		{
			_itemHeader.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/DoctrineFragments");
			_itemDescription.text = string.Format(LocalizationManager.GetTranslation("UI/PauseScreen/Player/DoctrineFragments/Description"), pieces, 3);
			_doctrineFragmentsItem.Configure(pieces);
		}
	}
}
