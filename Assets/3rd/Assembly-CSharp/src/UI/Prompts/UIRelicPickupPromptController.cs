using TMPro;
using UnityEngine;

namespace src.UI.Prompts
{
	public class UIRelicPickupPromptController : UIPromptBase
	{
		[Header("Content")]
		[SerializeField]
		private TextMeshProUGUI _title;

		[SerializeField]
		private TextMeshProUGUI _lore;

		[SerializeField]
		private TextMeshProUGUI _description;

		[Header("Stats")]
		[SerializeField]
		private TextMeshProUGUI _fragile;

		[SerializeField]
		private GameObject _fragileIcon;

		private RelicData _relicData;

		private RelicData _compareTo;

		protected override bool _addToActiveMenus
		{
			get
			{
				return false;
			}
		}

		public void Show(RelicData relicData, RelicData compareTo, bool instant = false)
		{
			Show(instant);
			_relicData = relicData;
			_compareTo = compareTo;
			_fragile.gameObject.SetActive(_relicData.InteractionType == RelicInteractionType.Fragile);
		}

		protected override void Localize()
		{
			_title.text = RelicData.GetTitleLocalisation(_relicData.RelicType);
			_lore.text = RelicData.GetLoreLocalization(_relicData.RelicType);
			_description.text = RelicData.GetDescriptionLocalisation(_relicData.RelicType);
			_fragile.gameObject.SetActive(_relicData.InteractionType == RelicInteractionType.Fragile);
			_fragileIcon.gameObject.SetActive(_relicData.InteractionType == RelicInteractionType.Fragile);
		}
	}
}
