using System.Collections.Generic;
using FMODUnity;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SimpleSpineActivateComponentAfterPlay : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string Animation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AnimationToQueue;

	public List<BaseMonoBehaviour> ComponentsToEnable;

	public List<GameObject> GameObjectToEnable;

	[EventRef]
	public string onStartSfx = "";

	[EventRef]
	public string onQueSfx = "";

	private void Start()
	{
		foreach (BaseMonoBehaviour item in ComponentsToEnable)
		{
			item.enabled = false;
		}
		foreach (GameObject item2 in GameObjectToEnable)
		{
			item2.SetActive(false);
		}
		Spine.AnimationState.SetAnimation(0, Animation, false);
		if (onStartSfx != "")
		{
			AudioManager.Instance.PlayOneShot(onStartSfx, base.transform.position);
		}
		Spine.AnimationState.AddAnimation(0, AnimationToQueue, true, 0f);
		Spine.AnimationState.Complete += AnimationState_Complete;
	}

	private void AnimationState_Complete(TrackEntry trackEntry)
	{
		Spine.AnimationState.Complete -= AnimationState_Complete;
		if (onQueSfx != "")
		{
			AudioManager.Instance.PlayOneShot(onQueSfx, base.transform.position);
		}
		foreach (BaseMonoBehaviour item in ComponentsToEnable)
		{
			item.enabled = true;
		}
		foreach (GameObject item2 in GameObjectToEnable)
		{
			item2.SetActive(true);
		}
	}

	private void OnDisable()
	{
		Spine.AnimationState.Complete -= AnimationState_Complete;
	}
}
