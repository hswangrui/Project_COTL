using System;
using System.Collections.Generic;
using src.UI.Overlays.TwitchFollowerVotingOverlay;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI.FollowerSelect
{
	//public abstract class UIFollowerSelectBase<T> : UIMenuBase, ITwitchVotingProvider where T : FollowerSelectItem
	public abstract class UIFollowerSelectBase<T> : UIMenuBase where T : FollowerSelectItem
	{
		public Action<FollowerInfo> OnFollowerSelected;

		[Header("Follower Select")]
		[SerializeField]
		protected RectTransform _contentContainer;

		[SerializeField]
		private GameObject _noFollowerText;

		[SerializeField]
		protected MMScrollRect _scrollRect;

		[SerializeField]
		protected UIMenuControlPrompts _controlPrompts;

		private List<FollowerInfo> _followerInfo = new List<FollowerInfo>();

		protected List<T> _followerInfoBoxes = new List<T>();

		protected bool _didCancel;

		protected bool _hideOnSelection;

		protected bool _cancellable;

		protected bool _hasSelection;

		public List<FollowerInfo> FollowerInfos
		{
			get
			{
				return _followerInfo;
			}
		}

		public List<T> FollowerInfoBoxes
		{
			get
			{
				return _followerInfoBoxes;
			}
		}

		//public virtual TwitchVoting.VotingType VotingType { get; set; }

		public virtual bool AllowsVoting { get; set; } = true;


		public void Show(List<int> followerIDS, List<int> blackList = null, bool instant = false, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			List<FollowerInfo> list = new List<FollowerInfo>();
			foreach (int followerID in followerIDS)
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(followerID);
				list.Add(infoByID);
			}
			List<FollowerInfo> list2 = new List<FollowerInfo>();
			if (blackList != null)
			{
				foreach (int black in blackList)
				{
					FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(black);
					list2.Add(infoByID2);
				}
			}
			Show(list, list2, instant, hideOnSelection, cancellable);
		}

		public void Show(List<FollowerBrain> followerBrains, List<FollowerBrain> blackList = null, bool instant = false, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			List<FollowerInfo> list = new List<FollowerInfo>();
			foreach (FollowerBrain followerBrain in followerBrains)
			{
				list.Add(followerBrain._directInfoAccess);
			}
			List<FollowerInfo> list2 = new List<FollowerInfo>();
			if (blackList != null)
			{
				foreach (FollowerBrain black in blackList)
				{
					list2.Add(black._directInfoAccess);
				}
			}
			Show(list, list2, instant, hideOnSelection, cancellable);
		}

		public void Show(List<Follower> followers, List<Follower> blackList = null, bool instant = false, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			List<FollowerInfo> list = new List<FollowerInfo>();
			foreach (Follower follower in followers)
			{
				list.Add(follower.Brain._directInfoAccess);
			}
			List<FollowerInfo> list2 = new List<FollowerInfo>();
			if (blackList != null)
			{
				foreach (Follower black in blackList)
				{
					list2.Add(black.Brain._directInfoAccess);
				}
			}
			Show(list, list2, instant, hideOnSelection, cancellable);
		}

		public void Show(List<SimFollower> simFollowers, List<SimFollower> blackList = null, bool instant = false, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			List<FollowerInfo> list = new List<FollowerInfo>();
			foreach (SimFollower simFollower in simFollowers)
			{
				list.Add(simFollower.Brain._directInfoAccess);
			}
			List<FollowerInfo> list2 = new List<FollowerInfo>();
			if (blackList != null)
			{
				foreach (SimFollower black in blackList)
				{
					list2.Add(black.Brain._directInfoAccess);
				}
			}
			Show(list, list2, instant, hideOnSelection, cancellable);
		}

		public void Show(List<FollowerInfo> followerInfo, List<FollowerInfo> blackList = null, bool instant = false, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			if (blackList == null)
			{
				blackList = new List<FollowerInfo>();
			}
			foreach (FollowerInfo item in followerInfo)
			{
				if (!blackList.Contains(item))
				{
					_followerInfo.Add(item);
				}
			}
			_hideOnSelection = hideOnSelection;
			_cancellable = cancellable;
			_hasSelection = hasSelection;
			Show(instant);
		}

		public void UpdateBlacklist(List<FollowerInfo> blackList = null)
		{
			int num = 0;
			for (int i = 0; i < blackList.Count; i++)
			{
				for (int num2 = _followerInfoBoxes.Count - 1; num2 >= 0; num2--)
				{
					if (_followerInfoBoxes[num2].FollowerInfo == blackList[i])
					{
						num = num2;
						_followerInfoBoxes[num2].Recycle();
						_followerInfoBoxes.RemoveAt(num2);
						break;
					}
				}
			}
			if (_followerInfoBoxes.Count > 0)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_followerInfoBoxes[(_followerInfoBoxes.Count > num) ? num : 0].Button);
			}
			else
			{
				_noFollowerText.SetActive(true);
			}
		}

		protected override void OnShowStarted()
		{
			if (!_hasSelection)
			{
				_controlPrompts.HideAcceptButton();
			}
			_scrollRect.enabled = false;
			_noFollowerText.SetActive(_followerInfo.Count == 0);
			if (!_cancellable && _controlPrompts != null)
			{
				_controlPrompts.HideCancelButton();
			}
			if (_followerInfo.Count > 0)
			{
				foreach (FollowerInfo item in _followerInfo)
				{
					T val = PrefabTemplate().Spawn(_contentContainer);
					val.transform.localScale = Vector3.one;
					val.Configure(item);
					val.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(val.OnFollowerSelected, new Action<FollowerInfo>(FollowerSelected));
					val.Button.SetInteractionState(true);
					val.Button._vibrateOnConfirm = false;
					_followerInfoBoxes.Add(val);
				}
				OverrideDefault(_followerInfoBoxes[0].Button);
				//if (TwitchAuthentication.IsAuthenticated && !TwitchVoting.Deactivated && AllowsVoting)
				//{
				//	TwitchInformationBox twitchInformationBox = MonoSingleton<UIManager>.Instance.TwitchInformationBox.Spawn(_contentContainer);
				//	twitchInformationBox.transform.localScale = Vector3.one;
				//	twitchInformationBox.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(twitchInformationBox.OnFollowerSelected, new Action<FollowerInfo>(FollowerSelected));
				//	twitchInformationBox.Button.SetInteractionState(true);
				//	twitchInformationBox.Button._vibrateOnConfirm = false;
				//	twitchInformationBox.transform.SetAsFirstSibling();
				//	twitchInformationBox.Configure(GetComponentInParent<UIFollowerSelectMenuController>());
				//}
				ActivateNavigation();
			}
			_scrollRect.enabled = true;
			_scrollRect.normalizedPosition = Vector2.one;
		}

		protected abstract T PrefabTemplate();

		protected virtual void FollowerSelected(FollowerInfo followerInfo)
		{
			for (int num = _followerInfoBoxes.Count - 1; num >= 0; num--)
			{
				if (_followerInfoBoxes[num].FollowerInfo.ID == followerInfo.ID)
				{
					Action<FollowerInfo> onFollowerSelected = OnFollowerSelected;
					if (onFollowerSelected != null)
					{
						onFollowerSelected(followerInfo);
					}
					if (_hideOnSelection)
					{
						Hide();
					}
				}
			}
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			foreach (T followerInfoBox in _followerInfoBoxes)
			{
				followerInfoBox.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Remove(followerInfoBox.OnFollowerSelected, new Action<FollowerInfo>(FollowerSelected));
				followerInfoBox.Recycle();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable && _cancellable)
			{
				UIManager.PlayAudio("event:/ui/go_back");
				_didCancel = true;
				Hide();
			}
		}

		private void Update()
		{
			Input.GetKeyDown(KeyCode.Space);
		}

		public List<FollowerInfo> ProvideInfo()
		{
			return _followerInfo;
		}

		public virtual void FinalizeVote(FollowerInfo followerInfo)
		{
			Action<FollowerInfo> onFollowerSelected = OnFollowerSelected;
			if (onFollowerSelected != null)
			{
				onFollowerSelected(followerInfo);
			}
			Hide();
		}
	}
}
