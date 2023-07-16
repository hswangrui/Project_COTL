using System;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Overlays.EventOverlay
{
	public class UIEventOverlay : UIMenuBase
	{
		[Serializable]
		private struct Entry
		{
			[SerializeField]
			private TextMeshProUGUI _text;

			[SerializeField]
			private Image _image;

			public void Setup(SeasonalEventData.OnboardingEntry onboardingEntry)
			{
				_text.text = onboardingEntry.Text;
				_image.sprite = onboardingEntry.Image;
			}
		}

		[Header("Copy")]
		[SerializeField]
		private TextMeshProUGUI _title;

		[SerializeField]
		private TextMeshProUGUI _description;

		[Header("Entries")]
		[SerializeField]
		private Entry _entry1;

		[SerializeField]
		private Entry _entry2;

		[SerializeField]
		private Entry _entry3;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _acceptButton;

		public void Show(SeasonalEventData seasonalEventData, bool instant = false)
		{
			Show(instant);
			_title.text = seasonalEventData.OnboardingTitle;
			_description.text = seasonalEventData.Description;
			_entry1.Setup(seasonalEventData.Entry1);
			_entry2.Setup(seasonalEventData.Entry2);
			_entry3.Setup(seasonalEventData.Entry3);
			_acceptButton.onClick.AddListener(delegate
			{
				Hide();
			});
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
