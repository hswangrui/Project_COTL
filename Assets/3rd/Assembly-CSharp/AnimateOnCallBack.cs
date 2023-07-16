using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class AnimateOnCallBack : BaseMonoBehaviour
{
	public string AnimationName;

	public string AddAnimationName;

	public bool Looping;

	public SkeletonAnimation Spine;

	public bool DestroyAfterAnimation;

	public UnityEvent Callback;

	public int Direction;

	public void SetSpineFacing()
	{
		Spine.skeleton.ScaleX = Direction;
	}

	public void Animate()
	{
		StartCoroutine(DoAnimation());
	}

	private IEnumerator DoAnimation()
	{
		TrackEntry trackEntry = Spine.AnimationState.SetAnimation(0, AnimationName, AddAnimationName != "" && Looping);
		float num = trackEntry.Animation.Duration;
		if (AddAnimationName != "")
		{
			trackEntry = Spine.AnimationState.AddAnimation(0, AddAnimationName, Looping, 0f);
			num += trackEntry.Animation.Duration;
		}
		yield return new WaitForSeconds(num);
		if (Callback.GetPersistentEventCount() > 0)
		{
			Callback.Invoke();
		}
		if (DestroyAfterAnimation)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
