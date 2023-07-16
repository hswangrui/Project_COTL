using I2.Loc;
using Lamb.UI;
using Lamb.UI.PauseDetails;
using TMPro;
using UnityEngine;

namespace src.UI.InfoCards
{
	public class TalismanInfoCard : UIInfoCardBase<int>
	{
		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private TalismanPiecesItem _talismanPiecesItem;

		public override void Configure(int pieces)
		{
			_itemHeader.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/TalismanPieces");
			_itemDescription.text = string.Format(LocalizationManager.GetTranslation("UI/PauseScreen/Player/TalismanPieces/Description"), pieces, 4);
			_talismanPiecesItem.Configure(pieces);
		}
	}
}
