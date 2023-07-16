using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_FeastTable : Interaction
{
	public static List<Interaction_FeastTable> FeastTables = new List<Interaction_FeastTable>();

	public Structure Structure;

	private Structures_FeastTable _StructureInfo;

	public AudioClip FeastSong;

	public GameObject Center;

	public GameObject PlayerPosition;

	public GameObject Container;

	public GameObject[] Seats;

	private bool IsEating;

	private float createdMealTimer;

	private float feastingMembersCheckTimer;

	private List<Follower> feasters = new List<Follower>();

	public bool IsFeastActive;

	private GameObject Player;

	private bool GiveThought;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_FeastTable brain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_FeastTable;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ContinuouslyHold = true;
		FeastTables.Add(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain == null)
		{
			Structure.Brain = StructureBrain.GetOrCreateBrain(StructuresData.GetInfoByType(StructureBrain.TYPES.FEAST_TABLE, 0));
			GetFeasters();
		}
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (!StructureInfo.IsGatheringActive)
		{
			StructureInfo.GivenHealth = false;
		}
		GetFeasters();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.ID == StructureInfo.ID)
		{
			StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (StructureInfo.IsGatheringActive)
		{
			if (!IsEating)
			{
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
				GameManager.GetInstance().AddToCamera(base.gameObject);
				StartCoroutine(EatIE());
			}
			else if (IsEating && !PlayerFarming.Instance.GoToAndStopping)
			{
				CancelEat();
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
	}

	private IEnumerator EatIE()
	{
		PlayerFarming.Instance.GoToAndStop(PlayerPosition);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		IsEating = true;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Dancing;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "eat", true);
	}

	private void CancelEat()
	{
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		IsEating = false;
		if (!StructureInfo.GivenHealth)
		{
			StartCoroutine(CancelEatIE());
		}
		StructureInfo.GivenHealth = true;
	}

	private IEnumerator CancelEatIE()
	{
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.health.BlueHearts += 4f;
	}

	private new void OnDestroy()
	{
		FeastTables.Remove(this);
	}

	public override void GetLabel()
	{
		string text = (IsEating ? ScriptLocalization.Interactions.Cancel : ScriptLocalization.Interactions.JoinDance);
		base.Label = (brain.Data.IsGatheringActive ? text : "");
		if (StructureInfo.GivenHealth)
		{
			base.Label = "";
			Interactable = false;
		}
	}

	private new void Update()
	{
		if ((Player = GameObject.FindWithTag("Player")) == null || StructureInfo == null)
		{
			return;
		}
		if (IsEating && !PlayerFarming.Instance.GoToAndStopping && (!StructureInfo.IsGatheringActive || InputManager.General.GetAnyButton()))
		{
			CancelEat();
		}
		if (!IsFeastActive)
		{
			return;
		}
		if (Time.time > createdMealTimer && feasters.Count > 0)
		{
			Follower follower = feasters[UnityEngine.Random.Range(0, feasters.Count)];
			if (follower.Brain.CurrentTask != null && follower.Brain.CurrentTask is FollowerTask_EatFeastTable)
			{
				float num = UnityEngine.Random.Range(0.25f, 0.75f);
				createdMealTimer = Time.time + num;
				ResourceCustomTarget.Create(follower.gameObject, Center.transform.position + Vector3.up * UnityEngine.Random.Range(-1f, 1f), GetRandomMeal(), null);
			}
		}
		else if (feasters.Count == 0)
		{
			GetFeasters();
		}
		GiveThought = true;
	}

	private void GetFeasters()
	{
		feasters.Clear();
		foreach (Follower item in FollowerManager.FollowersAtLocation(FollowerLocation.Church))
		{
			feasters.Add(item);
		}
		foreach (Follower item2 in FollowerManager.FollowersAtLocation(FollowerLocation.Base))
		{
			if (!feasters.Contains(item2) && !FollowerManager.FollowerLocked(item2.Brain.Info.ID))
			{
				feasters.Add(item2);
			}
		}
	}

	private InventoryItem.ITEM_TYPE GetRandomMeal()
	{
		InventoryItem.ITEM_TYPE[] allMeals = CookingData.GetAllMeals();
		return allMeals[UnityEngine.Random.Range(0, allMeals.Length)];
	}

	public Vector3 GetEatPosition(Follower follower)
	{
		GetFeasters();
		int num = feasters.IndexOf(follower);
		if (num != -1 && num < Seats.Length - 1)
		{
			return Seats[num].transform.position;
		}
		Vector3 result = base.transform.position + Vector3.up * 3f;
		result.x += ((UnityEngine.Random.Range(0, 2) == 0) ? UnityEngine.Random.Range(-1f, -1.75f) : UnityEngine.Random.Range(1f, 1.75f));
		result.y += UnityEngine.Random.Range(-3f, 2.5f);
		return result;
	}
}
