using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Structures_BuildSiteProject : StructureBrain, ITaskProvider
{
	public Action OnBuildProgressChanged;

	public Action OnBuildComplete;

	public int UsedSlotCount;

	public int AvailableSlotCount
	{
		get
		{
			return TotalSlotCount - UsedSlotCount;
		}
	}

	public int TotalSlotCount
	{
		get
		{
			return Data.Bounds.x * Data.Bounds.y;
		}
	}

	public float BuildProgress
	{
		get
		{
			return Data.Progress;
		}
		set
		{
			if (ProgressFinished)
			{
				return;
			}
			Data.Progress = value;
			Action onBuildProgressChanged = OnBuildProgressChanged;
			if (onBuildProgressChanged != null)
			{
				onBuildProgressChanged();
			}
			if (ProgressFinished)
			{
				Build();
				Action onBuildComplete = OnBuildComplete;
				if (onBuildComplete != null)
				{
					onBuildComplete();
				}
			}
		}
	}

	public bool ProgressFinished
	{
		get
		{
			return BuildProgress >= (float)StructuresData.BuildDurationGameMinutes(Data.ToBuildType);
		}
	}

	public override void Init(StructuresData data)
	{
		base.Init(data);
		Structures_PlacementRegion structures_PlacementRegion = FindPlacementRegion();
		if (structures_PlacementRegion != null)
		{
			structures_PlacementRegion.MarkObstructionsForClearing(Data.GridTilePosition, Data.Bounds);
		}
	}

	private void Build()
	{
		if (StructuresData.GetCategory(Data.ToBuildType) == Categories.AESTHETIC)
		{
			if (!DataManager.Instance.HasBuiltDecoration(Data.ToBuildType))
			{
				if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.FalseIdols))
				{
					CultFaithManager.AddThought(Thought.Cult_FalseIdols_Trait, -1, 1f);
				}
				else
				{
					CultFaithManager.AddThought(Thought.Cult_NewDecoration, -1, 1f);
				}
			}
			DataManager.Instance.SetBuiltDecoration(Data.ToBuildType);
		}
		else
		{
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.ConstructionEnthusiast))
			{
				CultFaithManager.AddThought(Thought.Cult_ConstructionEnthusiast_Trait, -1, 1f);
			}
			else if (Data.ToBuildType != TYPES.FARM_PLOT)
			{
				CultFaithManager.AddThought(Thought.Cult_NewBuilding, -1, 1f);
			}
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain.Location == Data.Location && Data.ToBuildType != TYPES.FARM_PLOT && Data.ToBuildType != TYPES.PROPAGANDA_SPEAKER)
				{
					if (allBrain.HasTrait(FollowerTrait.TraitType.ConstructionEnthusiast))
					{
						allBrain.AddThought(Thought.CultHasNewBuildingConstructionEnthusiast);
					}
					else
					{
						allBrain.AddThought(Thought.CultHasNewBuilding);
					}
				}
			}
		}
		AudioManager.Instance.PlayOneShot(StructuresData.GetBuildSfx(Data.ToBuildType), Data.Position);
		StructuresData infoByType = StructuresData.GetInfoByType(Data.ToBuildType, 0);
		infoByType.Direction = Data.Direction;
		infoByType.GridTilePosition = Data.GridTilePosition;
		infoByType.PlacementRegionPosition = Data.PlacementRegionPosition;
		infoByType.FollowerID = Data.FollowerID;
		StructureManager.BuildStructure(Data.Location, infoByType, Data.Position, Data.Bounds);
		StructureManager.RemoveStructure(this);
	}

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return new FollowerTask_BuildProject(Data.ID);
	}

	public bool CheckOverrideComplete()
	{
		return ProgressFinished;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity == ScheduledActivity.Work && !ProgressFinished)
		{
			for (int i = 0; i < AvailableSlotCount; i++)
			{
				FollowerTask_BuildProject followerTask_BuildProject = new FollowerTask_BuildProject(Data.ID);
				tasks.Add(followerTask_BuildProject.Priorty, followerTask_BuildProject);
			}
		}
	}

	public override void ToDebugString(StringBuilder sb)
	{
		base.ToDebugString(sb);
		sb.AppendLine(string.Format("ToBuild: {0}, Slots: {1}/{2}", Data.ToBuildType, UsedSlotCount, TotalSlotCount));
	}

	public void MarkObstructionsForClearing(Vector2Int GridPosition, Vector2Int Bounds, bool Prioritised)
	{
		Structures_PlacementRegion structures_PlacementRegion = FindPlacementRegion();
		int num = -1;
		while (++num < Bounds.x)
		{
			int num2 = -1;
			while (++num2 < Bounds.y)
			{
				Vector2Int position = new Vector2Int(GridPosition.x + num, GridPosition.y + num2);
				PlacementRegion.TileGridTile tileGridTile = structures_PlacementRegion.GetTileGridTile(position);
				if (tileGridTile != null && tileGridTile.Obstructed)
				{
					StructuresData obstructionAtPosition = structures_PlacementRegion.GetObstructionAtPosition(position);
					if (obstructionAtPosition != null)
					{
						obstructionAtPosition.Prioritised = true;
					}
					else
					{
						tileGridTile.Obstructed = false;
					}
				}
			}
		}
	}
}
