using System;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

public abstract class FollowerTask_AssistPlayerBase : FollowerTask
{
	protected bool _helpingPlayer;

	private Follower _follower;

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return _helpingPlayer;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return _helpingPlayer;
		}
	}

	public virtual float AssistRange
	{
		get
		{
			return 8f;
		}
	}

	protected sealed override int GetSubTaskCode()
	{
		return 0;
	}

	protected sealed override void TaskTick(float deltaGameTime)
	{
		if (_helpingPlayer && PlayerFarming.Location != _brain.Location)
		{
			if (new List<FollowerLocation>(LocationManager.LocationsInState(LocationState.Active)).Count > 0)
			{
				OnPlayerLocationChange();
			}
		}
		else
		{
			AssistPlayerTick(deltaGameTime);
		}
	}

	protected abstract void AssistPlayerTick(float deltaGameTime);

	protected virtual void OnPlayerLocationChange()
	{
		End();
	}

	protected virtual bool EndIfPlayerIsDistant()
	{
		PlayerFarming instance = PlayerFarming.Instance;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null && instance != null && Vector3.Distance(instance.transform.position, follower.transform.position) > AssistRange)
		{
			End();
			return true;
		}
		return false;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (_helpingPlayer)
		{
			_follower = follower;
			BiomeGenerator.OnBiomeChangeRoom += OnChangeRoom;
			Interaction_PlayerBuild.PlayerActivatingStart = (Action<BuildSitePlot>)Delegate.Combine(Interaction_PlayerBuild.PlayerActivatingStart, new Action<BuildSitePlot>(PlayerActivatingBuildSite));
			Interaction_Berries.PlayerActivatingStart = (Action<Interaction_Berries>)Delegate.Combine(Interaction_Berries.PlayerActivatingStart, new Action<Interaction_Berries>(PlayerActivatingBerries));
			Interaction_Weed.PlayerActivatingStart = (Action<Interaction_Weed>)Delegate.Combine(Interaction_Weed.PlayerActivatingStart, new Action<Interaction_Weed>(PlayerActivatingWeed));
			Interaction_PlayerClearRubble.PlayerActivatingStart = (Action<Rubble>)Delegate.Combine(Interaction_PlayerClearRubble.PlayerActivatingStart, new Action<Rubble>(PlayerActivatingRubble));
			WeightPlateManager.OnPlayerActivatingWeightPlate = (WeightPlateManager.PlayerActivatingWeightPlate)Delegate.Combine(WeightPlateManager.OnPlayerActivatingWeightPlate, new WeightPlateManager.PlayerActivatingWeightPlate(PlayerActivatingAssistRitual));
		}
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		_follower = null;
		BiomeGenerator.OnBiomeChangeRoom -= OnChangeRoom;
		Interaction_PlayerBuild.PlayerActivatingStart = (Action<BuildSitePlot>)Delegate.Remove(Interaction_PlayerBuild.PlayerActivatingStart, new Action<BuildSitePlot>(PlayerActivatingBuildSite));
		Interaction_Berries.PlayerActivatingStart = (Action<Interaction_Berries>)Delegate.Remove(Interaction_Berries.PlayerActivatingStart, new Action<Interaction_Berries>(PlayerActivatingBerries));
		Interaction_Weed.PlayerActivatingStart = (Action<Interaction_Weed>)Delegate.Remove(Interaction_Weed.PlayerActivatingStart, new Action<Interaction_Weed>(PlayerActivatingWeed));
		Interaction_PlayerClearRubble.PlayerActivatingStart = (Action<Rubble>)Delegate.Remove(Interaction_PlayerClearRubble.PlayerActivatingStart, new Action<Rubble>(PlayerActivatingRubble));
		WeightPlateManager.OnPlayerActivatingWeightPlate = (WeightPlateManager.PlayerActivatingWeightPlate)Delegate.Remove(WeightPlateManager.OnPlayerActivatingWeightPlate, new WeightPlateManager.PlayerActivatingWeightPlate(PlayerActivatingAssistRitual));
	}

	private void OnChangeRoom()
	{
		_follower.ClearPath();
		_follower.transform.position = PlayerFarming.Instance.transform.position;
		End();
	}

	private void PlayerActivatingBuildSite(BuildSitePlot buildSite)
	{
		_brain.TransitionToTask(new FollowerTask_Build(buildSite));
	}

	private void PlayerActivatingBerries(Interaction_Berries berries)
	{
		if (_brain.CurrentTaskType != FollowerTaskType.Forage)
		{
			_brain.TransitionToTask(new FollowerTask_Forage());
		}
	}

	private void PlayerActivatingTree(Interaction_Woodcutting tree)
	{
		if (_brain.CurrentTaskType != FollowerTaskType.ChopTrees)
		{
			_brain.TransitionToTask(new FollowerTask_ChopTrees());
		}
	}

	private void PlayerActivatingWeed(Interaction_Weed weed)
	{
		if (_brain.CurrentTaskType != FollowerTaskType.ClearWeeds)
		{
			_brain.TransitionToTask(new FollowerTask_ClearWeeds(weed));
		}
	}

	private void PlayerActivatingRubble(Rubble rubble)
	{
		if (_brain.CurrentTaskType != FollowerTaskType.ClearRubble)
		{
			_brain.TransitionToTask(new FollowerTask_ClearRubble(rubble));
		}
	}

	private void PlayerActivatingAssistRitual(List<WeightPlate> WeightPlates)
	{
		if (_brain.CurrentTaskType == FollowerTaskType.AssistRitual)
		{
			return;
		}
		WeightPlate weightPlate = null;
		float num = float.MaxValue;
		foreach (WeightPlate WeightPlate in WeightPlates)
		{
			if (!WeightPlate.Reserved)
			{
				float num2 = Vector3.Distance(WeightPlate.transform.position, _follower.transform.position);
				if (num2 < num)
				{
					num = num2;
					weightPlate = WeightPlate;
				}
			}
		}
		if (weightPlate != null)
		{
			weightPlate.Reserved = true;
			_brain.TransitionToTask(new FollowerTask_AssistRitual(weightPlate));
		}
	}
}
