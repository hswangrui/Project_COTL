using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples
{
	public class SpineEventListener : BaseMonoBehaviour
	{
		public List<spineEventListeners> spineEventListeners = new List<spineEventListeners>();

		[SerializeField]
		private SkeletonAnimation spine;

		public void UpdateSpine()
		{
			if (spine == null)
			{
				spine = base.gameObject.GetComponent<SkeletonAnimation>();
			}
			if (!(spine != null))
			{
				return;
			}
			foreach (spineEventListeners spineEventListener in spineEventListeners)
			{
				spineEventListener.skeletonAnimation = spine;
			}
		}

		private void Start()
		{
			foreach (spineEventListeners spineEventListener in spineEventListeners)
			{
				spineEventListener.Start();
			}
		}
	}
}
