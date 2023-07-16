using Spine.Unity;
using UnityEngine;

public class Interaction_Grapple : Interaction
{
	public SkeletonAnimation Spine;

	public Interaction_Grapple TargetGrapple;

	public Transform BoneTarget;

	public LineRenderer lineRenderer;

	private string sGrapple;

	private string sRequiresGrapple;

	public override void GetLabel()
	{
		if (CrownAbilities.CrownAbilityUnlocked(CrownAbilities.TYPE.Abilities_GrappleHook))
		{
			Interactable = true;
			base.Label = sGrapple;
		}
		else
		{
			Interactable = false;
			base.Label = sRequiresGrapple;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		if (!(TargetGrapple == null))
		{
			lineRenderer.SetPosition(0, BoneTarget.position);
			lineRenderer.SetPosition(1, TargetGrapple.BoneTarget.position);
			if (TargetGrapple.transform.position.x < base.transform.position.x)
			{
				lineRenderer.gameObject.SetActive(false);
			}
			switch (Utils.GetAngleDirection(Utils.GetAngle(base.transform.position, TargetGrapple.transform.position)))
			{
			case Utils.Direction.Right:
				Spine.skeleton.SetSkin("right");
				break;
			case Utils.Direction.Down:
				Spine.skeleton.SetSkin("down");
				break;
			case Utils.Direction.Left:
				Spine.skeleton.SetSkin("left");
				break;
			case Utils.Direction.Up:
				Spine.skeleton.SetSkin("up");
				break;
			}
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sRequiresGrapple = "Requires grapple hook - needs loc";
	}

	public void BecomeTarget()
	{
		Spine.AnimationState.SetAnimation(0, "hit", false);
		Spine.AnimationState.AddAnimation(0, "target", true, 0f);
	}

	public void BecomeOrigin()
	{
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		state.transform.position = base.transform.position;
		state.gameObject.GetComponent<PlayerController>().DoGrapple(TargetGrapple);
	}

	private new void OnDrawGizmos()
	{
		if (TargetGrapple != null)
		{
			Utils.DrawLine(base.transform.position, TargetGrapple.transform.position, Color.yellow);
		}
	}
}
