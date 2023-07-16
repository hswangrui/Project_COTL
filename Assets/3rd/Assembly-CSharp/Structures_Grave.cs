using System;
using UnityEngine;

public class Structures_Grave : StructureBrain
{
	private const float CHANCE_OF_SPAWNING_ZOMBIE = 0.05f;

	public bool UpgradedGrave;

	public Action<int> OnSoulsGained;

	public int SoulMax
	{
		get
		{
			if (UpgradedGrave)
			{
				return 15;
			}
			return 10;
		}
	}

	public float TimeBetweenSouls
	{
		get
		{
			if (UpgradedGrave)
			{
				return 600f;
			}
			return 360f;
		}
	}

	public int SoulCount
	{
		get
		{
			return Data.SoulCount;
		}
		set
		{
			int soulCount = SoulCount;
			Data.SoulCount = Mathf.Clamp(value, 0, SoulMax);
			if (SoulCount > soulCount)
			{
				Action<int> onSoulsGained = OnSoulsGained;
				if (onSoulsGained != null)
				{
					onSoulsGained(SoulCount - soulCount);
				}
			}
		}
	}

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return null;
	}

	public bool CheckOverrideComplete()
	{
		return true;
	}

	public override void OnAdded()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
	}

	private void OnNewPhase()
	{
	}

	private void SpawnZombie(FollowerInfo deadBody)
	{
		FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(deadBody);
		orCreateBrain.ResetStats();
		orCreateBrain.HardSwapToTask(new FollowerTask_ManualControl());
		orCreateBrain.Location = FollowerLocation.Base;
		orCreateBrain.DesiredLocation = FollowerLocation.Base;
		orCreateBrain.CurrentTask.Arrive();
		orCreateBrain.ApplyCurseState(Thought.Zombie);
		orCreateBrain.LastPosition = Data.Position;
		orCreateBrain.HardSwapToTask(new FollowerTask_Zombie());
		FollowerManager.CreateNewFollower(orCreateBrain._directInfoAccess, Data.Position);
	}

	private Grave GetGrave(int ID)
	{
		foreach (Grave grafe in Grave.Graves)
		{
			if (grafe.StructureInfo.ID == ID)
			{
				return grafe;
			}
		}
		return null;
	}
}
