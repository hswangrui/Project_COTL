using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.UINavigator;
using UnityEngine;

namespace src.UI.Menus
{
	public class UIPrisonMenuController : UIFollowerSelectMenuController
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass10_0
		{
			public PrisonInfoCard currentCard;

			public UIPrisonMenuController _003C_003E4__this;

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
		private PrisonMenuInfoCardController _prisonMenuInfoCardController;

		[SerializeField]
		private RectTransform _rootContainer;

		[SerializeField]
		private RectTransform _cardContainer;

		[Header("Misc")]
		[SerializeField]
		private UIHoldInteraction _uiHoldInteraction;

		//public override TwitchVoting.VotingType VotingType
		//{
		//	get
		//	{
		//		return TwitchVoting.VotingType.PRISON;
		//	}
		//}

		protected override void FollowerSelected(FollowerInfo followerInfo)
		{
			_prisonMenuInfoCardController.enabled = false;
			OverrideDefaultOnce(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			StartCoroutine(FocusFollowerCard(followerInfo));
		}

		private IEnumerator FocusFollowerCard(FollowerInfo followerInfo)
		{
			_003C_003Ec__DisplayClass10_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass10_0();
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			_controlPrompts.HideAcceptButton();
			_uiHoldInteraction.Reset();
			CS_0024_003C_003E8__locals0.currentCard = _prisonMenuInfoCardController.CurrentCard;
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.SetParent(_rootContainer, true);
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack).SetUpdate(true);
			_animator.Play(kSelectedFollowerAnimationState);
			yield return new WaitForSecondsRealtime(1f);
			CS_0024_003C_003E8__locals0.currentCard.FollowerSpine.AnimationState.SetAnimation(0, "Reactions/react-worried1", true);
			CS_0024_003C_003E8__locals0.cancel = false;
			yield return _uiHoldInteraction.DoHoldInteraction(CS_0024_003C_003E8__locals0._003CFocusFollowerCard_003Eg__OnHold_007C0, CS_0024_003C_003E8__locals0._003CFocusFollowerCard_003Eg__OnCancel_007C1);
			MMVibrate.StopRumble();
			if (CS_0024_003C_003E8__locals0.cancel)
			{
				CS_0024_003C_003E8__locals0.currentCard.FollowerSpine.AnimationState.SetAnimation(0, "idle", true);
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
				_prisonMenuInfoCardController.enabled = true;
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
	}
}
