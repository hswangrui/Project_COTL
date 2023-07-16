using UnityEngine.UI;

namespace Lamb.UI
{
	public class TraitInfoCardController : UIInfoCardController<TraitInfoCard, FollowerTrait.TraitType>
	{
		protected override bool IsSelectionValid(Selectable selectable, out FollowerTrait.TraitType showParam)
		{
			showParam = FollowerTrait.TraitType.None;
			IndoctrinationTraitItem component;
			if (selectable.TryGetComponent<IndoctrinationTraitItem>(out component))
			{
				showParam = component.TraitType;
				return true;
			}
			return false;
		}
	}
}
