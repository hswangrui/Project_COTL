using System;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Items
{
	public class FollowerThoughtItem : BaseMonoBehaviour
	{
		private const int kSpriteSizeReduction = 10;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TextMeshProUGUI _thoughtDescription;

		[SerializeField]
		private Sprite _fiathDoubleDown;

		[SerializeField]
		private Sprite _faithDown;

		[SerializeField]
		private Sprite _faithUp;

		[SerializeField]
		private Sprite _faithDoubleUp;

		[SerializeField]
		private MMSelectable _selectable;

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}

		public void Configure(ThoughtData thoughtData)
		{
			if (thoughtData.Modifier <= -7f)
			{
				_icon.sprite = _fiathDoubleDown;
			}
			else if (thoughtData.Modifier < 0f)
			{
				_icon.sprite = _faithDown;
			}
			else if (thoughtData.Modifier >= 7f)
			{
				_icon.sprite = _faithDoubleUp;
			}
			else if (thoughtData.Modifier >= 0f)
			{
				_icon.sprite = _faithUp;
			}
			string text = FollowerThoughts.GetLocalisedName(thoughtData.ThoughtType);
			if (text.Contains("<sprite"))
			{
				text = text.Insert(text.IndexOf("<sprite", StringComparison.OrdinalIgnoreCase), string.Format("<size=-{0}>", 10));
				text += "</size>";
			}
			_thoughtDescription.text = string.Join(": ", text.Bold(), FollowerThoughts.GetLocalisedDescription(thoughtData.ThoughtType));
		}
	}
}
