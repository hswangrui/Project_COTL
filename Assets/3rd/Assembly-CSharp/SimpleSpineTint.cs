using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class SimpleSpineTint : BaseMonoBehaviour
{
	[Serializable]
	public class MyDictionaryEntry
	{
		public string Attachment;

		public Color color = Color.white;
	}

	[SerializeField]
	private List<MyDictionaryEntry> ItemImages;

	private void Start()
	{
		SkeletonAnimation component = GetComponent<SkeletonAnimation>();
		foreach (MyDictionaryEntry itemImage in ItemImages)
		{
			component.skeleton.FindSlot(itemImage.Attachment).SetColor(itemImage.color);
		}
	}
}
