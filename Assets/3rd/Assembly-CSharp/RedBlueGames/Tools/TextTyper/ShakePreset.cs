using System;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[Serializable]
	public class ShakePreset
	{
		[Tooltip("Name identifying this preset. Can also be used as a ShakeLibrary indexer key.")]
		public string Name;

		[Range(0f, 20f)]
		[Tooltip("Amount of x-axis shake to apply during animation")]
		public float xPosStrength;

		[Range(0f, 20f)]
		[Tooltip("Amount of y-axis shake to apply during animation")]
		public float yPosStrength;

		[Range(0f, 90f)]
		[Tooltip("Amount of rotational shake to apply during animation")]
		public float RotationStrength;

		[Range(0f, 10f)]
		[Tooltip("Amount of scale shake to apply during animation")]
		public float ScaleStrength;
	}
}
