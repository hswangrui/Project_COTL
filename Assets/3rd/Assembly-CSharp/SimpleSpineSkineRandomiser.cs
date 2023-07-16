using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class SimpleSpineSkineRandomiser : BaseMonoBehaviour
{
	[Serializable]
	public class SkinAndChance
	{
		public string SkinName;

		[Range(0f, 100f)]
		public int Probability = 50;
	}

	public List<SkinAndChance> Skins = new List<SkinAndChance>();

	private ISkeletonAnimation skeletonAnimation;

	private void Start()
	{
		skeletonAnimation = GetComponent<ISkeletonAnimation>();
		int[] array = new int[Skins.Count];
		int num = -1;
		while (++num < Skins.Count)
		{
			array[num] = Skins[num].Probability;
		}
		int randomWeightedIndex = Utils.GetRandomWeightedIndex(array);
		skeletonAnimation.Skeleton.SetSkin(Skins[randomWeightedIndex].SkinName);
	}
}
