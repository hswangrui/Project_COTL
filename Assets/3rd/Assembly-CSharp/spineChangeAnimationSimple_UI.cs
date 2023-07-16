using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class spineChangeAnimationSimple_UI : BaseMonoBehaviour
{
	public SkeletonGraphic SkeletonData;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string skeletonAnimation;

	public bool loop;

	public float waitTime;

	public UnityEvent unityEventOnAnimationEnd;

	public AudioClip sfx;

	private bool playedAnimation;

	private TrackEntry Track;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void changeAnimation()
	{
		Track = SkeletonData.AnimationState.SetAnimation(0, skeletonAnimation, loop);
		SkeletonData.AnimationState.End += delegate
		{
			unityEventOnAnimationEnd.Invoke();
		};
	}
}
