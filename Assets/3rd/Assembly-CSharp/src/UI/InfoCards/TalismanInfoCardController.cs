using Lamb.UI;
using Lamb.UI.PauseDetails;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class TalismanInfoCardController : UIInfoCardController<TalismanInfoCard, int>
	{
		protected override bool IsSelectionValid(Selectable selectable, out int showParam)
		{
			showParam = 0;
			TalismanPiecesItem component;
			if (selectable.TryGetComponent<TalismanPiecesItem>(out component) && (Inventory.KeyPieces + Inventory.TempleKeys > 0 || DataManager.Instance.HadFirstTempleKey))
			{
				showParam = Inventory.KeyPieces;
				return true;
			}
			return false;
		}

		protected override int DefaultShowParam()
		{
			return -1;
		}
	}
}
