using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.Menus
{
	public class PhotoInfoCardController : UIInfoCardController<PhotoInfoCard, PhotoModeManager.PhotoData>
	{
		protected override bool IsSelectionValid(Selectable selectable, out PhotoModeManager.PhotoData showParam)
		{
			showParam = null;
			PhotoInformationBox component;
			if (selectable.TryGetComponent<PhotoInformationBox>(out component))
			{
				showParam = component.PhotoData;
				return true;
			}
			return false;
		}
	}
}
