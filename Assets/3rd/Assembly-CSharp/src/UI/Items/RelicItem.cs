using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Items
{
	public class RelicItem : MonoBehaviour
	{
		private const string kUnlockedLayer = "Unlocked";

		private const string kLockedLayer = "Locked";

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private GameObject _lockedContainer;

		[SerializeField]
		private MMSelectable _selectable;

		[SerializeField]
		private RelicAlert _alert;

		[SerializeField]
		private Image _flashIcon;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private RectTransform _container;

		public RelicData Data { get; private set; }

		public bool Locked { get; private set; }

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}

		public RelicAlert Alert
		{
			get
			{
				return _alert;
			}
		}

		private void Awake()
		{
			MMSelectable selectable = _selectable;
			selectable.OnSelected = (Action)Delegate.Combine(selectable.OnSelected, new Action(OnSelected));
		}

		public void Configure(RelicData data)
		{
			Data = data;
			_alert.Configure(data.RelicType);
			if (Data != null)
			{
				_icon.sprite = Data.UISprite;
				if (!DataManager.Instance.PlayerFoundRelics.Contains(data.RelicType))
				{
					Locked = true;
					SetAsLocked();
				}
			}
		}

		private void SetAsLocked()
		{
			_animator.SetLayerWeight(_animator.GetLayerIndex("Unlocked"), 0f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), 1f);
		}

		public void ForceIncognitoState()
		{
			_alert.gameObject.SetActive(false);
			_icon.color = new Color(0f, 1f, 1f, 1f);
			_flashIcon.gameObject.SetActive(true);
			_flashIcon.color = new Color(0f, 0f, 0f, 0.5f);
		}

		public void ForceLockedState()
		{
			SetAsLocked();
			_alert.gameObject.SetActive(false);
		}

		public IEnumerator DoUnlock()
		{
			yield return Flash();
			yield return ShowAlert();
		}

		public IEnumerator Flash()
		{
			_container.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			Configure(Data);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Unlocked"), 1f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), 0f);
			_icon.color = new Color(1f, 1f, 1f, 1f);
			_flashIcon.gameObject.SetActive(true);
			_alert.gameObject.SetActive(false);
			UIManager.PlayAudio("event:/unlock_building/unlock");
			_container.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			_flashIcon.DOColor(new Color(1f, 1f, 1f, 0f), 0.25f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			yield return new WaitForSecondsRealtime(0.1f);
			Locked = false;
		}

		public IEnumerator ShowAlert()
		{
			Vector3 one = Vector3.one;
			_alert.transform.localScale = Vector3.zero;
			_alert.transform.DOScale(one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
			_alert.gameObject.SetActive(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}

		private void OnSelected()
		{
			_alert.TryRemoveAlert();
		}
	}
}
