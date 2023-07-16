using Spine.Unity;
using UnityEngine;

public class WormHeadManager : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string Head;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string HeadFacingUp;

	[SerializeField]
	private Vector2 upAngle = new Vector2(45f, 135f);

	private bool FacingUp;

	private StateMachine state;

	private void Start()
	{
		state = GetComponentInParent<StateMachine>();
	}

	private void LateUpdate()
	{
		if (state.facingAngle >= upAngle.x && state.facingAngle <= upAngle.y)
		{
			if (!FacingUp)
			{
				Spine.skeleton.SetSkin(HeadFacingUp);
				Spine.skeleton.SetSlotsToSetupPose();
				FacingUp = true;
			}
		}
		else if (FacingUp)
		{
			Spine.skeleton.SetSkin(Head);
			Spine.skeleton.SetSlotsToSetupPose();
			FacingUp = false;
		}
	}
}
