using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class RecipeItem : UIInventoryItem, ISelectHandler, IEventSystemHandler
	{
		public Action<RecipeItem> OnRecipeChosen;

		[Header("Progress")]
		[SerializeField]
		private Image _alert;

		[SerializeField]
		private GameObject _hungerContainer;

		[SerializeField]
		private Image _hungerFill;

		[SerializeField]
		private GameObject _starContainer;

		[SerializeField]
		private GameObject[] _starFills;

		[SerializeField]
		private GameObject _removeIcon;

		private bool _isQueued;

		private bool _discovered;

		private bool _canAfford;

		public void Configure(InventoryItem.ITEM_TYPE type, bool showQuantity = true, bool isQueued = false)
		{
			_discovered = CookingData.HasRecipeDiscovered(type);
			base.Configure(type, showQuantity);
			if (_button != null)
			{
				_button.onClick.AddListener(OnButtonClicked);
				MMButton button = _button;
				button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(base.Shake));
			}
			float tummyRating = CookingData.GetTummyRating(base.Type);
			_hungerFill.fillAmount = tummyRating;
			if (tummyRating <= 0.25f)
			{
				_hungerFill.color = StaticColors.RedColor;
			}
			else if (tummyRating <= 0.5f)
			{
				_hungerFill.color = StaticColors.OrangeColor;
			}
			else
			{
				_hungerFill.color = StaticColors.GreenColor;
			}
			if (!showQuantity)
			{
				int satationLevel = CookingData.GetSatationLevel(type);
				for (int i = 0; i < _starFills.Length; i++)
				{
					_starFills[i].SetActive(satationLevel >= i + 1);
				}
			}
			else
			{
				_starContainer.SetActive(false);
			}
			if (_alert != null)
			{
				_alert.enabled = false;
			}
			_removeIcon.SetActive(isQueued);
		}

		public void OnSelect(BaseEventData eventData)
		{
		}

		private void OnButtonClicked()
		{
			Action<RecipeItem> onRecipeChosen = OnRecipeChosen;
			if (onRecipeChosen != null)
			{
				onRecipeChosen(this);
			}
		}

		public override void UpdateQuantity()
		{
			if (!_showQuantity && !_isQueued)
			{
				return;
			}
			_canAfford = CookingData.CanMakeMeal(base.Type);
			if (_button != null)
			{
				_button.Confirmable = _discovered && _canAfford;
			}
			if (_showQuantity && !_isQueued)
			{
				_quantity = CookingData.GetCookableRecipeAmount(base.Type, Inventory.items);
				_amountText.text = _quantity.ToString();
				_amountText.color = ((_quantity <= 0) ? StaticColors.RedColor : StaticColors.OffWhiteColor);
				if (_quantity <= 0)
				{
					_icon.color = (_discovered ? new Color(0f, 1f, 1f, 1f) : Color.black);
					if (!_discovered)
					{
						_hungerContainer.gameObject.SetActive(false);
						_amountText.text = "";
					}
				}
				else
				{
					_icon.color = new Color(1f, 1f, 1f, 1f);
				}
				return;
			}
			if (!_isQueued && !_canAfford)
			{
				if (_discovered)
				{
					_icon.color = new Color(0f, 1f, 1f, 1f);
				}
				else
				{
					_icon.color = Color.black;
				}
			}
			_amountText.text = "";
		}
	}
}
