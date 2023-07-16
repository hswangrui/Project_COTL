using System;
using DG.Tweening;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class CrownAbilityItemBuyable : BaseMonoBehaviour
	{
		private const string kUnlockedLayer = "Unlocked";

		private const string kLockedLayer = "Locked";

		public Action<CrownAbilityItemBuyable> OnUpgradeChosen;

		[SerializeField]
		private RectTransform _shakeContainer;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private TextMeshProUGUI _costText;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private UpgradeTypeMapping _iconMapping;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private GameObject _alert;

		[SerializeField]
		private Image _flashIcon;

		private UpgradeSystem.Type _type;

		private Vector2 _origin;

		private StructuresData.ItemCost _cost;

		private bool _unlocked;

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
				return _type;
			}
		}

		public StructuresData.ItemCost Cost
		{
			get
			{
				return _cost;
			}
		}

		public void Configure(UpgradeSystem.Type type)
		{
			_type = type;
			_origin = _shakeContainer.anchoredPosition;
			_cost = UpgradeSystem.GetCost(_type)[0];
			_unlocked = UpgradeSystem.GetUnlocked(_type);
			UpdateState();
			UpgradeTypeMapping.SpriteItem item = _iconMapping.GetItem(_type);
			_icon.sprite = item.Sprite;
			_button.onClick.AddListener(OnButtonClicked);
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(Shake));
			_flashIcon.gameObject.SetActive(false);
		}

		public void UpdateState()
		{
			_button.Confirmable = _cost.CanAfford() && !_unlocked;
			_alert.SetActive(_cost.CanAfford() && !_unlocked);
			if (!_unlocked)
			{
				if (_cost.CanAfford())
				{
					_icon.color = StaticColors.OffWhiteColor;
				}
				else
				{
					_icon.color = StaticColors.GreyColor;
				}
				_costText.text = StructuresData.ItemCost.GetCostString(_cost);
			}
			else
			{
				_costText.text = "";
				_icon.color = StaticColors.OffWhiteColor;
			}
		}

		private void OnButtonClicked()
		{
			Action<CrownAbilityItemBuyable> onUpgradeChosen = OnUpgradeChosen;
			if (onUpgradeChosen != null)
			{
				onUpgradeChosen(this);
			}
		}

		public void ForceIncognitoState()
		{
			if (_alert != null)
			{
				_alert.gameObject.SetActive(false);
			}
			_costText.text = "";
			_icon.color = new Color(0f, 1f, 1f, 1f);
			_flashIcon.gameObject.SetActive(true);
			_flashIcon.color = new Color(0f, 0f, 0f, 0.5f);
		}

		public void AnimateIncognitoOut()
		{
			_icon.DOKill();
			_icon.DOColor(Color.white, 0.25f).SetUpdate(true);
			_flashIcon.DOKill();
			_flashIcon.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(delegate
			{
				if (!_unlocked)
				{
					if (_cost.CanAfford())
					{
						_icon.color = StaticColors.OffWhiteColor;
					}
					else
					{
						_icon.color = StaticColors.GreyColor;
					}
					_costText.text = StructuresData.ItemCost.GetCostString(_cost);
				}
				else
				{
					_costText.text = "";
					_icon.color = StaticColors.OffWhiteColor;
				}
			});
		}

		private void Shake()
		{
			_shakeContainer.DOKill();
			_shakeContainer.localScale = Vector2.one;
			_shakeContainer.anchoredPosition = _origin;
			_shakeContainer.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		public void Bump()
		{
			_shakeContainer.localScale = Vector3.one * 1.4f;
			_shakeContainer.DOKill();
			_shakeContainer.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
		}
	}
}
