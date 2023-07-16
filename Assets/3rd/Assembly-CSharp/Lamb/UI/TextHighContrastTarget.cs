using Lamb.UI.Assets;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class TextHighContrastTarget : HighContrastTarget
	{
		private TMP_Text _text;

		private Color _cachedColor;

		public TextHighContrastTarget(TMP_Text text, HighContrastConfiguration configuration)
			: base(configuration)
		{
			_text = text;
		}

		public override void Apply(bool state)
		{
			if (state)
			{
				_text.color = _configuration.TextColor;
			}
			else
			{
				_text.color = _cachedColor;
			}
		}

		public override void Init()
		{
			_cachedColor = _text.color;
		}
	}
}
