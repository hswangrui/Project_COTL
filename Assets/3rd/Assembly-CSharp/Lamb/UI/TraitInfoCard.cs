using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class TraitInfoCard : UIInfoCardBase<FollowerTrait.TraitType>
	{
		[SerializeField]
		private TextMeshProUGUI _traitTitle;

		[SerializeField]
		private TextMeshProUGUI _traitDescription;

		[SerializeField]
		private IndoctrinationTraitItem _traitItem;

		public override void Configure(FollowerTrait.TraitType trait)
		{
			_traitTitle.text = FollowerTrait.GetLocalizedTitle(trait);
			_traitDescription.text = FollowerTrait.GetLocalizedDescription(trait).StripHtml();
			_traitItem.Configure(trait);
		}
	}
}
