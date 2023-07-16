using System.Collections.Generic;
using UnityEngine;

public class Structures_FarmerStation : StructureBrain, ITaskProvider
{
	public const float MAX_PLOT_DISTANCE = 6f;

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return null;
	}

	public bool CheckOverrideComplete()
	{
		return true;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity == ScheduledActivity.Work && !ReservedForTask)
		{
			if (GetNextUnseededPlot() != null)
			{
				FollowerTask_Farm followerTask_Farm = new FollowerTask_Farm(Data.ID);
				tasks.Add(followerTask_Farm.Priorty, followerTask_Farm);
			}
			else if (GetNextUnwateredPlot() != null)
			{
				FollowerTask_Farm followerTask_Farm2 = new FollowerTask_Farm(Data.ID);
				tasks.Add(followerTask_Farm2.Priorty, followerTask_Farm2);
			}
			else if (GetNextUnfertilizedPlot() != null && Structures_SiloFertiliser.GetClosestFertiliser(Data.Position, Data.Location) != null)
			{
				FollowerTask_Farm followerTask_Farm3 = new FollowerTask_Farm(Data.ID);
				tasks.Add(followerTask_Farm3.Priorty, followerTask_Farm3);
			}
			else if (GetNextUnpickedPlot() != null && Data.Type == TYPES.FARM_STATION_II)
			{
				FollowerTask_Farm followerTask_Farm4 = new FollowerTask_Farm(Data.ID);
				tasks.Add(followerTask_Farm4.Priorty, followerTask_Farm4);
			}
		}
	}

	public Structures_FarmerPlot GetNextUnwateredPlot()
	{
		Structures_FarmerPlot result = null;
		float num = 6f;
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		boxCollider2D.size = Vector2.one * 6f;
		boxCollider2D.transform.position = Data.Position + Vector3.up * 0.7f;
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (Structures_FarmerPlot allUnwateredPlot in StructureManager.GetAllUnwateredPlots(Data.Location))
		{
			float num2 = Vector3.Distance(Data.Position, allUnwateredPlot.Data.Position);
			Vector3 position = allUnwateredPlot.Data.Position;
			if (num2 < num && boxCollider2D.OverlapPoint(position))
			{
				result = allUnwateredPlot;
				num = num2;
			}
		}
		return result;
	}

	public Structures_FarmerPlot GetNextUnseededPlot()
	{
		Structures_FarmerPlot result = null;
		float num = 6f;
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		List<Structures_FarmerPlot> allUnseededPlots = StructureManager.GetAllUnseededPlots(Data.Location);
		boxCollider2D.size = Vector2.one * 6f;
		boxCollider2D.transform.position = Data.Position + Vector3.up * 0.7f;
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (Structures_FarmerPlot item in allUnseededPlots)
		{
			float num2 = Vector3.Distance(Data.Position, item.Data.Position);
			Vector3 position = item.Data.Position;
			if (num2 < num && boxCollider2D.OverlapPoint(position))
			{
				result = item;
				num = num2;
			}
		}
		return result;
	}

	public Structures_FarmerPlot GetNextUnfertilizedPlot()
	{
		Structures_FarmerPlot result = null;
		float num = 6f;
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		boxCollider2D.size = Vector2.one * 6f;
		boxCollider2D.transform.position = Data.Position + Vector3.up * 0.7f;
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (Structures_FarmerPlot allUnfertilizedPlot in StructureManager.GetAllUnfertilizedPlots(Data.Location))
		{
			float num2 = Vector3.Distance(Data.Position, allUnfertilizedPlot.Data.Position);
			Vector3 position = allUnfertilizedPlot.Data.Position;
			if (num2 < num && boxCollider2D.OverlapPoint(position))
			{
				result = allUnfertilizedPlot;
				num = num2;
			}
		}
		return result;
	}

	public List<Structures_BerryBush> GetCropsAtPosition(Vector3 position)
	{
		List<Structures_BerryBush> list = new List<Structures_BerryBush>();
		foreach (Structures_BerryBush allUnpickedPlot in StructureManager.GetAllUnpickedPlots(Data.Location))
		{
			if (Vector3.Distance(allUnpickedPlot.Data.Position, position) < 0.5f && !allUnpickedPlot.BerryPicked)
			{
				list.Add(allUnpickedPlot);
			}
		}
		return list;
	}

	public Structures_BerryBush GetNextUnpickedPlot()
	{
		Structures_BerryBush result = null;
		float num = 6f;
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		boxCollider2D.size = Vector2.one * 6f;
		boxCollider2D.transform.position = Data.Position + Vector3.up * 0.7f;
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (Structures_BerryBush allUnpickedPlot in StructureManager.GetAllUnpickedPlots(Data.Location))
		{
			if (PlayerFarming.Location == FollowerLocation.Base)
			{
				bool flag = false;
				foreach (FarmPlot farmPlot in FarmPlot.FarmPlots)
				{
					if (!(farmPlot._activeCropController != null) || farmPlot.StructureBrain.Data.ID != allUnpickedPlot.CropID)
					{
						continue;
					}
					bool flag2 = false;
					foreach (GameObject cropState in farmPlot._activeCropController.CropStates)
					{
						if (cropState.activeSelf)
						{
							flag2 = true;
						}
					}
					if (farmPlot._activeCropController.BumperCropObject.activeSelf)
					{
						flag2 = true;
					}
					if (flag2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			float num2 = Vector3.Distance(Data.Position, allUnpickedPlot.Data.Position);
			Vector3 position = allUnpickedPlot.Data.Position;
			if (num2 < num && boxCollider2D.OverlapPoint(position))
			{
				result = allUnpickedPlot;
				num = num2;
			}
		}
		return result;
	}
}
