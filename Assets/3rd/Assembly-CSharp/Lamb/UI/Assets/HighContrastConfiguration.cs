using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "High Contrast Configuration", menuName = "Massive Monster/High Contrast Configuration", order = 1)]
	public class HighContrastConfiguration : ScriptableObject
	{
		[Serializable]
		public struct HighContrastColorSet
		{
			public Color NormalColor;

			public Color HighlightColor;

			public Color PressedColor;

			public Color SelectedColor;

			public Color DisabledColor;

			public HighContrastColorSet(Selectable selectable)
			{
				NormalColor = selectable.colors.normalColor;
				HighlightColor = selectable.colors.highlightedColor;
				PressedColor = selectable.colors.pressedColor;
				SelectedColor = selectable.colors.selectedColor;
				DisabledColor = selectable.colors.disabledColor;
			}

			public HighContrastColorSet(TMP_Text text)
			{
				NormalColor = text.color;
				HighlightColor = text.color;
				PressedColor = text.color;
				SelectedColor = text.color;
				DisabledColor = text.color;
			}

			public HighContrastColorSet(SelectableColourProxy colourProxy)
			{
				NormalColor = colourProxy.Colors.normalColor;
				HighlightColor = colourProxy.Colors.highlightedColor;
				PressedColor = colourProxy.Colors.pressedColor;
				SelectedColor = colourProxy.Colors.selectedColor;
				DisabledColor = colourProxy.Colors.disabledColor;
			}

			public void Apply(Selectable selectable)
			{
				selectable.colors = GetColorBlock();
			}

			public void Apply(SelectableColourProxy colourProxy)
			{
				colourProxy.Colors = GetColorBlock();
			}

			private ColorBlock GetColorBlock()
			{
				ColorBlock result = default(ColorBlock);
				result.normalColor = NormalColor;
				result.highlightedColor = HighlightColor;
				result.pressedColor = PressedColor;
				result.selectedColor = SelectedColor;
				result.disabledColor = DisabledColor;
				result.colorMultiplier = 1f;
				return result;
			}

			public void Apply(TMP_Text text)
			{
				text.color = NormalColor;
			}
		}

		[Header("Selectable")]
		[SerializeField]
		private HighContrastColorSet _colorTransitionSet;

		[Header("Text")]
		[SerializeField]
		private Color _textColor;

		public HighContrastColorSet ColorTransitionSet
		{
			get
			{
				return _colorTransitionSet;
			}
		}

		public Color TextColor
		{
			get
			{
				return _textColor;
			}
		}
	}
}
