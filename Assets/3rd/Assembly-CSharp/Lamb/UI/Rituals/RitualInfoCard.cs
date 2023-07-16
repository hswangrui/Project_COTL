using System.Collections.Generic;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Rituals
{
	public class RitualInfoCard : UIInfoCardBase<UpgradeSystem.Type>
	{
		[Header("Copy")]
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private TextMeshProUGUI _faithText;

		[SerializeField]
		private GameObject _faithContainer;

		[SerializeField]
		private BarController _faithBar;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private RitualIconMapping _iconMapping;

		[Header("Limited Time")]
		[SerializeField]
		private GameObject _limitedTimeIcon;

		[SerializeField]
		private GameObject _limitedTimeText;

		[Header("Costs")]
		[SerializeField]
		private TextMeshProUGUI[] _costTexts;

		public override void Configure(UpgradeSystem.Type config)
		{
			_headerText.text = UpgradeSystem.GetLocalizedName(config);
			_descriptionText.text = UpgradeSystem.GetLocalizedDescription(config);
			_icon.sprite = _iconMapping.GetImage(config);
			_limitedTimeIcon.SetActive(UpgradeSystem.IsSpecialRitual(config));
			_limitedTimeText.SetActive(UpgradeSystem.IsSpecialRitual(config));
			List<StructuresData.ItemCost> cost = UpgradeSystem.GetCost(config);
			for (int i = 0; i < _costTexts.Length; i++)
			{
				if (i >= cost.Count)
				{
					_costTexts[i].gameObject.SetActive(false);
					continue;
				}
				_costTexts[i].text = cost[i].ToStringShowQuantity();
				_costTexts[i].gameObject.SetActive(true);
			}
			if (_faithContainer != null)
			{
				float ritualFaithChange = UpgradeSystem.GetRitualFaithChange(config);
				FollowerTrait.TraitType ritualTrait = UpgradeSystem.GetRitualTrait(config);
				_faithContainer.SetActive(ritualFaithChange != 0f);
				Color colour = ((ritualFaithChange > 0f) ? StaticColors.GreenColor : StaticColors.RedColor);
				_faithText.text = (((ritualFaithChange > 0f) ? "<sprite name=\"icon_FaithUp\">" : "<sprite name=\"icon_FaithDown\">") + Mathf.Abs(ritualFaithChange)).Colour(colour) + ((ritualTrait != 0 && DataManager.Instance.CultTraits.Contains(ritualTrait)) ? (" (" + FollowerTrait.GetLocalizedTitle(ritualTrait) + ")") : "");
				if (ritualFaithChange < 0f)
				{
					_faithBar.SetBarSizeForInfo(CultFaithManager.CultFaithNormalised, (CultFaithManager.CurrentFaith + ritualFaithChange) / 85f, FollowerBrainStats.BrainWashed);
				}
				else
				{
					_faithBar.SetBarSizeForInfo((CultFaithManager.CurrentFaith + ritualFaithChange) / 85f, CultFaithManager.CultFaithNormalised, FollowerBrainStats.BrainWashed);
				}
			}
		}
	}
}
