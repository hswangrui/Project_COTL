using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lamb.UI.SermonWheelOverlay
{
	public class UISermonWheelController : UIRadialMenuBase<SermonWheelCategory, SermonCategory>
	{
		private InventoryItem.ITEM_TYPE _currency;

		private Material _material;

		[SerializeField]
		private GameObject _wheelItemsContainer;

		[SerializeField]
		private GameObject _crystalItemsContainer;

		[SerializeField]
		private List<SermonWheelCategory> _crystalItems;

		protected override bool SelectOnHighlight
		{
			get
			{
				return false;
			}
		}

		public void Show(InventoryItem.ITEM_TYPE currency, bool instant = false)
		{
			_currency = currency;
			if (_currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
			{
				_wheelItems = _crystalItems;
				_wheelItemsContainer.SetActive(false);
			}
			else
			{
				_crystalItemsContainer.SetActive(false);
			}
			foreach (SermonWheelCategory wheelItem in _wheelItems)
			{
				wheelItem.Configure(_currency);
			}
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			if (_currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
			{
				_radialMaterialInstance.SetColor("_HighlightColor", StaticColors.BlueColourHex.ColourFromHex());
			}
			base.OnShowStarted();
		}

		protected override void OnChoiceFinalized()
		{
			_finalizedSelection = true;
			Hide();
		}

		protected override void MakeChoice(SermonWheelCategory item)
		{
			Action<SermonCategory> onItemChosen = OnItemChosen;
			if (onItemChosen != null)
			{
				onItemChosen(item.SermonCategory);
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_finalizedSelection = true;
				Hide();
			}
		}
	}
}
