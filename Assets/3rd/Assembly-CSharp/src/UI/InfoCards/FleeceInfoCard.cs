using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

namespace src.UI.InfoCards
{
	public class FleeceInfoCard : UIInfoCardBase<int>
	{
		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private FleeceItem _fleeceItem;

		[SerializeField]
		private GameObject _costHeader;

		[SerializeField]
		private GameObject _costContainer;

		[SerializeField]
		private TextMeshProUGUI _costText;

		public GameObject _redOutline;

		public override void Configure(int fleece)
		{
			_redOutline.SetActive(false);
			_fleeceItem.Configure(fleece);
			_itemHeader.text = LocalizationManager.GetTranslation(string.Format("TarotCards/Fleece{0}/Name", fleece));
			_itemDescription.text = LocalizationManager.GetTranslation(string.Format("TarotCards/Fleece{0}/Description", fleece));
			if (!DataManager.Instance.UnlockedFleeces.Contains(fleece))
			{
				StructuresData.ItemCost itemCost = new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.TALISMAN, 1);
				_costText.text = itemCost.ToStringShowQuantity();
				_costHeader.SetActive(true);
				_costContainer.SetActive(true);
			}
			else
			{
				_costHeader.SetActive(false);
				_costContainer.SetActive(false);
			}
		}
	}
}
