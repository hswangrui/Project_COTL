using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class FleeceItemBuyable : MonoBehaviour
	{
		private const string kUnlockedLayer = "Unlocked";

		private const string kLockedLayer = "Locked";

		public Action<int> OnFleeceChosen;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _shakeContainer;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private TextMeshProUGUI _costText;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private FleeceIconMapping _fleeceIconMapping;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private Image _outline;

		[SerializeField]
		private GameObject _alert;

		[SerializeField]
		private Image _flashIcon;

		[SerializeField]
		private int _forcedFleeceIndex = -1;

		public bool postGameFleece;

		private int _fleeceIndex;

		private bool _unlocked;

		private Vector2 _origin;

		private StructuresData.ItemCost _cost;

		public int ForcedFleeceIndex
		{
			get
			{
				return _forcedFleeceIndex;
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public int Fleece
		{
			get
			{
				return _fleeceIndex;
			}
		}

		public StructuresData.ItemCost Cost
		{
			get
			{
				return _cost;
			}
		}

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public bool Unlocked
		{
			get
			{
				return _unlocked;
			}
		}

		public void Configure(int index)
		{
			if (postGameFleece && !DataManager.Instance.PostGameFleecesOnboarded)
			{
				base.gameObject.SetActive(false);
				return;
			}
			if (_forcedFleeceIndex != -1 && !DataManager.Instance.UnlockedFleeces.Contains(_forcedFleeceIndex))
			{
				index = _forcedFleeceIndex;
				base.gameObject.SetActive(false);
				return;
			}
			_fleeceIndex = index;
			_outline.color = StaticColors.GreenColor;
			_origin = _shakeContainer.anchoredPosition;
			_cost = new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.TALISMAN, 1);
			_unlocked = DataManager.Instance.UnlockedFleeces.Contains(_fleeceIndex);
			_button.Confirmable = (_cost.CanAfford() && !_unlocked) || _unlocked;
			UpdateState();
			_fleeceIconMapping.GetImage(_fleeceIndex, _icon);
			_fleeceIconMapping.GetImage(_fleeceIndex, _outline);
			_button.onClick.AddListener(OnButtonClicked);
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(Shake));
			_flashIcon.gameObject.SetActive(false);
		}

		public void UpdateState()
		{
			if (_cost == null)
			{
				return;
			}
			_alert.SetActive(_cost.CanAfford() && !_unlocked);
			if (!_unlocked)
			{
				_outline.gameObject.SetActive(false);
				_costText.text = StructuresData.ItemCost.GetCostString(new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.TALISMAN, 1));
				if (_cost.CanAfford())
				{
					_icon.color = new Color(1f, 1f, 1f, 1f);
				}
				else
				{
					_icon.color = new Color(0f, 1f, 1f, 1f);
				}
			}
			else
			{
				_icon.color = new Color(1f, 1f, 1f, 1f);
				_costText.text = "";
				_outline.gameObject.SetActive(DataManager.Instance.PlayerFleece == _fleeceIndex);
			}
		}

		private void OnButtonClicked()
		{
			Action<int> onFleeceChosen = OnFleeceChosen;
			if (onFleeceChosen != null)
			{
				onFleeceChosen(_fleeceIndex);
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
					_costText.text = StructuresData.ItemCost.GetCostString(new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.TALISMAN, 1));
					if (_cost.CanAfford())
					{
						_icon.color = new Color(1f, 1f, 1f, 1f);
					}
					else
					{
						_icon.color = new Color(0f, 1f, 1f, 1f);
					}
				}
				else
				{
					_costText.text = "";
				}
			});
		}

		public void Shake()
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

		public void PrepareForUnlock()
		{
			_costText.text = string.Empty;
			_shakeContainer.gameObject.SetActive(false);
		}

		public IEnumerator DoUnlock()
		{
			_shakeContainer.gameObject.SetActive(true);
			_shakeContainer.localScale = Vector3.zero;
			_icon.color = new Color(1f, 1f, 1f, 1f);
			_flashIcon.gameObject.SetActive(true);
			_flashIcon.color = new Color(1f, 1f, 1f, 1f);
			if (_alert != null)
			{
				_alert.gameObject.SetActive(false);
			}
			UIManager.PlayAudio("event:/unlock_building/unlock");
			_shakeContainer.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
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
			yield return null;
		}
	}
}
