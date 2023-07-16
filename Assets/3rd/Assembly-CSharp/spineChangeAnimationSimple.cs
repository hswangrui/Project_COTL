using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

public class spineChangeAnimationSimple : BaseMonoBehaviour
{
	public SkeletonAnimation SkeletonData;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string skeletonAnimation;

	public bool loop;

	public float waitTime;

	public UnityEvent unityEventOnAnimationEnd;

	public string sfx;

	private bool playedAnimation;

	private TrackEntry Track;

	public bool DestroyAfterAnimation;

	public void Play()
	{
		changeAnimation();
	}

	public void changeAnimation()
	{
		SkeletonData.AnimationState.SetAnimation(0, skeletonAnimation, loop);
		if (!sfx.IsNullOrEmpty())
		{
			AudioManager.Instance.PlayOneShot(sfx, base.transform.position);
		}
		StartCoroutine(DelayAddEvent());
	}

	private IEnumerator DelayAddEvent()
	{
		yield return null;
		SkeletonData.AnimationState.Complete += AnimationEnd;
	}

	private void OnDestroy()
	{
		if (SkeletonData != null && SkeletonData.AnimationState != null)
		{
			SkeletonData.AnimationState.Complete -= AnimationEnd;
		}
	}

	private void OnDisable()
	{
		if (SkeletonData != null && SkeletonData.AnimationState != null)
		{
			SkeletonData.AnimationState.Complete -= AnimationEnd;
		}
	}

	private void AnimationEnd(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == skeletonAnimation)
		{
			unityEventOnAnimationEnd.Invoke();
			if (DestroyAfterAnimation)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
