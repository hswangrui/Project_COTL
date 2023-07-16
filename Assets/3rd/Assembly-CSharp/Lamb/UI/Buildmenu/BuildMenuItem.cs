using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI.Alerts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lamb.UI.BuildMenu
{
	public class BuildMenuItem : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public Action<StructureBrain.TYPES> OnStructureSelected;

		[SerializeField]
		private RectTransform _rectTransform;

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
		private Image _flashIcon;

		[SerializeField]
		private TextMeshProUGUI _costText;

		[SerializeField]
		private StructureAlert _alert;

		[SerializeField]
		private Material _blackAndWhiteMaterial;

		[SerializeField]
		private GameObject _lockedContainer;

		private Material _blackAndWhiteMaterialInstance;

		public StructureBrain.TYPES _structureType;

		private bool _alreadyBuilt;

		private Vector2 _anchoredOrigin;

		private bool _locked;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public StructureBrain.TYPES Structure
		{
			get
			{
				return _structureType;
			}
		}

		public bool Locked
		{
			get
			{
				return _locked;
			}
		}

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		private bool _clickable
		{
			get
			{
				return _button.Confirmable;
			}
			set
			{
				_button.Confirmable = value;
			}
		}

		public void Configure(StructureBrain.TYPES structureType)
		{
			_anchoredOrigin = _container.anchoredPosition;
			_structureType = structureType;
			Image icon = _icon;
			TypeAndPlacementObject byType = TypeAndPlacementObjects.GetByType(_structureType);
			icon.sprite = ((byType != null) ? byType.IconImage : null);
			_costText.text = StructuresData.ItemCost.GetCostString(StructuresData.GetCost(structureType));
			_button.onClick.AddListener(OnButtonClicked);
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(OnButtonClickDenied));
			_button.Confirmable = false;
			_alert.Configure(_structureType);
			_selectedIcon.enabled = false;
			_clickable = false;
			_locked = !StructuresData.GetUnlocked(structureType);
			_lockedContainer.SetActive(_locked);
			if (_locked)
			{
				SetBlackAndWhite();
				_costText.text = "";
				_locked = true;
			}
			else if (StructuresData.RequiresTempleToBuild(_structureType) && !StructuresData.HasTemple())
			{
				SetBlackAndWhite();
				_costText.text = "-";
			}
			else if (StructuresData.GetBuildOnlyOne(_structureType) && (StructureManager.IsBuilt(_structureType) || StructureManager.IsBuilding(_structureType) || StructureManager.IsAnyUpgradeBuiltOrBuilding(_structureType)))
			{
				SetBlackAndWhite();
				_costText.text = "-";
			}
			else if (StructuresData.IsUpgradeStructure(_structureType) && StructureManager.GetAllStructuresOfType(StructuresData.GetUpgradePrerequisite(_structureType)).Count <= 0)
			{
				SetBlackAndWhite();
			}
			else if (!CheckCanAfford(_structureType))
			{
				SetBlackAndWhite();
			}
			else
			{
				_clickable = true;
			}
		}

		public void SetBlackAndWhite()
		{
			_blackAndWhiteMaterialInstance = new Material(_blackAndWhiteMaterial);
			_blackAndWhiteMaterialInstance.SetFloat("_GrayscaleLerpFade", 1f);
			_icon.material = _blackAndWhiteMaterialInstance;
			_cantAffordIcon.material = _blackAndWhiteMaterialInstance;
			_canAffordIcon.gameObject.SetActive(_clickable);
			_cantAffordIcon.gameObject.SetActive(!_clickable);
		}

		public void ForceIncognitoState()
		{
			SetBlackAndWhite();
			_alert.gameObject.SetActive(false);
			_costText.text = "";
			_cantAffordIcon.gameObject.SetActive(true);
			_canAffordIcon.gameObject.SetActive(false);
		}

		public void ForceLockedState()
		{
			_alert.gameObject.SetActive(false);
			_lockedContainer.SetActive(true);
			_costText.text = "";
		}

		public IEnumerator DoUnlock()
		{
			_container.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			Configure(_structureType);
			if (_blackAndWhiteMaterialInstance != null)
			{
				UnityEngine.Object.Destroy(_blackAndWhiteMaterialInstance);
				_icon.material = null;
				_cantAffordIcon.material = null;
			}
			_flashIcon.gameObject.SetActive(true);
			_alert.gameObject.SetActive(false);
			UIManager.PlayAudio("event:/unlock_building/unlock");
			_container.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			_flashIcon.DOColor(new Color(1f, 1f, 1f, 0f), 0.25f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			yield return new WaitForSecondsRealtime(0.1f);
			Vector3 endValue = new Vector3(0.3f, 0.3f, 1f);
			_alert.transform.localScale = Vector3.zero;
			_alert.transform.DOScale(endValue, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
			_alert.gameObject.SetActive(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}

		private bool CheckCanAfford(StructureBrain.TYPES type)
		{
			if (CheatConsole.BuildingsFree)
			{
				return true;
			}
			foreach (StructuresData.ItemCost item in StructuresData.GetCost(type))
			{
				if (!item.CanAfford())
				{
					return false;
				}
			}
			return true;
		}

		private void OnButtonClicked()
		{
			Action<StructureBrain.TYPES> onStructureSelected = OnStructureSelected;
			if (onStructureSelected != null)
			{
				onStructureSelected(_structureType);
			}
		}

		private void OnButtonClickDenied()
		{
			_container.DOKill();
			_container.anchoredPosition = _anchoredOrigin;
			_container.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		private void OnDisable()
		{
			_canvasGroup.alpha = 1f;
			base.transform.localScale = Vector3.one;
			_flashIcon.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy(_blackAndWhiteMaterialInstance);
			_blackAndWhiteMaterialInstance = null;
		}

		public void OnSelect(BaseEventData eventData)
		{
			_selectedIcon.enabled = true;
			_alert.TryRemoveAlert();
			StopAllCoroutines();
			StartCoroutine(DoSelected(base.transform.localScale.x, 1.2f));
		}

		public void OnDeselect(BaseEventData eventData)
		{
			_selectedIcon.enabled = false;
			StopAllCoroutines();
			StartCoroutine(DoDeSelected());
		}

		private IEnumerator DoSelected(float starting, float target)
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
				base.transform.localScale = Vector3.one * num2;
				yield return null;
			}
			base.transform.localScale = Vector3.one * target;
		}

		private IEnumerator DoDeSelected()
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
				yield return null;
			}
			base.transform.localScale = Vector3.one * targetScale;
		}
	}
}
