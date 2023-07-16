using System;
using I2.Loc;
using UnityEngine;

public class Interaction_RatauShrine : Interaction
{
	public DevotionCounterOverlay devotionCounterOverlay;

	public GameObject ReceiveSoulPosition;

	public Structure Structure;

	public SpriteXPBar XPBar;

	private string sString;

	private GameObject Player;

	private bool Activating;

	private float Delay;

	private float Distance;

	public float DistanceToTriggerDeposits = 5f;

	public Structures_Shrine_Ratau StructureBrain
	{
		get
		{
			return Structure.Brain as Structures_Shrine_Ratau;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		ContinuouslyHold = true;
	}

	public override void OnEnableInteraction()
	{
		DataManager.Instance.ShrineLevel = 1;
		base.OnEnableInteraction();
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		UpdateBar();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnStructuresPlaced()
	{
		UpdateBar();
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
	}

	private void OnBrainAssigned()
	{
		Debug.Log("StructureBrain.Data.LastPrayTime: " + StructureBrain.Data.LastPrayTime);
		if (StructureBrain.Data.LastPrayTime == -1f)
		{
			StructureBrain.SoulCount = StructureBrain.SoulMax;
			StructureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + StructureBrain.TimeBetweenSouls;
		}
		Structures_Shrine_Ratau structureBrain = StructureBrain;
		structureBrain.OnSoulsGained = (Action<int>)Delegate.Combine(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		UpdateBar();
	}

	private new void OnDestroy()
	{
		if (StructureBrain != null)
		{
			Structures_Shrine_Ratau structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Remove(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
	}

	public override void GetLabel()
	{
		if (StructureBrain == null)
		{
			Debug.Log("STRUCTRUE BRAIN IS NULL!");
			base.Label = "";
			return;
		}
		string text = ((GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten) ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
		Interactable = StructureBrain.SoulCount > 0;
		base.Label = sString + " " + text + " " + StructureBrain.SoulCount + StaticColors.GreyColorHex + " / " + StructureBrain.SoulMax;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			Activating = true;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.ReceiveDevotion;
	}

	private void OnSoulsGained(int count)
	{
		UpdateBar();
	}

	private void UpdateBar()
	{
		if (!(XPBar == null) && StructureBrain != null)
		{
			float value = Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
			XPBar.UpdateBar(value);
		}
	}

	private new void Update()
	{
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		GetLabel();
		Distance = Vector3.Distance(base.transform.position, Player.transform.position);
		if (Activating && (StructureBrain.SoulCount <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Distance > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			Structures_Shrine_Ratau structureBrain = StructureBrain;
			int soulCount = structureBrain.SoulCount - 1;
			structureBrain.SoulCount = soulCount;
			Delay = 0.2f;
			UpdateBar();
		}
		if (StructureBrain != null && StructureBrain.Data.LastPrayTime != -1f && TimeManager.TotalElapsedGameTime > StructureBrain.Data.LastPrayTime && StructureBrain.SoulCount < StructureBrain.SoulMax)
		{
			base.HasChanged = true;
			int num = 1;
			float num2 = TimeManager.TotalElapsedGameTime - StructureBrain.Data.LastPrayTime;
			num += Mathf.FloorToInt(num2 / StructureBrain.TimeBetweenSouls);
			StructureBrain.SoulCount = Mathf.Clamp(StructureBrain.SoulCount + num, 0, StructureBrain.SoulMax);
			StructureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + StructureBrain.TimeBetweenSouls;
		}
	}

	private void GivePlayerSoul()
	{
		PlayerFarming.Instance.GetSoul(1);
	}
}
