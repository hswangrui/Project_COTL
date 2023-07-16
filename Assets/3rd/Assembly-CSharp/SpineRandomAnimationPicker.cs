using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class SpineRandomAnimationPicker : BaseMonoBehaviour
{
	public List<SpineAnimations> spineAnims = new List<SpineAnimations>();

	public bool randomTimeScale = true;

	public bool IsUsingLOD;

	private SkeletonAnimation Spine;

	private void Start()
	{
		PickRandomAnimation();
	}

	private void PickRandomAnimation()
	{
		if (spineAnims.Count == 0)
		{
			return;
		}
		if (Spine == null)
		{
			Spine = base.gameObject.GetComponent<SkeletonAnimation>();
		}
		if (!(Spine == null))
		{
			SpineAnimations spineAnimations = spineAnims.RandomElement();
			if (!string.IsNullOrEmpty(spineAnimations.TriggeredAnimation))
			{
				Spine.AnimationState.SetAnimation(0, spineAnimations.TriggeredAnimation, true);
			}
			if (randomTimeScale)
			{
				Spine.timeScale = Random.Range(0.8f, 1.2f);
			}
		}
	}

	private void OnBecameInvisible()
	{
		if (Spine == null)
		{
			Spine = base.gameObject.GetComponent<SkeletonAnimation>();
		}
		if (!(Spine == null) && !IsUsingLOD)
		{
			Spine.enabled = false;
		}
	}

	private void OnBecameVisible()
	{
		if (Spine == null)
		{
			Spine = base.gameObject.GetComponent<SkeletonAnimation>();
		}
		if (!(Spine == null) && !IsUsingLOD)
		{
			Spine.enabled = true;
		}
	}
}
