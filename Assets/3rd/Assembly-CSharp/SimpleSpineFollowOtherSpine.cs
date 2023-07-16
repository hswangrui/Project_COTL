using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SimpleSpineFollowOtherSpine : BaseMonoBehaviour
{
	public SkeletonAnimation SpineToFollow;

	public SkeletonAnimation Spine;

	private MeshRenderer meshRenderer;

	private MeshRenderer MeshRendererToFollow;

	public Material material;

	public bool UseCoroutine;

	public bool setMaterial = true;

	private void OnEnable()
	{
		SpineToFollow.AnimationState.Start += MirrorAnimation;
		meshRenderer = Spine.GetComponent<MeshRenderer>();
		MeshRendererToFollow = SpineToFollow.GetComponent<MeshRenderer>();
	}

	private void OnDisable()
	{
		SpineToFollow.AnimationState.Start -= MirrorAnimation;
	}

	private void MirrorAnimation(TrackEntry trackEntry)
	{
		Spine.AnimationState.SetAnimation(0, SpineToFollow.AnimationName, SpineToFollow.AnimationState.GetCurrent(0).Loop).MixDuration = 0f;
		Spine.AnimationState.TimeScale = 0f;
	}

	private IEnumerator ApplyMaterial()
	{
		yield return new WaitForEndOfFrame();
		meshRenderer.material = material;
	}

	private void LateUpdate()
	{
		if (setMaterial)
		{
			meshRenderer.material = material;
		}
		Spine.state.GetCurrent(0).TrackTime = SpineToFollow.state.GetCurrent(0).TrackTime;
		Spine.AnimationState.Apply(Spine.skeleton);
		if (meshRenderer.enabled != MeshRendererToFollow.enabled)
		{
			meshRenderer.enabled = MeshRendererToFollow.enabled;
		}
		if (Spine.skeleton.ScaleX != SpineToFollow.skeleton.ScaleX)
		{
			Spine.skeleton.ScaleX = SpineToFollow.skeleton.ScaleX;
		}
	}
}
