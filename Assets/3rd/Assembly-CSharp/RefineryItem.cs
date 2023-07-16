using System;
using System.Collections;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RefineryItem : UIInventoryItem, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Action<RefineryItem> OnItemSelected;

	[SerializeField]
	private GameObject _progressContainer;

	[SerializeField]
	private Image _progressRing;

	[SerializeField]
	private Image _canAffordIcon;

	[SerializeField]
	private Image _cantAffordIcon;

	[SerializeField]
	private Image _selectedIcon;

	[SerializeField]
	private TextMeshProUGUI _costText;

	[SerializeField]
	private CanvasGroup _cancelCanvasGroup;

	private Vector2 _anchoredOrigin;

	private bool _canAfford;

	private bool _queued;

	private bool _firstQueuedResource;

	public bool CanAfford
	{
		get
		{
			return _canAfford;
		}
	}

	private void Awake()
	{
		_button.onClick.AddListener(OnButtonClicked);
		MMButton button = _button;
		button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(base.Shake));
	}

	public void Configure(InventoryItem.ITEM_TYPE type, bool queued = false, int queuedIndex = 0, bool showQuantity = true)
	{
		base.Configure(type, showQuantity);
		_queued = queued;
		_anchoredOrigin = _container.anchoredPosition;
		_selectedIcon.enabled = false;
		_cancelCanvasGroup.alpha = 0f;
		_costText.gameObject.SetActive(!queued);
		_costText.text = StructuresData.ItemCost.GetCostString(Structures_Refinery.GetCost(type));
		if (_amountText != null)
		{
			_amountText.gameObject.SetActive(Structures_Refinery.GetAmount(type) > 1);
			_amountText.text = "x" + Structures_Refinery.GetAmount(type);
		}
		_firstQueuedResource = queuedIndex == 0;
		_progressContainer.SetActive(queued && _firstQueuedResource);
		if (!_queued)
		{
			_button.Confirmable = _canAfford;
		}
		else
		{
			_button.Confirmable = true;
		}
	}

	public override void UpdateQuantity()
	{
		_canAfford = CheckCanAfford(base.Type) && !_queued;
		_costText.text = StructuresData.ItemCost.GetCostString(Structures_Refinery.GetCost(base.Type));
		if (!_queued)
		{
			_button.Confirmable = _canAfford;
			if (_showQuantity)
			{
				_icon.color = new Color(_canAfford ? 1 : 0, 1f, 1f, 1f);
				_canAffordIcon.color = new Color(_canAfford ? 1 : 0, 1f, 1f, 1f);
			}
			_canAffordIcon.gameObject.SetActive(_canAfford);
			_cantAffordIcon.gameObject.SetActive(!_canAfford);
		}
		else
		{
			_button.Confirmable = true;
			_canAffordIcon.gameObject.SetActive(true);
			_cantAffordIcon.gameObject.SetActive(false);
		}
	}

	private void OnButtonClicked()
	{
		Action<RefineryItem> onItemSelected = OnItemSelected;
		if (onItemSelected != null)
		{
			onItemSelected(this);
		}
	}

	public void UpdateRefiningProgress(float normTime)
	{
		_progressRing.fillAmount = normTime;
	}

	private bool CheckCanAfford(InventoryItem.ITEM_TYPE type)
	{
		if (CheatConsole.BuildingsFree)
		{
			return true;
		}
		foreach (StructuresData.ItemCost item in Structures_Refinery.GetCost(type))
		{
			if (item.CanAfford())
			{
				return true;
			}
		}
		return false;
	}

	public void OnSelect(BaseEventData eventData)
	{
		_selectedIcon.enabled = true;
		StopAllCoroutines();
		StartCoroutine(Selected(base.transform.localScale.x, 1.2f));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_selectedIcon.enabled = false;
		StopAllCoroutines();
		StartCoroutine(DeSelected());
	}

	private IEnumerator Selected(float starting, float target)
	{
		_canvasGroup.alpha = 1f;
		base.transform.localScale = Vector3.one;
		float progress = 0f;
		float duration = 0.1f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.unscaledDeltaTime);
			if (!(num < duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(starting, target, progress / duration);
			if (_queued)
			{
				_cancelCanvasGroup.alpha = progress / duration;
			}
			base.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		base.transform.localScale = Vector3.one * target;
		if (_queued)
		{
			_cancelCanvasGroup.alpha = 1f;
		}
	}

	private IEnumerator DeSelected()
	{
		float progress = 0f;
		float duration = 0.3f;
		float startingScale = base.transform.localScale.x;
		float targetScale = 1f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.unscaledDeltaTime);
			if (!(num < duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(startingScale, targetScale, progress / duration);
			base.transform.localScale = Vector3.one * num2;
			if (_queued)
			{
				_cancelCanvasGroup.alpha = 1f - progress / duration;
			}
			yield return null;
		}
		base.transform.localScale = Vector3.one * targetScale;
		if (_queued)
		{
			_cancelCanvasGroup.alpha = 0f;
		}
	}
}
