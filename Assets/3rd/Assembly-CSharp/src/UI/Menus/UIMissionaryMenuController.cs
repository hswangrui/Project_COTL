using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Lamb.UI.FollowerSelect;
using src.UINavigator;
using UnityEngine;

namespace src.UI.Menus
{
	public class UIMissionaryMenuController : UIFollowerSelectMenuController
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass15_0
		{
			public bool cancel;

			public InventoryItem.ITEM_TYPE chosenItem;

			public MissionInfoCard currentCard;

			public UIMissionaryMenuController _003C_003E4__this;

			internal void _003CFocusFollowerCard_003Eg__Cancel_007C0()
			{
				cancel = true;
			}

			internal void _003CFocusFollowerCard_003Eg__MissionChosen_007C1(InventoryItem.ITEM_TYPE itemType)
			{
				chosenItem = itemType;
			}

			internal void _003CFocusFollowerCard_003Eb__2()
			{
				currentCard.RectTransform.SetParent(_003C_003E4__this._cardContainer, true);
			}
		}

		public Action<FollowerInfo, InventoryItem.ITEM_TYPE> OnMissionaryChosen;

		private string kSelectedFollowerAnimationState = "Selected";

		private string kCancelSelectionAnimationState = "Cancelled";

		private string kConfirmedSelectionAnimationState = "Confirmed";

		[Header("Info Card")]
		[SerializeField]
		private MissionInfoCardController _missionInfoCardController;

		[SerializeField]
		private RectTransform _rootContainer;

		[SerializeField]
		private RectTransform _cardContainer;

		private bool _choosingFollower;

		private bool _cannotCancel;

		//public override TwitchVoting.VotingType VotingType
		//{
		//	get
		//	{
		//		return TwitchVoting.VotingType.MISSIONARY;
		//	}
		//}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			SetActiveStateForMenu(_missionInfoCardController.gameObject, false);
		}

		public override void FinalizeVote(FollowerInfo followerInfo)
		{
			_cannotCancel = true;
			_controlPrompts.HideCancelButton();
			StartCoroutine(DeferredFinalize(followerInfo));
		}

		private IEnumerator DeferredFinalize(FollowerInfo followerInfo)
		{
			yield return null;
			_missionInfoCardController.ShowCardWithParam(followerInfo);
			_missionInfoCardController.enabled = false;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			StartCoroutine(FocusFollowerCard(followerInfo));
			_choosingFollower = true;
		}

		protected override void FollowerSelected(FollowerInfo followerInfo)
		{
			_missionInfoCardController.enabled = false;
			OverrideDefaultOnce(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			StartCoroutine(FocusFollowerCard(followerInfo));
			_choosingFollower = true;
		}

		private IEnumerator FocusFollowerCard(FollowerInfo followerInfo)
		{
			_003C_003Ec__DisplayClass15_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass15_0();
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			_controlPrompts.HideAcceptButton();
			_missionInfoCardController.enabled = false;
			_animator.Play(kSelectedFollowerAnimationState);
			CS_0024_003C_003E8__locals0.currentCard = _missionInfoCardController.CurrentCard;
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.SetParent(_rootContainer, true);
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOKill();
			CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack).SetUpdate(true);
			_animator.Play(kSelectedFollowerAnimationState);
			yield return new WaitForSecondsRealtime(1f);
			_controlPrompts.ShowAcceptButton();
			MissionInfoCard currentCard = CS_0024_003C_003E8__locals0.currentCard;
			currentCard.OnMissionSelected = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(currentCard.OnMissionSelected, new Action<InventoryItem.ITEM_TYPE>(CS_0024_003C_003E8__locals0._003CFocusFollowerCard_003Eg__MissionChosen_007C1));
			SetActiveStateForMenu(CS_0024_003C_003E8__locals0.currentCard.gameObject, true);
			_canvasGroup.interactable = true;
			MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(CS_0024_003C_003E8__locals0.currentCard.FirstAvailableButton());
			CS_0024_003C_003E8__locals0.chosenItem = InventoryItem.ITEM_TYPE.NONE;
			CS_0024_003C_003E8__locals0.cancel = false;
			while (CS_0024_003C_003E8__locals0.chosenItem == InventoryItem.ITEM_TYPE.NONE && !CS_0024_003C_003E8__locals0.cancel)
			{
				if (InputManager.UI.GetCancelButtonDown() && !_cannotCancel)
				{
					CS_0024_003C_003E8__locals0._003CFocusFollowerCard_003Eg__Cancel_007C0();
				}
				yield return null;
			}
			SetActiveStateForMenu(CS_0024_003C_003E8__locals0.currentCard.gameObject, false);
			_canvasGroup.interactable = false;
			if (CS_0024_003C_003E8__locals0.cancel)
			{
				_controlPrompts.HideAcceptButton();
				Debug.Log("Run cancel routine".Colour(Color.yellow));
				CS_0024_003C_003E8__locals0.currentCard.FollowerSpine.AnimationState.SetAnimation(0, "idle", true);
				Vector2 vector = _rootContainer.InverseTransformPoint(_cardContainer.TransformPoint(Vector3.zero));
				CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOKill();
				CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOLocalMove(vector, 1f).SetEase(Ease.InOutBack).SetUpdate(true)
					.OnComplete(delegate
					{
						CS_0024_003C_003E8__locals0.currentCard.RectTransform.SetParent(CS_0024_003C_003E8__locals0._003C_003E4__this._cardContainer, true);
					});
				_animator.Play(kCancelSelectionAnimationState);
				yield return new WaitForSecondsRealtime(1f);
				_controlPrompts.ShowAcceptButton();
				_canvasGroup.interactable = true;
				SetActiveStateForMenu(_scrollRect.gameObject, true);
				ActivateNavigation();
				_missionInfoCardController.enabled = true;
				_choosingFollower = false;
			}
			else
			{
				Debug.Log("Run hide routine".Colour(Color.red));
				_controlPrompts.HideAcceptButton();
				_controlPrompts.HideCancelButton();
				CS_0024_003C_003E8__locals0.currentCard.RectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return _animator.YieldForAnimation(kConfirmedSelectionAnimationState);
				Action<FollowerInfo, InventoryItem.ITEM_TYPE> onMissionaryChosen = OnMissionaryChosen;
				if (onMissionaryChosen != null)
				{
					onMissionaryChosen(followerInfo, CS_0024_003C_003E8__locals0.chosenItem);
				}
				Hide(true);
			}
		}

		public override void OnCancelButtonInput()
		{
			if (!_choosingFollower)
			{
				base.OnCancelButtonInput();
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/ui/close_menu");
			}
		}
	}
}
