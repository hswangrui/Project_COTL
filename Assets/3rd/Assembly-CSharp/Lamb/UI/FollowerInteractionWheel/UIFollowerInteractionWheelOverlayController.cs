using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using I2.Loc;
using Spine.Unity.Examples;
using TMPro;
using UnityEngine;

namespace Lamb.UI.FollowerInteractionWheel
{
	public class UIFollowerInteractionWheelOverlayController : UIRadialMenuBase<UIFollowerWheelInteractionItem, FollowerCommands[]>
	{
		private const string kNextCategory_In_Animation = "NextCategory_In";

		private const string kNextCategory_Out_Animation = "NextCategory_Out";

		private const string kPrevCategory_In_Animation = "PrevCategory_In";

		private const string kPrevCategory_Out_Animation = "PrevCategory_Out";

		private Follower _follower;

		private bool _cancellable;

		private List<CommandItem> _rootCommandItems;

		private Stack<List<CommandItem>> _commandStack = new Stack<List<CommandItem>>();

		private Stack<FollowerCommands> _commandHistory = new Stack<FollowerCommands>();

		public EventInstance _loopInstance;

		private FollowerSpineEventListener listener;

		public TextMeshProUGUI followerName;

		protected override bool SelectOnHighlight
		{
			get
			{
				return false;
			}
		}

		public void Show(Follower follower, List<CommandItem> commandItems, bool instant = false, bool cancellable = true)
		{
			Show(instant);
			_cancellable = cancellable;
			_follower = follower;
			_rootCommandItems = commandItems;
			string text = ((!string.IsNullOrEmpty(follower.Brain.Info.ViewerID)) ? (" <sprite name=\"icon_TwitchIcon\"> <color=#" + ColorUtility.ToHtmlStringRGB(StaticColors.TwitchPurple) + ">") : " <color=yellow>");
			followerName.text = text + follower.Brain.Info.Name + "</color>" + ((follower.Brain.Info.XPLevel > 1) ? (" " + ScriptLocalization.Interactions.Level + " " + follower.Brain.Info.XPLevel.ToNumeral()) : "") + (follower.Brain.Info.MarriedToLeader ? " <sprite name=\"icon_Married\">" : "");
			if (!_cancellable)
			{
				_controlPrompts.HideCancelButton();
			}
			if (listener != null)
			{
				listener.PlayFollowerVOLoop("event:/dialogue/followers/general_acknowledge");
			}
			else
			{
				Debug.Log("Listener not found");
			}
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			if (listener != null)
			{
				listener.StopFollowerVOLoop();
			}
		}

		private void OnDisable()
		{
			if (listener != null)
			{
				listener.StopFollowerVOLoop();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (listener != null)
			{
				listener.StopFollowerVOLoop();
			}
		}

		protected override void OnShowStarted()
		{
			ConfigureItems(_rootCommandItems);
		}

		private void ConfigureItems(List<CommandItem> commandItems)
		{
			if (commandItems.Count > 8)
			{
				commandItems.Sort((CommandItem p1, CommandItem p2) => p2.IsAvailable(_follower).CompareTo(p1.IsAvailable(_follower)));
				List<FollowerCommandItems.NextPageCommandItem> list = new List<FollowerCommandItems.NextPageCommandItem>();
				FollowerCommandItems.NextPageCommandItem nextPageCommandItem = FollowerCommandItems.NextPage();
				nextPageCommandItem.PageNumber = 1;
				list.Add(nextPageCommandItem);
				commandItems.Insert(4, nextPageCommandItem);
				int num = 0;
				for (int num2 = commandItems.Count - 1; num2 >= 8; num2--)
				{
					CommandItem item = commandItems[num2];
					commandItems.RemoveAt(num2);
					nextPageCommandItem.SubCommands.Insert(0, item);
					num++;
					if (num >= 7 && num2 > 8)
					{
						FollowerCommandItems.NextPageCommandItem nextPageCommandItem2 = FollowerCommandItems.NextPage();
						list.Add(nextPageCommandItem2);
						nextPageCommandItem.SubCommands.Insert(4, nextPageCommandItem2);
						nextPageCommandItem = nextPageCommandItem2;
						nextPageCommandItem.PageNumber = list.Count;
						num = 0;
					}
				}
				foreach (FollowerCommandItems.NextPageCommandItem item2 in list)
				{
					item2.TotalPageNumbers = list.Count + 1;
				}
			}
			for (int i = 0; i < _wheelItems.Count; i++)
			{
				if (i > commandItems.Count - 1)
				{
					_wheelItems[i].Configure(null, null);
				}
				else
				{
					_wheelItems[i].Configure(_follower, commandItems[i]);
				}
			}
		}

		private IEnumerator NextCategory(List<CommandItem> nextItems)
		{
			yield return _animator.YieldForAnimation("NextCategory_Out");
			ConfigureItems(nextItems);
			_controlPrompts.ShowCancelButton();
			yield return _animator.YieldForAnimation("NextCategory_In");
			StartCoroutine(DoWheelLoop());
		}

		private IEnumerator PrevCategory(List<CommandItem> prevItems)
		{
			yield return _animator.YieldForAnimation("PrevCategory_Out");
			ConfigureItems(prevItems);
			yield return _animator.YieldForAnimation("PrevCategory_In");
			StartCoroutine(DoWheelLoop());
		}

		protected override void OnChoiceFinalized()
		{
		}

		protected override void MakeChoice(UIFollowerWheelInteractionItem item)
		{
			if (item.CommandItem.SubCommands != null && item.CommandItem.SubCommands.Count > 0 && item.CommandItem.IsAvailable(null))
			{
				_commandStack.Push(_rootCommandItems);
				_commandHistory.Push(item.CommandItem.Command);
				_rootCommandItems = item.CommandItem.SubCommands;
				StartCoroutine(NextCategory(_rootCommandItems));
				return;
			}
			if (item.FollowerCommand == FollowerCommands.AreYouSureNo)
			{
				OnCancelButtonInput();
				return;
			}
			_finalizedSelection = true;
			Hide();
			_commandHistory.Push(item.CommandItem.Command);
			Action<FollowerCommands[]> onItemChosen = OnItemChosen;
			if (onItemChosen != null)
			{
				onItemChosen(_commandHistory.ToArray());
			}
		}

		public override void OnCancelButtonInput()
		{
			if (!_cancellable || !_canvasGroup.interactable)
			{
				return;
			}
			UIManager.PlayAudio("event:/ui/go_back");
			if (_commandStack.Count > 0)
			{
				_rootCommandItems = _commandStack.Pop();
				_commandHistory.Pop();
				StopAllCoroutines();
				CleanupWheelLoop();
				StartCoroutine(PrevCategory(_rootCommandItems));
				if (_commandStack.Count == 0 && !_cancellable)
				{
					_controlPrompts.HideCancelButton();
				}
			}
			else
			{
				base.OnCancelButtonInput();
			}
		}
	}
}
