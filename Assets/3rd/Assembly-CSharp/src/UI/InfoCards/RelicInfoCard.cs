using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class RelicInfoCard : UIInfoCardBase<RelicData>
	{
		[Header("Relic Info")]
		[SerializeField]
		private RectTransform _iconContainer;

		[SerializeField]
		private CanvasGroup _iconContainerCanvasgroup;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private GameObject _lock;

		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemLore;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[Header("Stats")]
		[SerializeField]
		private GameObject _statsContainer;

		[SerializeField]
		private GameObject _fragileIcon;

		[SerializeField]
		private TextMeshProUGUI _charge;

		public RectTransform IconContainer
		{
			get
			{
				return _iconContainer;
			}
		}

		public CanvasGroup IconCanvasGroup
		{
			get
			{
				return _iconContainerCanvasgroup;
			}
		}

		public override void Configure(RelicData config)
		{
			if (config != null)
			{
				_icon.sprite = config.UISprite;
				_icon.gameObject.SetActive(true);
				_lock.SetActive(false);
				_itemHeader.text = LocalizationManager.GetTranslation(string.Format("Relics/{0}", config.RelicType));
				_itemLore.text = LocalizationManager.GetTranslation(string.Format("Relics/{0}/Lore", config.RelicType));
				_itemDescription.text = LocalizationManager.GetTranslation(string.Format("Relics/{0}/Description", config.RelicType));
				_fragileIcon.SetActive(config.InteractionType == RelicInteractionType.Fragile);
				_statsContainer.SetActive(true);
				RelicChargeCategory chargeCategory = RelicData.GetChargeCategory(config);
				if (config.InteractionType == RelicInteractionType.Fragile)
				{
					_charge.text = LocalizationManager.GetTranslation("UI/Fragile");
				}
				else
				{
					_charge.text = LocalizationManager.GetTranslation("UI/Charge") + "<b>" + RelicData.GetChargeCategoryColor(chargeCategory) + LocalizationManager.GetTranslation(string.Format("UI/{0}", chargeCategory));
				}
			}
			else
			{
				_icon.gameObject.SetActive(false);
				_lock.SetActive(true);
				_itemHeader.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/NoRelic");
				_itemLore.gameObject.SetActive(false);
				_itemDescription.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/NoRelic/Description");
				_fragileIcon.SetActive(false);
				_statsContainer.SetActive(false);
			}
		}
	}
}
