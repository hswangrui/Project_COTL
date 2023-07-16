using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using TMPro;
using UnityEngine;

namespace src.UI.Menus.ShareHouseMenu
{
	public class UIShareHouseMenuController : UIMenuBase
	{
		[SerializeField]
		private ShareHouseItem[] _followerSelectItems;

		[SerializeField]
		private TMP_Text _occupiedText;

		[SerializeField]
		private TMP_Text _buttonPrompt;

		private Interaction_Bed _interactionBed;

		private StructuresData _structuresData;

		public void Show(Interaction_Bed interactionBed, bool instant = false)
		{
			_interactionBed = interactionBed;
			_structuresData = _interactionBed.Structure.Structure_Info;
			OnShareHouseItemSelected(_followerSelectItems[0]);
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/ui/open_menu");
			for (int i = 0; i < _followerSelectItems.Length; i++)
			{
				ShareHouseItem shareHouseItem = _followerSelectItems[i];
				if (_structuresData.MultipleFollowerIDs.Count >= i + 1)
				{
					shareHouseItem.Configure(FollowerInfo.GetInfoByID(_structuresData.MultipleFollowerIDs[i]));
				}
				else
				{
					shareHouseItem.Configure(null);
				}
				ShareHouseItem shareHouseItem2 = shareHouseItem;
				shareHouseItem2.OnShareHouseItemSelected = (Action<ShareHouseItem>)Delegate.Combine(shareHouseItem2.OnShareHouseItemSelected, new Action<ShareHouseItem>(OnShareHouseItemSelected));
				shareHouseItem.Button.onClick.AddListener(delegate
				{
					OnFollowerSelected(shareHouseItem);
				});
			}
			UpdateOccupiedText();
		}

		private void OnShareHouseItemSelected(ShareHouseItem shareHouseItem)
		{
			if (shareHouseItem.FollowerInfo != null)
			{
				_buttonPrompt.text = ScriptLocalization.UI_ShareHouseMenu.ReassignBed;
			}
			else
			{
				_buttonPrompt.text = ScriptLocalization.UI_ShareHouseMenu.AssignBed;
			}
		}

		private void OnFollowerSelected(ShareHouseItem shareHouseItem)
		{
			UIFollowerSelectMenuController uIFollowerSelectMenuController = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
			//uIFollowerSelectMenuController.VotingType = TwitchVoting.VotingType.BED;
			uIFollowerSelectMenuController.Show(DataManager.Instance.Followers, GetBlacklist());
			PushInstance(uIFollowerSelectMenuController);
			uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
			{
				shareHouseItem.Configure(followerInfo);
				_interactionBed.OnFollowerChosenForConversion(followerInfo);
				UpdateOccupiedText();
			});
		}

		private List<FollowerInfo> GetBlacklist()
		{
			List<FollowerInfo> followerBlacklist = _interactionBed.GetFollowerBlacklist();
			ShareHouseItem[] followerSelectItems = _followerSelectItems;
			foreach (ShareHouseItem shareHouseItem in followerSelectItems)
			{
				if (!followerBlacklist.Contains(shareHouseItem.FollowerInfo))
				{
					followerBlacklist.Add(shareHouseItem.FollowerInfo);
				}
			}
			return followerBlacklist;
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(this);
		}

		private void UpdateOccupiedText()
		{
			int num = 0;
			ShareHouseItem[] followerSelectItems = _followerSelectItems;
			for (int i = 0; i < followerSelectItems.Length; i++)
			{
				if (followerSelectItems[i].FollowerInfo != null)
				{
					num++;
				}
			}
			_occupiedText.text = string.Format(ScriptLocalization.UI_ShareHouseMenu.OccupiedSlots, num, 3);
		}
	}
}
