using Spine.Unity;
using UnityEngine;

public class SimpleFlipSpineOnFacing : MonoBehaviour
{
	private StateMachine state;

	public int Dir;

	private SkeletonAnimation anim;

	public bool ReverseFacing;

	private void Start()
	{
		state = GetComponentInParent<StateMachine>();
		anim = GetComponent<SkeletonAnimation>();
	}

	private void LateUpdate()
	{
		Dir = ((state.facingAngle > 90f && state.facingAngle < 270f) ? 1 : (-1)) * ((!ReverseFacing) ? 1 : (-1));
		anim.skeleton.ScaleX = Dir;
	}
}
