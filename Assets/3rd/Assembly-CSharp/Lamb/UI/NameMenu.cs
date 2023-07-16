using System;
using UnityEngine;

namespace Lamb.UI
{
	public class NameMenu : UISubmenuBase
	{
		public Action OnStartedEditingName;

		public Action<string> OnEndedEditingName;

		[SerializeField]
		private MMInputField _nameInputField;

		[SerializeField]
		private MMButton _randomiseButton;

		private Follower _follower;

		public bool Editing { get; private set; }

		private void Start()
		{
			MMInputField nameInputField = _nameInputField;
			nameInputField.OnStartedEditing = (Action)Delegate.Combine(nameInputField.OnStartedEditing, new Action(OnStartedEditing));
			MMInputField nameInputField2 = _nameInputField;
			nameInputField2.OnEndedEditing = (Action<string>)Delegate.Combine(nameInputField2.OnEndedEditing, new Action<string>(OnEndedEditing));
			_randomiseButton.onClick.AddListener(RandomiseName);
		}

		public void Configure(Follower follower)
		{
			_follower = follower;
			_nameInputField.text = _follower.Brain.Info.Name;
		}

		private void OnStartedEditing()
		{
			Editing = true;
			Action onStartedEditingName = OnStartedEditingName;
			if (onStartedEditingName != null)
			{
				onStartedEditingName();
			}
		}

		private void OnEndedEditing(string text)
		{
			if (text != _follower.Brain.Info.Name)
			{
				_follower.SetBodyAnimation("Indoctrinate/indoctrinate-react", false);
				_follower.AddBodyAnimation("pray", true, 0f);
			}
			Editing = false;
			Action<string> onEndedEditingName = OnEndedEditingName;
			if (onEndedEditingName != null)
			{
				onEndedEditingName(text);
			}
		}

		private void RandomiseName()
		{
			_nameInputField.text = FollowerInfo.GenerateName();
		}

		protected override void OnHideStarted()
		{
			Action<string> onEndedEditingName = OnEndedEditingName;
			if (onEndedEditingName != null)
			{
				onEndedEditingName(_nameInputField.text);
			}
		}
	}
}
