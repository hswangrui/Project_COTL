using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.UpgradeMenu
{
	public class UpgradeMenuCard : MonoBehaviour
	{
		private const string kShownGenericAnimationState = "Shown";

		private const string kHiddenGenericAnimationState = "Hidden";

		private const string kShowTrigger = "Show";

		private const string kHideTrigger = "Hide";

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[Header("Copy")]
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private TextMeshProUGUI _loreText;

		[Header("Costs")]
		[SerializeField]
		private TextMeshProUGUI _costText;

		[Header("Graphics")]
		[SerializeField]
		private Image _icon;

		public void Show(UpgradeSystem.Type Type, bool instant = false)
		{
			_icon.sprite = UpgradeSystem.GetIcon(Type);
			_headerText.text = UpgradeSystem.GetLocalizedName(Type);
			_descriptionText.text = UpgradeSystem.GetLocalizedDescription(Type);
			_loreText.text = "";
			_costText.text = GetCostText(Type);
			ResetTriggers();
			if (instant)
			{
				_animator.Play("Shown");
			}
			else
			{
				_animator.SetTrigger("Show");
			}
			Debug.Log("Show card");
		}

		public void Hide(bool instant = false)
		{
			ResetTriggers();
			if (instant)
			{
				_animator.Play("Hidden");
			}
			else
			{
				_animator.SetTrigger("Hide");
			}
		}

		private void ResetTriggers()
		{
			_animator.ResetTrigger("Show");
			_animator.ResetTrigger("Hide");
		}

		private string GetCostText(UpgradeSystem.Type type)
		{
			string text = "";
			List<StructuresData.ItemCost> cost = UpgradeSystem.GetCost(type);
			for (int i = 0; i < cost.Count; i++)
			{
				int itemQuantity = Inventory.GetItemQuantity((int)cost[i].CostItem);
				int costValue = cost[i].CostValue;
				text += ((costValue > itemQuantity) ? "<color=#ff0000>" : "<color=#FEF0D3>");
				text += FontImageNames.GetIconByType(cost[i].CostItem);
				text = text + itemQuantity + "</color> / " + costValue.ToString() + "  ";
			}
			return text;
		}
	}
}
