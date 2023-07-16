using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class SimpleRandomSpineSkin : BaseMonoBehaviour
{
	[Serializable]
	public class SkinNames
	{
		[SpineSkin("", "SkeletonData", true, false, false)]
		public string Skin;
	}

	public SkeletonAnimation SkeletonData;

	public List<SkinNames> Skins = new List<SkinNames>();

	private void Start()
	{
		string skin = Skins[UnityEngine.Random.Range(0, Skins.Count)].Skin;
		SkeletonData.skeleton.SetSkin(skin);
	}
}
