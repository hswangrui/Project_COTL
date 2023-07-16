using Lamb.UI;
using Lamb.UI.PauseDetails;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class DoctrineInfoCardController : UIInfoCardController<DoctrineInfoCard, int>
	{
		protected override bool IsSelectionValid(Selectable selectable, out int showParam)
		{
			showParam = 0;
			DoctrineFragmentsItem component;
			if (selectable.TryGetComponent<DoctrineFragmentsItem>(out component) && (DataManager.Instance.CompletedDoctrineStones > 0 || DataManager.Instance.FirstDoctrineStone))
			{
				showParam = DataManager.Instance.DoctrineCurrentCount;
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
