using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.FollowerInteractionWheel
{
	public class UIFollowerWheelInteractionItem : UIRadialWheelItem
	{
		[SerializeField]
		private Image _notification;

		[SerializeField]
		private TextMeshProUGUI _text;

		private string _iconText;

		private string _title;

		private string _description;

		private FollowerCommands _command;

		private CommandItem _commandItem;

		public FollowerCommands FollowerCommand
		{
			get
			{
				return _command;
			}
		}

		public CommandItem CommandItem
		{
			get
			{
				return _commandItem;
			}
		}

		public void Configure(Follower follower, CommandItem commandItem)
		{
			FollowerInteractionAlerts followerInteractions = DataManager.Instance.Alerts.FollowerInteractions;
			followerInteractions.OnAlertRemoved = (Action<FollowerCommands>)Delegate.Remove(followerInteractions.OnAlertRemoved, new Action<FollowerCommands>(ClearAlert));
			if (commandItem != null)
			{
				_iconText = FontImageNames.IconForCommand(commandItem.Command);
				_title = commandItem.GetTitle(follower);
				_description = commandItem.GetDescription(follower);
				_command = commandItem.Command;
				_text.text = _iconText;
				_commandItem = commandItem;
				if (DataManager.Instance.Alerts.FollowerInteractions.HasAlert(commandItem.Command))
				{
					_notification.gameObject.SetActive(true);
					FollowerInteractionAlerts followerInteractions2 = DataManager.Instance.Alerts.FollowerInteractions;
					followerInteractions2.OnAlertRemoved = (Action<FollowerCommands>)Delegate.Combine(followerInteractions2.OnAlertRemoved, new Action<FollowerCommands>(ClearAlert));
				}
				else
				{
					_notification.gameObject.SetActive(false);
				}
			}
			base.gameObject.SetActive(commandItem != null);
			_canvasGroup.alpha = ((commandItem != null) ? 1 : 0);
			_button.interactable = commandItem != null;
			if (commandItem != null && !commandItem.IsAvailable(follower))
			{
				_description = commandItem.GetLockedDescription(follower);
				DoInactive();
			}
			else
			{
				DoActive();
			}
		}

		private void ClearAlert(FollowerCommands command)
		{
			if (command == _command)
			{
				_notification.gameObject.SetActive(false);
			}
		}

		public override string GetTitle()
		{
			return _title;
		}

		public override bool IsValidOption()
		{
			return true;
		}

		public override bool Visible()
		{
			return _command != FollowerCommands.None;
		}

		public override void DoSelected()
		{
			base.DoSelected();
			DataManager.Instance.Alerts.FollowerInteractions.Remove(_command);
		}

		private void OnDestroy()
		{
			if (DataManager.Instance != null)
			{
				FollowerInteractionAlerts followerInteractions = DataManager.Instance.Alerts.FollowerInteractions;
				followerInteractions.OnAlertRemoved = (Action<FollowerCommands>)Delegate.Remove(followerInteractions.OnAlertRemoved, new Action<FollowerCommands>(ClearAlert));
			}
		}

		public override string GetDescription()
		{
			return _description;
		}
	}
}
