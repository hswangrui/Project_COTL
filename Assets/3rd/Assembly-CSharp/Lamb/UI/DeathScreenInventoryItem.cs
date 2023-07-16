using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class DeathScreenInventoryItem : UIInventoryItem
	{
		[SerializeField]
		private TextMeshProUGUI _quantityDelta;

		public InventoryItem Item;

		public TextMeshProUGUI AmountText
		{
			get
			{
				return _amountText;
			}
		}

		public TextMeshProUGUI DeltaText
		{
			get
			{
				return _quantityDelta;
			}
		}

		public override void Configure(InventoryItem item, bool showQuantity = true)
		{
			base.Configure(item, showQuantity);
			_quantityDelta.text = "";
			Item = item;
		}

		public void ShowDelta(int delta)
		{
			_quantityDelta.text = delta.ToString();
			if (delta > 0)
			{
				_quantityDelta.text = "+" + _quantityDelta.text;
				_quantityDelta.color = StaticColors.GreenColor;
			}
			else if (delta < 0)
			{
				_quantityDelta.color = StaticColors.RedColor;
			}
			else
			{
				_quantityDelta.text = "";
			}
		}
	}
}
