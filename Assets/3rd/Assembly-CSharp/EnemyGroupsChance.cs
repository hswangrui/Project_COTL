using System.Collections.Generic;

public class EnemyGroupsChance : BaseMonoBehaviour
{
	public List<GroupProbability> Groups;

	private void OnEnable()
	{
	}

	public void InitEnemies()
	{
		int num = -1;
		if (RoomManager.r.EnemyChoice == -1)
		{
			int[] array = new int[Groups.Count];
			num = -1;
			while (++num < Groups.Count)
			{
				array[num] = Groups[num].Probability;
			}
			RoomManager.r.EnemyChoice = Utils.GetRandomWeightedIndex(array);
		}
		num = -1;
		while (++num < Groups.Count)
		{
			Groups[num].GroupObject.SetActive(false);
		}
		Groups[RoomManager.r.EnemyChoice].GroupObject.SetActive(true);
	}

	public void GetGroups()
	{
		Groups = new List<GroupProbability>();
		int num = -1;
		while (++num < base.transform.childCount)
		{
			Groups.Add(new GroupProbability(base.transform.GetChild(num).gameObject));
			base.transform.GetChild(num).name = "Enemies " + (num + 1);
		}
	}

	public void ResetWeights()
	{
		foreach (GroupProbability group in Groups)
		{
			group.Probability = 50;
		}
	}
}
