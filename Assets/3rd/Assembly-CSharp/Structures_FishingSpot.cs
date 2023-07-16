using System;
using System.Collections.Generic;

public class Structures_FishingSpot : StructureBrain
{
	public const int FISH_SPAWN_PER_DAY = 10;

	public int FishSpawnedToday;

	public List<Interaction_Fishing.FishType> SpawnedFish = new List<Interaction_Fishing.FishType>();

	public bool CanSpawnFish
	{
		get
		{
			return FishSpawnedToday < 10;
		}
	}

	public override void OnAdded()
	{
		base.OnAdded();
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDay));
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDay));
	}

	private void OnNewDay()
	{
		FishSpawnedToday = SpawnedFish.Count;
	}

	public void AddFishSpawned(Interaction_Fishing.FishType spawnedFish)
	{
		SpawnedFish.Add(spawnedFish);
		FishSpawnedToday++;
	}

	public void FishCaught(Interaction_Fishing.FishType spawnedFish)
	{
		SpawnedFish.Add(spawnedFish);
	}
}
