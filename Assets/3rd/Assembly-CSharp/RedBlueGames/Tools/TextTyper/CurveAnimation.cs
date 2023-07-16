using TMPro;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class CurveAnimation : TextAnimation
	{
		[SerializeField]
		[Tooltip("The library of CurvePresets that can be used by this component.")]
		private CurveLibrary curveLibrary;

		[SerializeField]
		[Tooltip("The name (key) of the CurvePreset this animation should use.")]
		private string curvePresetKey;

		private CurvePreset curvePreset;

		private float timeAnimationStarted;

		public void LoadPreset(CurveLibrary library, string presetKey)
		{
			curveLibrary = library;
			curvePresetKey = presetKey;
			curvePreset = library[presetKey];
		}

		protected override void OnEnable()
		{
			if (curveLibrary != null && !string.IsNullOrEmpty(curvePresetKey))
			{
				LoadPreset(curveLibrary, curvePresetKey);
			}
			timeAnimationStarted = Time.time;
			base.OnEnable();
		}

		protected override void Animate(int characterIndex, out Vector2 translation, out float rotation, out float scale)
		{
			translation = Vector2.zero;
			rotation = 0f;
			scale = 1f;
			if (curvePreset != null && characterIndex >= base.FirstCharToAnimate && characterIndex <= base.LastCharToAnimate)
			{
				float time = Time.time - timeAnimationStarted + (float)characterIndex * curvePreset.timeOffsetPerChar;
				float x = curvePreset.xPosCurve.Evaluate(time) * curvePreset.xPosMultiplier;
				float y = curvePreset.yPosCurve.Evaluate(time) * curvePreset.yPosMultiplier;
				translation = new Vector2(x, y);
				rotation = curvePreset.rotationCurve.Evaluate(time) * curvePreset.rotationMultiplier;
				scale += curvePreset.scaleCurve.Evaluate(time) * curvePreset.scaleMultiplier;
			}
		}
	}
}
