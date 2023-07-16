using System;
using I2.Loc;
using UnityEngine;

public class Interaction_Outpost : Interaction
{
	public Structure Structure;

	public GameObject ReceiveSoulPosition;

	public SpriteRenderer ProgressBar;

	private string assignString;

	private string collectString;

	private bool _hasFollowers;

	private bool _hasSouls;

	private GameObject Player;

	private bool CollectingSouls;

	private float Delay;

	public float DistanceToTriggerDeposits = 5f;

	public Structures_Shrine StructureBrain
	{
		get
		{
			return Structure.Brain as Structures_Shrine;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		Structure.Structure_Info.CanBeMoved = false;
		Structure.Structure_Info.CanBeRecycled = false;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		assignString = "Assign Follower";
		collectString = ScriptLocalization.Interactions.ReceiveDevotion;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		if (StructureBrain != null)
		{
			Structures_Shrine structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Remove(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
	}

	private void OnStructuresPlaced()
	{
		Structures_Shrine structureBrain = StructureBrain;
		structureBrain.OnSoulsGained = (Action<int>)Delegate.Combine(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		UpdateBar();
	}

	private void OnSoulsGained(int count)
	{
		UpdateBar();
	}

	private void UpdateBar()
	{
		float x = Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
		ProgressBar.transform.localScale = new Vector3(x, ProgressBar.transform.localScale.y);
	}

	public override void GetLabel()
	{
		_hasFollowers = HasFollowingFollowers();
		_hasSouls = StructureBrain.SoulCount > 0;
		Interactable = _hasFollowers || _hasSouls;
		if (_hasFollowers)
		{
			base.Label = assignString;
		}
		else if (_hasSouls)
		{
			base.Label = string.Format("{0} {1} x{2}/{3}", collectString, "<sprite name=\"icon_spirits\">", StructureBrain.SoulCount, StructureBrain.SoulMax);
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (_hasFollowers)
		{
			base.OnInteract(state);
			{
				foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
				{
					if (allBrain.FollowingPlayer)
					{
						allBrain.SetNewHomeLocation(StructureBrain.Data.Location);
						allBrain.FollowingPlayer = false;
						allBrain.CompleteCurrentTask();
					}
				}
				return;
			}
		}
		if (_hasSouls && !CollectingSouls)
		{
			base.OnInteract(state);
			CollectingSouls = true;
		}
	}

	private bool HasFollowingFollowers()
	{
		bool result = false;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.FollowingPlayer)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private new void Update()
	{
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		GetLabel();
		if (CollectingSouls && (StructureBrain.SoulCount <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
		{
			CollectingSouls = false;
		}
		if ((Delay -= Time.deltaTime) < 0f && CollectingSouls)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			Structures_Shrine structureBrain = StructureBrain;
			int soulCount = structureBrain.SoulCount - 1;
			structureBrain.SoulCount = soulCount;
			Delay = 0.2f;
			UpdateBar();
		}
	}

	private void GivePlayerSoul()
	{
		PlayerFarming.Instance.GetSoul(1);
	}
}
