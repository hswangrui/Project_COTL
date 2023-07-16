using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Lamb.UI.FollowerSelect;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIDemonSummonMenuController : UIFollowerSelectMenuController
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass24_0
		{
			public DemonInfoCard currentCard;

			public UIDemonSummonMenuController _003C_003E4__this;

			public bool cancel;

			internal void _003CFocusFollowerCard_003Eg__OnHold_007C0(float progress)
			{
				float num = 1f + 0.25f * progress;
				currentCard.RectTransform.localScale = new Vector3(num, num, num);
				currentCard.RectTransform.localPosition = UnityEngine.Random.insideUnitCircle * progress * _003C_003E4__this._uiHoldInteraction.HoldTime * 2f;
				MMVibrate.RumbleContinuous(progress * 0.2f, progress * 0.2f);
				if (currentCard.RedOutline.gameObject.activeSelf != progress > 0f)
				{
					currentCard.RedOutline.gameObject.SetActive(progress > 0f);
				}
				currentCard.RedOutline.localScale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(1.2f, 1.2f, 1.2f), progress);
			}

			internal void _003CFocusFollowerCard_003Eg__OnCancel_007C1()
			{
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = currentCard.RedOutline.DOScale(Vector3.one, 0.25f).SetUpdate(true);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)delegate
				{
					currentCard.RedOutline.gameObject.SetActive(false);
				});
				currentCard.RectTransform.DOScale(Vector3.one, 0.25f).SetUpdate(true);
				cancel = true;
				MMVibrate.StopRumble();
			}

			internal void _003CFocusFollowerCard_003Eb__2()
			{
				currentCard.RedOutline.gameObject.SetActive(false);
			}

			internal void _003CFocusFollowerCard_003Eb__3()
			{
				currentCard.RectTransform.SetParent(_003C_003E4__this._cardContainer, true);
			}
		}

		private string kSelectedFollowerAnimationState = "Selected";

		private string kCancelSelectionAnimationState = "Cancelled";

		private string kConfirmedSelectionAnimationState = "Confirmed";

		[Header("Info Card")]
		[SerializeField]
		private DemonInfoCardController _demonMenuInfoCardController;

		[SerializeField]
		private RectTransform _rootContainer;

		[SerializeField]
		private RectTransform _cardContainer;

		[Header("Limits")]
		[SerializeField]
		private RectTransform _shootyLimitContainer;

		[SerializeField]
		private TextMeshProUGUI _shootyLimit;

		[SerializeField]
		private RectTransform _chompLimitContainer;

		[SerializeField]
		private TextMeshProUGUI _chompLimit;

		[SerializeField]
		private RectTransform _arrowsLimitContainer;

		[SerializeField]
		private TextMeshProUGUI _arrowsLimit;

		[SerializeField]
		private RectTransform _collectorsLimitContainer;

		[SerializeField]
		private TextMeshProUGUI _collectorsLimit;

		[SerializeField]
		private RectTransform _exploderLimitContainer;

		[SerializeField]
		private TextMeshProUGUI _exploderLimit;

		[SerializeField]
		private RectTransform _spiritLimitContainer;

		[SerializeField]
		private TextMeshProUGUI _spiritLimit;

		[Header("Misc")]
		[SerializeField]
		private UIHoldInteraction _uiHoldInteraction;

		private List<int> _demonIDS = new List<int>();

		//public override TwitchVoting.VotingType VotingType
		//{
		//	get
		//	{
		//		return TwitchVoting.VotingType.DEMON;
		//	}
		//}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			foreach (FollowerInformationBox followerInfoBox in _followerInfoBoxes)
			{
				int demonType = DemonModel.GetDemonType(followerInfoBox.FollowerInfo);
				followerInfoBox.Button.Confirmable = !_demonIDS.Contains(demonType);
				MMButton button = followerInfoBox.Button;
				button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, (Action)delegate
				{
					OnConfirmationDenied(demonType);
				});
			}
		}

		protected override void FollowerSelected(FollowerInfo followerInfo)
		{
			_demonMenuInfoCardController.enabled = false;
			OverrideDefaultOnce(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			StartCoroutine(FocusFollowerCard(followerInfo));
		}

		private IEnumerator FocusFollowerCard(FollowerInfo followerInfo)
		{
			_003C_003Ec__DisplayClass24_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass24_0();
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			_controlPrompts.HideAcceptButton();
			_uiHoldInteraction.Reset();
			CS_0024_003C_003E8__locals0.currentCard = _demonMenuInfoCardController.CurrentCard;
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.SetParent(_rootContainer, true);
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack).SetUpdate(true);
			_animator.Play(kSelectedFollowerAnimationState);
			yield return new WaitForSecondsRealtime(1f);
			CS_0024_003C_003E8__locals0.cancel = false;
			yield return _uiHoldInteraction.DoHoldInteraction(CS_0024_003C_003E8__locals0._003CFocusFollowerCard_003Eg__OnHold_007C0, CS_0024_003C_003E8__locals0._003CFocusFollowerCard_003Eg__OnCancel_007C1);
			MMVibrate.StopRumble();
			if (CS_0024_003C_003E8__locals0.cancel)
			{
				Vector2 vector = _rootContainer.InverseTransformPoint(_cardContainer.TransformPoint(Vector3.zero));
				CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOLocalMove(vector, 1f).SetEase(Ease.InOutBack).SetUpdate(true)
					.OnComplete(delegate
					{
						CS_0024_003C_003E8__locals0.currentCard.RectTransform.SetParent(CS_0024_003C_003E8__locals0._003C_003E4__this._cardContainer, true);
					});
				_animator.Play(kCancelSelectionAnimationState);
				yield return new WaitForSecondsRealtime(1f);
				_controlPrompts.ShowAcceptButton();
				SetActiveStateForMenu(true);
				_demonMenuInfoCardController.enabled = true;
			}
			else
			{
				_controlPrompts.HideCancelButton();
				CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return _animator.YieldForAnimation(kConfirmedSelectionAnimationState);
				Action<FollowerInfo> onFollowerSelected = OnFollowerSelected;
				if (onFollowerSelected != null)
				{
					onFollowerSelected(followerInfo);
				}
				Hide(true);
			}
		}

		private void OnConfirmationDenied(int demonType)
		{
			switch (demonType)
			{
			case 0:
				ShakeLimit(_shootyLimitContainer);
				break;
			case 1:
				ShakeLimit(_chompLimitContainer);
				break;
			case 2:
				ShakeLimit(_arrowsLimitContainer);
				break;
			case 3:
				ShakeLimit(_collectorsLimitContainer);
				break;
			case 4:
				ShakeLimit(_exploderLimitContainer);
				break;
			case 5:
				ShakeLimit(_spiritLimitContainer);
				break;
			}
		}

		private void ShakeLimit(RectTransform container)
		{
			container.transform.DOKill();
			container.anchoredPosition = Vector2.zero;
			container.transform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		public void UpdateDemonCounts(List<int> followerIDs)
		{
			foreach (int followerID in followerIDs)
			{
				_demonIDS.Add(DemonModel.GetDemonType(followerID));
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			using (List<int>.Enumerator enumerator = _demonIDS.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case 0:
						num++;
						break;
					case 1:
						num2++;
						break;
					case 2:
						num3++;
						break;
					case 3:
						num4++;
						break;
					case 4:
						num5++;
						break;
					case 5:
						num6++;
						break;
					}
				}
			}
			_shootyLimit.text = string.Format("{0}/{1}", num, 1);
			_chompLimit.text = string.Format("{0}/{1}", num2, 1);
			_arrowsLimit.text = string.Format("{0}/{1}", num3, 1);
			_collectorsLimit.text = string.Format("{0}/{1}", num4, 1);
			_exploderLimit.text = string.Format("{0}/{1}", num5, 1);
			_spiritLimit.text = string.Format("{0}/{1}", num6, 1);
		}
	}
}
