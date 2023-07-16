using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI.Alerts;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Rituals
{
	public class RitualItem : MonoBehaviour
	{
		private const string kUnlockedLayer = "Unlocked";

		private const string kLockedLayer = "Locked";

		public Action<UpgradeSystem.Type> OnRitualItemSelected;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private RitualAlert _alert;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private Image _ritualImage;

		[SerializeField]
		private TextMeshProUGUI _costText;

		[SerializeField]
		private RectTransform _iconContainer;

		[SerializeField]
		private GameObject _cooldownContainer;

		[SerializeField]
		private Image _cooldownFill;

		[SerializeField]
		private Image _flashIcon;

		[SerializeField]
		private RitualIconMapping _iconMapping;

		private UpgradeSystem.Type _ritualType;

		private bool _locked;

		private bool _maxed;

		private bool _cooldown;

		private bool _canAfford;

		private Vector2 _anchoredOrigin;

		public UpgradeSystem.Type RitualType
		{
			get
			{
				return _ritualType;
			}
		}

		public bool Locked
		{
			get
			{
				return _locked;
			}
		}

		public bool Maxed
		{
			get
			{
				return _maxed;
			}
		}

		public bool Cooldown
		{
			get
			{
				return _cooldown;
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public void Configure(UpgradeSystem.Type ritualType)
		{
			_cooldownContainer.SetActive(false);
			_ritualType = ritualType;
			_locked = !CheatConsole.UnlockAllRituals && !DataManager.Instance.UnlockedUpgrades.Contains(_ritualType);
			_maxed = UpgradeSystem.IsUpgradeMaxed(_ritualType);
			_cooldown = UpgradeSystem.GetCoolDownNormalised(_ritualType) > 0f;
			_canAfford = UpgradeSystem.UserCanAffordUpgrade(_ritualType);
			_anchoredOrigin = _iconContainer.anchoredPosition;
			_button.onClick.AddListener(OnRitualItemClicked);
			MMButton button = _button;
			button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnSelected));
			MMButton button2 = _button;
			button2.OnConfirmDenied = (Action)Delegate.Combine(button2.OnConfirmDenied, new Action(Shake));
			_flashIcon.gameObject.SetActive(false);
			if (_alert != null)
			{
				_alert.Configure(ritualType);
			}
			if (!_locked)
			{
				_ritualImage.sprite = _iconMapping.GetImage(_ritualType);
				if (_maxed)
				{
					_costText.text = ScriptLocalization.UI_Generic.Max.Colour(StaticColors.RedColor);
				}
				else if (_cooldown)
				{
					_costText.text = ScriptLocalization.UI_Generic.Cooldown.Colour(StaticColors.RedColor);
					_cooldownContainer.SetActive(true);
					_cooldownFill.fillAmount = UpgradeSystem.GetCoolDownNormalised(_ritualType);
					_ritualImage.color = new Color(0f, 1f, 1f, 1f);
				}
				else
				{
					_costText.text = StructuresData.ItemCost.GetCostString(UpgradeSystem.GetCost(ritualType));
				}
			}
			else
			{
				SetAsLocked();
			}
			_button.Confirmable = !_locked && _canAfford && !_cooldown;
		}

		public void ForceIncognitoState()
		{
			if (_alert != null)
			{
				_alert.gameObject.SetActive(false);
			}
			_costText.text = "";
			_ritualImage.color = new Color(0f, 1f, 1f, 1f);
			_flashIcon.gameObject.SetActive(true);
			_flashIcon.color = new Color(0f, 0f, 0f, 0.5f);
		}

		public void AnimateIncognitoOut()
		{
			_ritualImage.DOKill();
			_ritualImage.DOColor(Color.white, 0.25f).SetUpdate(true);
			_flashIcon.DOKill();
			_flashIcon.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(delegate
			{
				if (_maxed)
				{
					_costText.text = ScriptLocalization.UI_Generic.Max.Colour(StaticColors.RedColor);
				}
				else if (_cooldown)
				{
					_costText.text = ScriptLocalization.UI_Generic.Cooldown.Colour(StaticColors.RedColor);
					_cooldownContainer.SetActive(true);
				}
				else
				{
					_costText.text = StructuresData.ItemCost.GetCostString(UpgradeSystem.GetCost(_ritualType));
				}
			});
		}

		public void ForceLockedState()
		{
			SetAsLocked();
			if (_alert != null)
			{
				_alert.gameObject.SetActive(false);
			}
		}

		public IEnumerator DoUnlock()
		{
			_iconContainer.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			Configure(_ritualType);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Unlocked"), 1f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), 0f);
			_ritualImage.color = new Color(1f, 1f, 1f, 1f);
			_flashIcon.gameObject.SetActive(true);
			if (_alert != null)
			{
				_alert.gameObject.SetActive(false);
			}
			UIManager.PlayAudio("event:/unlock_building/unlock");
			_iconContainer.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			_flashIcon.DOColor(new Color(1f, 1f, 1f, 0f), 0.25f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			yield return new WaitForSecondsRealtime(0.1f);
			if (_alert != null)
			{
				Vector3 one = Vector3.one;
				_alert.transform.localScale = Vector3.zero;
				_alert.transform.DOScale(one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
				_alert.gameObject.SetActive(true);
				yield return new WaitForSecondsRealtime(0.5f);
			}
		}

		public void SetMaxed()
		{
			_maxed = true;
			_costText.text = ScriptLocalization.UI_Generic.Max.Colour(StaticColors.RedColor);
		}

		private void OnRitualItemClicked()
		{
			if (!_locked && !_maxed && _canAfford && !_cooldown)
			{
				Action<UpgradeSystem.Type> onRitualItemSelected = OnRitualItemSelected;
				if (onRitualItemSelected != null)
				{
					onRitualItemSelected(_ritualType);
				}
			}
			else if (_maxed)
			{
				Shake();
			}
		}

		private void Shake()
		{
			_iconContainer.DOKill();
			_iconContainer.anchoredPosition = _anchoredOrigin;
			_iconContainer.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		private void SetAsLocked()
		{
			_costText.text = "";
			_animator.SetLayerWeight(_animator.GetLayerIndex("Unlocked"), 0f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), 1f);
		}

		private void OnSelected()
		{
			if (_alert != null)
			{
				_alert.TryRemoveAlert();
			}
		}
	}
}
