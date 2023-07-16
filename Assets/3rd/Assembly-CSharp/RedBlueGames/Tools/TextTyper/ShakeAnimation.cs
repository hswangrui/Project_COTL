using TMPro;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class ShakeAnimation : TextAnimation
	{
		[SerializeField]
		[Tooltip("The library of ShakePresets that can be used by this component.")]
		private ShakeLibrary shakeLibrary;

		[SerializeField]
		[Tooltip("The name (key) of the ShakePreset this animation should use.")]
		private string shakePresetKey;

		private ShakePreset shakePreset;

		public void LoadPreset(ShakeLibrary library, string presetKey)
		{
			shakeLibrary = library;
			shakePresetKey = presetKey;
			shakePreset = library[presetKey];
		}

		protected override void OnEnable()
		{
			if (shakeLibrary != null && !string.IsNullOrEmpty(shakePresetKey))
			{
				LoadPreset(shakeLibrary, shakePresetKey);
			}
			base.OnEnable();
		}

		protected override void Animate(int characterIndex, out Vector2 translation, out float rotation, out float scale)
		{
			translation = Vector2.zero;
			rotation = 0f;
			scale = 1f;
			if (shakePreset != null && characterIndex >= base.FirstCharToAnimate && characterIndex <= base.LastCharToAnimate)
			{
				float x = Random.Range(0f - shakePreset.xPosStrength, shakePreset.xPosStrength);
				float y = Random.Range(0f - shakePreset.yPosStrength, shakePreset.yPosStrength);
				translation = new Vector2(x, y);
				rotation = Random.Range(0f - shakePreset.RotationStrength, shakePreset.RotationStrength);
				scale = 1f + Random.Range(0f - shakePreset.ScaleStrength, shakePreset.ScaleStrength);
			}
		}
	}
}
