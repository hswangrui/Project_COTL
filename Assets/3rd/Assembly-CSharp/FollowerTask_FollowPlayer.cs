using System;
using UnityEngine;

public class FollowerTask_FollowPlayer : FollowerTask_AssistPlayerBase
{
	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.FollowPlayer;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public FollowerTask_FollowPlayer()
	{
		_helpingPlayer = true;
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Idle);
	}

	protected override void AssistPlayerTick(float deltaGameTime)
	{
		PlayerFarming instance = PlayerFarming.Instance;
		if (instance == null)
		{
			return;
		}
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (!(follower != null))
		{
			return;
		}
		if (base.State == FollowerTaskState.Idle)
		{
			if (Vector3.Distance(instance.transform.position, follower.transform.position) > 3f)
			{
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
			}
		}
		else if (base.State == FollowerTaskState.GoingTo && _currentDestination.HasValue && Vector3.Distance(instance.transform.position, _currentDestination.Value) > 3f)
		{
			RecalculateDestination();
		}
	}

	protected override void OnPlayerLocationChange()
	{
		_brain.DesiredLocation = PlayerFarming.Location;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		PlayerFarming instance = PlayerFarming.Instance;
		if (instance == null)
		{
			return follower.transform.position;
		}
		float num = 1f;
		float f = Utils.GetAngle(instance.transform.position, follower.transform.position) * ((float)Math.PI / 180f);
		return instance.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}
}
