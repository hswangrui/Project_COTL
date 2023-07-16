using System;
using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace src.UI.Items
{
	public class TutorialMenuItem : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		public Action<TutorialMenuItem> OnTopicChosen;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private TutorialTopic _topic;

		[SerializeField]
		private TutorialAlert _alert;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public TutorialTopic Topic
		{
			get
			{
				return _topic;
			}
		}

		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClicked);
			_alert.Configure(_topic);
		}

		private void OnButtonClicked()
		{
			Action<TutorialMenuItem> onTopicChosen = OnTopicChosen;
			if (onTopicChosen != null)
			{
				onTopicChosen(this);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			_alert.TryRemoveAlert();
		}
	}
}
