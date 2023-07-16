using UnityEngine;

public class FollowerTask_AssistRitual : FollowerTask_AssistPlayerBase
{
	public WeightPlate WeightPlate;

	private bool OnPlate;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.AssistRitual;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public FollowerTask_AssistRitual(WeightPlate WeightPlate)
	{
		_helpingPlayer = true;
		this.WeightPlate = WeightPlate;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "worship");
	}

	protected override void AssistPlayerTick(float deltaGameTime)
	{
		if (EndIfPlayerIsDistant())
		{
			WeightPlate.Reserved = false;
		}
	}

	protected override bool EndIfPlayerIsDistant()
	{
		PlayerFarming instance = PlayerFarming.Instance;
		WeightPlateManager weightPlateManager = WeightPlate.WeightPlateManager;
		if (weightPlateManager != null && instance != null && Vector3.Distance(instance.transform.position, weightPlateManager.transform.position) > WeightPlate.WeightPlateManager.DeactivateRange)
		{
			End();
			return true;
		}
		return false;
	}

	protected override void OnArrive()
	{
		base.OnArrive();
		OnPlate = true;
		WeightPlate.OnTriggerEnter2D(null);
	}

	protected override void OnEnd()
	{
		if (OnPlate)
		{
			WeightPlate.OnTriggerExit2D(null);
		}
		base.OnEnd();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (WeightPlate == null)
		{
			return follower.transform.position;
		}
		return WeightPlate.transform.position;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.FacePosition(WeightPlate.WeightPlateManager.transform.position);
	}
}
