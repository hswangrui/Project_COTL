using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using src.UI.Overlays.MysticShopOverlay;
using src.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIMysticShopOverlayController : UIMenuBase
	{
		public Action<InventoryItem.ITEM_TYPE> OnRewardChosen;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Controllers")]
		[SerializeField]
		private RadiusController _radiusController;

		[SerializeField]
		private FlashController _flashController;

		[Header("Wheel")]
		[SerializeField]
		private RectTransform _container;

		[SerializeField]
		private RectTransform _subContainer;

		[SerializeField]
		private AnimationCurve _spinCurve;

		[SerializeField]
		private RectTransform _dial2;

		[SerializeField]
		private RectTransform _dial3;

		[SerializeField]
		private RectTransform _innerCircle1;

		[SerializeField]
		private RectTransform _innerCircle2;

		[Header("Selection Dial")]
		[SerializeField]
		private FlashController _chosenFlashController;

		[SerializeField]
		private MysticShopRewardOption _chosenOption;

		[SerializeField]
		private RectTransform _dial;

		[SerializeField]
		private GameObject _dialGraphic;

		[Header("Options")]
		[SerializeField]
		private MMRadialLayoutGroup _optionsLayoutGroup;

		[SerializeField]
		private RectTransform _optionsContainer;

		[SerializeField]
		private MysticShopRingInnerRenderer _ringRenderer;

		[Header("Flourishes")]
		[SerializeField]
		private MMRotatedLayoutGroup _flourishLayout;

		[SerializeField]
		private RectTransform _flourishContainer;

		[Header("BG")]
		[SerializeField]
		private Image _backgroundFX;

		private MysticShopRewardOption[] _rewardOptions;

		private MysticShopFlourishRenderer[] _flourishes;

		private WeightedCollection<InventoryItem.ITEM_TYPE> _rewards;

		private EventInstance _sfx;

		public override void Awake()
		{
			base.Awake();
			_chosenFlashController.enabled = false;
			_flashController.Flash = 1f;
			_radiusController.Expansion = 0f;
			_chosenOption.gameObject.SetActive(false);
			_rewardOptions = _optionsContainer.GetComponentsInChildren<MysticShopRewardOption>();
			_flourishes = _flourishContainer.GetComponentsInChildren<MysticShopFlourishRenderer>();
			if (!DataManager.Instance.MysticShopUsed)
			{
				_controlPrompts.HideAcceptButton();
			}
			_container.DOScale(Vector3.one * 1.1f, 7.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
				.SetUpdate(true);
		}

		public void Show(WeightedCollection<InventoryItem.ITEM_TYPE> rewards, bool instant = false)
		{
			_rewards = rewards;
			_flourishLayout.Offset = 360f / (float)rewards.Count / 2f;
			_ringRenderer.Segments = rewards.Count;
			for (int i = 0; i < _rewardOptions.Length; i++)
			{
				if (i > _rewards.Count - 1)
				{
					_rewardOptions[i].gameObject.SetActive(false);
					_flourishes[i].gameObject.SetActive(false);
					continue;
				}
				_rewardOptions[i].Configure(_rewards[i]);
				if (_rewardOptions.Length == 3)
				{
					_flourishes[i].Fill = 1f;
				}
				else if (_rewardOptions.Length == 4)
				{
					_flourishes[i].Fill = 0.75f;
				}
			}
			_sfx = AudioManager.Instance.PlayOneShotWithInstance("event:/mystic/mystic_wheel_spin");
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			StartCoroutine(DoSpin());
			_backgroundFX.color = new Color(1f, 1f, 1f, 0f);
			_backgroundFX.DOFade(1f, 1f);
		}

		private IEnumerator DoSpin()
		{
			yield return new WaitForSecondsRealtime(0.5f);
			int index = _rewards.GetRandomIndex();
			int num = UnityEngine.Random.Range(7, 8);
			float duration = 5f;
			float num2 = index * -(360 / _rewards.Count) + 360 * num;
			_dial.DORotate(new Vector3(0f, 0f, num2), duration, RotateMode.LocalAxisAdd).SetEase(_spinCurve).SetUpdate(true);
			_dial2.DORotate(new Vector3(0f, 0f, num2 / 2f), duration, RotateMode.LocalAxisAdd).SetEase(_spinCurve).SetUpdate(true);
			_dial3.DORotate(new Vector3(0f, 0f, num2 / 4f), duration, RotateMode.LocalAxisAdd).SetEase(_spinCurve).SetUpdate(true);
			_innerCircle1.DORotate(new Vector3(0f, 0f, (0f - num2) / 6f), duration, RotateMode.LocalAxisAdd).SetEase(_spinCurve).SetUpdate(true);
			_innerCircle2.DORotate(new Vector3(0f, 0f, (0f - num2) / 8f), duration, RotateMode.LocalAxisAdd).SetEase(_spinCurve).SetUpdate(true);
			while (duration > 0f)
			{
				duration -= Time.unscaledDeltaTime;
				if (InputManager.UI.GetAcceptButtonDown() && DataManager.Instance.MysticShopUsed)
				{
					_animator.Play("Shown");
					_dial.DOComplete();
					_dial2.DOComplete();
					_dial3.DOComplete();
					_innerCircle1.DOComplete();
					_innerCircle2.DOComplete();
					if (_sfx.isValid())
					{
						_sfx.stop(STOP_MODE.IMMEDIATE);
						_sfx.release();
					}
					break;
				}
				yield return null;
			}
			_controlPrompts.HideAcceptButton();
			UIManager.PlayAudio("event:/mystic/mystic_prize_select");
			yield return new WaitForSecondsRealtime(0.1f);
			_flashController.enabled = false;
			_rewardOptions[index].Choose();
			_dialGraphic.SetActive(false);
			_chosenOption.gameObject.SetActive(true);
			_chosenOption.Configure(_rewards[index]);
			_chosenOption.transform.DOPunchScale(Vector3.one * 0.15f, 2f, 3).SetEase(Ease.OutBounce).SetUpdate(true);
			_subContainer.transform.DOPunchScale(Vector3.one * 0.05f, 2f, 3).SetEase(Ease.OutBounce).SetUpdate(true);
			_chosenFlashController.enabled = true;
			_chosenFlashController.Flash = 1f;
			DOTween.To(() => _chosenFlashController.Flash, delegate(float x)
			{
				_chosenFlashController.Flash = x;
			}, 0f, 0.5f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(1f);
			yield return new WaitForSecondsRealtime(1.5f);
			_chosenFlashController.enabled = false;
			_flashController.enabled = true;
			Action<InventoryItem.ITEM_TYPE> onRewardChosen = OnRewardChosen;
			if (onRewardChosen != null)
			{
				onRewardChosen(_rewards[index]);
			}
			UIManager.PlayAudio("event:/mystic/mystic_wheel_shrink");
			_backgroundFX.DOFade(0f, 1f);
			Hide();
		}

		protected override void OnHideCompleted()
		{
			DataManager.Instance.MysticShopUsed = true;
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
