using System;
using System.Collections;
using UnityEngine;

public class FirstEntryToBase : BaseMonoBehaviour
{
	public GameObject Rat1Indoctrinate;

	public GameObject Rat1Shrine;

	public GameObject Rat1Dungeon;

	public GameObject Rat2Food;

	public GameObject Rat2Faith;

	public GameObject Rat2BackToDungeon;

	public GameObject Rat3Night;

	private bool ForceFollowersToBuild;

	public GameObject UITutorialPrefab;

	private UIShrineTutorial UIShrineTutorial;

	private void Start()
	{
		HideAll();
		if (!DataManager.Instance.InTutorial)
		{
			Rat1Indoctrinate.SetActive(true);
		}
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (ForceFollowersToBuild && structure.Type == StructureBrain.TYPES.BUILD_SITE && structure.ToBuildType == StructureBrain.TYPES.SHRINE)
		{
			foreach (Follower follower in Follower.Followers)
			{
				follower.Brain.CompleteCurrentTask();
			}
			ForceFollowersToBuild = false;
		}
		if (!DataManager.Instance.Tutorial_ReturnToDungeon && structure.Type == StructureBrain.TYPES.BUILD_SITE && structure.ToBuildType == StructureBrain.TYPES.TEMPLE)
		{
			HideAll();
			Debug.Log("Rat2BackToDungeon " + Rat2BackToDungeon);
			if (Rat2BackToDungeon != null)
			{
				Rat2BackToDungeon.SetActive(true);
			}
			DataManager.Instance.Tutorial_ReturnToDungeon = true;
		}
	}

	private void OnObjectiveComplete(string GroupID)
	{
		switch (GroupID)
		{
		default:
		{
			bool flag = GroupID == "Objectives/GroupTitles/Faith";
			break;
		}
		case "Objectives/GroupTitles/RecruitFollower":
			HideAll();
			DataManager.Instance.AllowBuilding = true;
			DataManager.Instance.InTutorial = true;
			Rat1Shrine.SetActive(true);
			break;
		case "Objectives/GroupTitles/RepairTheShrine":
			CreateFollowers();
			HideAll();
			Rat1Dungeon.SetActive(true);
			break;
		case "Objectives/GroupTitles/GoToDungeon":
			HideAll();
			Rat2Food.SetActive(true);
			ForceFollowersToBuild = true;
			break;
		case "Objectives/GroupTitles/Food":
			HideAll();
			Rat2Faith.SetActive(true);
			break;
		}
	}

	private void OnDisable()
	{
	}

	private void HideAll()
	{
		if (Rat1Shrine != null)
		{
			Rat1Shrine.SetActive(false);
		}
		if (Rat1Indoctrinate != null)
		{
			Rat1Indoctrinate.SetActive(false);
		}
		if (Rat1Dungeon != null)
		{
			Rat1Dungeon.SetActive(false);
		}
		if (Rat2Food != null)
		{
			Rat2Food.SetActive(false);
		}
		if (Rat2Faith != null)
		{
			Rat2Faith.SetActive(false);
		}
		if (Rat2BackToDungeon != null)
		{
			Rat2BackToDungeon.SetActive(false);
		}
		if (Rat3Night != null)
		{
			Rat3Night.SetActive(false);
		}
	}

	public void CreateFollowers()
	{
		FollowerManager.CreateNewRecruit(FollowerLocation.Base, BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position);
	}

	public void PlayShrineTutorial()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(UITutorialPrefab, GameObject.FindWithTag("Canvas").transform);
		UIShrineTutorial = gameObject.GetComponent<UIShrineTutorial>();
	}

	public void ShowXPBar()
	{
		StartCoroutine(ShowXPBarRoutine());
	}

	private IEnumerator ShowXPBarRoutine()
	{
		yield return new WaitForSeconds(1f);
		HUD_Manager.Instance.XPBarTransitions.MoveBackInFunction();
	}
}
