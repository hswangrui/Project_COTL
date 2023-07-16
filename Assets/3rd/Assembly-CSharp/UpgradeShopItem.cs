using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI.Alerts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeShopItem : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Action<UpgradeSystem.Type> OnUpgradeSelected;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private MMButton _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _canAffordIcon;

	[SerializeField]
	private Image _cantAffordIcon;

	[SerializeField]
	private Image _selectedIcon;

	[SerializeField]
	private TextMeshProUGUI _costText;

	[SerializeField]
	private UpgradeAlert _alert;

	[SerializeField]
	private Material _blackAndWhiteMaterial;

	private UpgradeSystem.Type _upgradeType;

	private Vector2 _anchoredOrigin;

	private bool _canAfford;

	private Material _blackAndWhiteMaterialInstance;

	private Coroutine cSelectionRoutine;

	public MMButton Button
	{
		get
		{
			return _button;
		}
	}

	public UpgradeSystem.Type Type
	{
		get
		{
			return _upgradeType;
		}
	}

	public void Configure(UpgradeSystem.Type type, float delay)
	{
		_upgradeType = type;
		_anchoredOrigin = _container.anchoredPosition;
		_icon.sprite = UpgradeSystem.GetIcon(type);
		_costText.text = StructuresData.ItemCost.GetCostString(UpgradeSystem.GetCost(type));
		_button.onClick.AddListener(OnButtonClicked);
		_alert.Configure(type);
		_selectedIcon.enabled = false;
		_canAfford = CheckCanAfford(type);
		_canAffordIcon.gameObject.SetActive(_canAfford);
		_cantAffordIcon.gameObject.SetActive(!_canAfford);
		if (!_canAfford)
		{
			_blackAndWhiteMaterialInstance = new Material(_blackAndWhiteMaterial);
			_blackAndWhiteMaterialInstance.SetFloat("_GrayscaleLerpFade", 1f);
			_icon.material = _blackAndWhiteMaterialInstance;
			_cantAffordIcon.material = _blackAndWhiteMaterialInstance;
		}
		StartCoroutine(FadeIn(delay));
	}

	private void OnButtonClicked()
	{
		if (_canAfford)
		{
			Action<UpgradeSystem.Type> onUpgradeSelected = OnUpgradeSelected;
			if (onUpgradeSelected != null)
			{
				onUpgradeSelected(_upgradeType);
			}
		}
		else
		{
			_container.transform.DOKill();
			_container.anchoredPosition = _anchoredOrigin;
			_container.transform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}
	}

	private IEnumerator FadeIn(float Delay)
	{
		_canvasGroup.alpha = 0f;
		yield return new WaitForSecondsRealtime(Delay);
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			_canvasGroup.alpha = Progress / Duration;
			yield return null;
		}
		_canvasGroup.alpha = 1f;
	}

	private bool CheckCanAfford(UpgradeSystem.Type type)
	{
		if (CheatConsole.BuildingsFree)
		{
			return true;
		}
		List<StructuresData.ItemCost> cost = UpgradeSystem.GetCost(type);
		for (int i = 0; i < cost.Count; i++)
		{
			if (Inventory.GetItemQuantity((int)cost[i].CostItem) < cost[i].CostValue)
			{
				return false;
			}
		}
		return true;
	}

	public void OnSelect(BaseEventData eventData)
	{
		_alert.TryRemoveAlert();
		_selectedIcon.enabled = true;
		if (cSelectionRoutine != null)
		{
			StopCoroutine(cSelectionRoutine);
		}
		cSelectionRoutine = StartCoroutine(Selected(base.transform.localScale.x, 1.2f));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_selectedIcon.enabled = false;
		if (cSelectionRoutine != null)
		{
			StopCoroutine(cSelectionRoutine);
		}
		cSelectionRoutine = StartCoroutine(DeSelected());
	}

	private IEnumerator Selected(float Starting, float Target)
	{
		float Progress = 0f;
		float Duration = 0.1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(Starting, Target, Progress / Duration);
			base.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		base.transform.localScale = Vector3.one * Target;
	}

	private IEnumerator DeSelected()
	{
		float Progress = 0f;
		float Duration = 0.3f;
		float StartingScale = base.transform.localScale.x;
		float TargetScale = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(StartingScale, TargetScale, Progress / Duration);
			base.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		base.transform.localScale = Vector3.one * TargetScale;
	}
}
