using Spine.Unity;
using UnityEngine;

public class AnimateOnTrigger : BaseMonoBehaviour
{
	public float ActivateDistance = 4f;

	public Vector3 ActivateOffset = Vector3.zero;

	public SkeletonAnimation Spine;

	public bool HideBeforeTriggered = true;

	[SpineAnimation("", "Spine", true, false)]
	public string InitialAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string TriggeredAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string EndOnAnimation;

	private bool Activated;

	private GameObject Player;

	private StateMachine state;

	private void Start()
	{
		if (HideBeforeTriggered)
		{
			Spine.gameObject.SetActive(false);
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, InitialAnimation, false);
		}
	}

	private void Update()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null) && !Activated && Vector3.Distance(base.transform.position + ActivateOffset, Player.transform.position) < ActivateDistance)
		{
			Activated = true;
			Spine.gameObject.SetActive(true);
			Spine.AnimationState.SetAnimation(0, TriggeredAnimation, false);
			Spine.AnimationState.AddAnimation(0, EndOnAnimation, false, 0f);
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, ActivateDistance, Color.green);
	}
}
