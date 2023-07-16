using System;
using UnityEngine;

public class Structures_Scarecrow : StructureBrain
{
	public Action OnCatchBird;

	private float LastBirdCaught;

	public bool HasBird;

	public override void OnAdded()
	{
		if (Data.Type == TYPES.SCARECROW_2)
		{
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		}
	}

	public override void OnRemoved()
	{
		if (Data.Type == TYPES.SCARECROW_2)
		{
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		}
	}

	private void OnNewPhaseStarted()
	{
		if (!HasBird && UnityEngine.Random.value <= 0.6f && TimeManager.TotalElapsedGameTime - LastBirdCaught > 720f)
		{
			HasBird = true;
			Action onCatchBird = OnCatchBird;
			if (onCatchBird != null)
			{
				onCatchBird();
			}
		}
	}

	public void EmptyTrap()
	{
		HasBird = false;
		LastBirdCaught = TimeManager.TotalElapsedGameTime;
	}
}
