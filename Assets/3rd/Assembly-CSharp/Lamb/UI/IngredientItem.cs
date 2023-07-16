using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lamb.UI
{
	public class IngredientItem : UIInventoryItem, ISelectHandler, IEventSystemHandler
	{
		[SerializeField]
		protected InventoryAlert _alert;

		[SerializeField]
		private GameObject[] _starFills;

		[SerializeField]
		private GameObject _removeIcon;

		public void Configure(InventoryItem.ITEM_TYPE type, bool queued, bool showQuantity = true)
		{
			base.Configure(type, showQuantity);
			if (_alert != null)
			{
				_alert.Configure(type);
			}
			int starRating = GetStarRating();
			for (int i = 0; i < _starFills.Length; i++)
			{
				_starFills[i].SetActive(starRating >= i + 1);
			}
			_removeIcon.SetActive(queued);
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (_alert != null)
			{
				_alert.TryRemoveAlert();
			}
		}

		private int GetStarRating()
		{
			switch (CookingData.GetIngredientType(base.Type))
			{
			case CookingData.IngredientType.VEGETABLE_MEDIUM:
			case CookingData.IngredientType.MEAT_MEDIUM:
			case CookingData.IngredientType.FISH_MEDIUM:
			case CookingData.IngredientType.SPECIAL_FOLLOWER_MEAT:
				return 2;
			case CookingData.IngredientType.VEGETABLE_HIGH:
			case CookingData.IngredientType.MEAT_HIGH:
			case CookingData.IngredientType.FISH_HIGH:
				return 3;
			default:
				return 1;
			}
		}
	}
}
