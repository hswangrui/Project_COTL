using System.Collections.Generic;
using UnityEngine;

public class Prison : BaseMonoBehaviour
{
	public static List<Prison> Prisons = new List<Prison>();

	public Structure Structure;

	public Transform PrisonerLocation;

	public GameObject PrisonerExitLocation;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	private void OnEnable()
	{
		Prisons.Add(this);
	}

	private void OnDisable()
	{
		Prisons.Remove(this);
	}

	public static bool HasAvailablePrisons()
	{
		foreach (Prison prison in Prisons)
		{
			if (prison.StructureInfo.FollowerID == -1)
			{
				return true;
			}
		}
		return false;
	}

	public static List<Follower> ImprisonableFollowers()
	{
		List<Follower> list = new List<Follower>(FollowerManager.FollowersAtLocation(FollowerLocation.Base));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			FollowerBrainInfo info = list[num].Brain.Info;
			int iD = info.ID;
			if (DataManager.Instance.Followers_Imprisoned_IDs.Contains(iD) || DataManager.Instance.Followers_OnMissionary_IDs.Contains(iD) || DataManager.Instance.Followers_Demons_IDs.Contains(iD) || info.CursedState == Thought.OldAge)
			{
				list.RemoveAt(num);
			}
		}
		list.Sort((Follower a, Follower b) => b.Brain.HasThought(Thought.Dissenter).CompareTo(a.Brain.HasThought(Thought.Dissenter)));
		return list;
	}
}
