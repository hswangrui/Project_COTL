using System;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[Serializable]
	public class CurvePreset
	{
		[Tooltip("Name identifying this preset. Can also be used as a CurveLibrary indexer key.")]
		public string Name;

		[Tooltip("Time offset between each character when calculating animation transform. 0 makes all characters move together. Other values produce a 'wave' effect.")]
		[Range(0f, 0.5f)]
		public float timeOffsetPerChar;

		[Tooltip("Curve showing x-position delta over time")]
		public AnimationCurve xPosCurve;

		[Tooltip("x-position curve is multiplied by this value")]
		[Range(0f, 20f)]
		public float xPosMultiplier;

		[Tooltip("Curve showing y-position delta over time")]
		public AnimationCurve yPosCurve;

		[Tooltip("y-position curve is multiplied by this value")]
		[Range(0f, 20f)]
		public float yPosMultiplier;

		[Tooltip("Curve showing 2D rotation delta over time")]
		public AnimationCurve rotationCurve;

		[Tooltip("2D rotation curve is multiplied by this value")]
		[Range(0f, 90f)]
		public float rotationMultiplier;

		[Tooltip("Curve showing uniform scale delta over time")]
		public AnimationCurve scaleCurve;

		[Tooltip("Uniform scale curve is multiplied by this value")]
		[Range(0f, 10f)]
		public float scaleMultiplier;
	}
}
