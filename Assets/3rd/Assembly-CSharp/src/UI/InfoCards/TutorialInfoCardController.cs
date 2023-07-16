using Lamb.UI;
using src.UI.Items;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class TutorialInfoCardController : UIInfoCardController<TutorialInfoCard, TutorialTopic>
	{
		protected override bool IsSelectionValid(Selectable selectable, out TutorialTopic showParam)
		{
			showParam = TutorialTopic.None;
			TutorialMenuItem component;
			if (selectable.TryGetComponent<TutorialMenuItem>(out component))
			{
				showParam = component.Topic;
				return true;
			}
			return false;
		}

		protected override TutorialTopic DefaultShowParam()
		{
			return TutorialTopic.None;
		}
	}
}
