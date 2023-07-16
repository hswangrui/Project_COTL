using System.Collections.Generic;
using UnityEngine;

public class IslandGroup : BaseMonoBehaviour
{
	public List<GroupProbability> Groups;

	private void OnEnable()
	{
		if (RoomManager.Instance != null)
		{
			RoomManager.Instance.OnInitEnemies += InitEnemies;
		}
		else
		{
			InitEnemies();
		}
	}

	private void OnDisable()
	{
		RoomManager.Instance.OnInitEnemies -= InitEnemies;
	}

	private void InitEnemies()
	{
		int num = -1;
		if (RoomManager.r.IslandChoice == -1)
		{
			int[] array = new int[Groups.Count];
			num = -1;
			while (++num < Groups.Count)
			{
				array[num] = Groups[num].Probability;
			}
			RoomManager.r.IslandChoice = Utils.GetRandomWeightedIndex(array);
		}
		num = -1;
		while (++num < Groups.Count)
		{
			Groups[num].GroupObject.SetActive(false);
		}
		Groups[RoomManager.r.IslandChoice].GroupObject.SetActive(true);
		EnemyGroupsChance[] componentsInChildren = Groups[RoomManager.r.IslandChoice].GroupObject.GetComponentsInChildren<EnemyGroupsChance>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].InitEnemies();
		}
	}

	public void GetGroups()
	{
		Groups = new List<GroupProbability>();
		int num = -1;
		while (++num < base.transform.childCount)
		{
			Groups.Add(new GroupProbability(base.transform.GetChild(num).gameObject));
		}
	}

	public void ResetWeights()
	{
		foreach (GroupProbability group in Groups)
		{
			group.Probability = 50;
		}
	}

	public void Organise()
	{
		Transform transform = null;
		int num = -1;
		while (++num < Groups.Count)
		{
			transform = Groups[num].GroupObject.transform;
			Transform transform2 = transform.Find("Islands");
			if (transform2 == null)
			{
				GameObject obj = new GameObject();
				obj.transform.parent = transform;
				obj.name = "Islands";
				transform2 = obj.transform;
			}
			Transform transform3 = transform.Find("Enemies");
			if (transform3 == null)
			{
				GameObject obj2 = new GameObject();
				obj2.transform.parent = transform;
				obj2.name = "Enemies";
				transform3 = obj2.transform;
			}
			Transform transform4 = transform.Find("Traps");
			if (transform4 == null)
			{
				GameObject obj3 = new GameObject();
				obj3.transform.parent = transform;
				obj3.name = "Traps";
				transform4 = obj3.transform;
			}
			Transform transform5 = transform.Find("Resources");
			if (transform5 == null)
			{
				GameObject obj4 = new GameObject();
				obj4.transform.parent = transform;
				obj4.name = "Resources";
				transform5 = obj4.transform;
			}
			Transform transform6 = transform.Find("Blocks");
			if (transform6 == null)
			{
				GameObject obj5 = new GameObject();
				obj5.transform.parent = transform;
				obj5.name = "Blocks";
				transform6 = obj5.transform;
			}
			foreach (Transform item in transform.transform)
			{
				if (item.name.Contains("Trap"))
				{
					item.parent = transform4;
				}
				else if (item.name.Contains("Enemy"))
				{
					item.parent = transform3;
				}
				else if (item.name.Contains("Resource"))
				{
					item.parent = transform5;
				}
				else if (item.name.Contains("Block"))
				{
					item.parent = transform6;
				}
				else if (item.name.Contains("Island"))
				{
					item.parent = transform2;
				}
			}
		}
	}
}
