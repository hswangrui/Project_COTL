using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class HarvestTotem : Interaction
{
	public static float EFFECTIVE_DISTANCE = 7f;

	public static Vector3 Centre = new Vector3(0f, 0f);

	public SpriteRenderer RangeSprite;

	private static List<HarvestTotem> HarvestTotems = new List<HarvestTotem>();

	public GameObject ReceiveSoulPosition;

	private Structure Structure;

	public GameObject DevotionReady;

	[SerializeField]
	private SpriteXPBar XpBar;

	private string sString;

	private LayerMask playerMask;

	private GameObject Player;

	private bool Activating;

	private float Delay;

	public float DistanceToTriggerDeposits = 5f;

	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	private float DistanceRadius = 1f;

	private float Distance = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	private Vector3 _updatePos;

	public Structures_HarvestTotem StructureBrain
	{
		get
		{
			return Structure.Brain as Structures_HarvestTotem;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (GetComponentInParent<PlacementObject>() == null)
		{
			RangeSprite.DOColor(FadeOut, 0f).SetUpdate(true);
		}
		HarvestTotems.Add(this);
		Structure = GetComponentInChildren<Structure>();
		DataManager.Instance.ShrineLevel = 1;
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		HarvestTotems.Remove(this);
	}

	private void Start()
	{
		RangeSprite.size = new Vector2(EFFECTIVE_DISTANCE, EFFECTIVE_DISTANCE);
		UpdateLocalisation();
		ContinuouslyHold = true;
		if (XpBar != null)
		{
			XpBar.gameObject.SetActive(false);
		}
		ActivateDistance = 2f;
		playerMask = (int)playerMask | (1 << LayerMask.NameToLayer("Player"));
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Utils.DrawCircleXY(base.transform.position + Centre, EFFECTIVE_DISTANCE, Color.green);
		Utils.DrawCircleXY(base.transform.position + Centre, 0.5f, Color.red);
	}

	private void OnStructuresPlaced()
	{
		UpdateBar();
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
	}

	private void OnBrainAssigned()
	{
		if (Structure.Type != global::StructureBrain.TYPES.HARVEST_TOTEM)
		{
			if (StructureBrain.Data.LastPrayTime == -1f)
			{
				StructureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + StructureBrain.TimeBetweenSouls;
			}
			Structures_HarvestTotem structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Combine(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
			UpdateBar();
		}
	}

	private new void OnDestroy()
	{
		if (Structure != null && StructureBrain != null)
		{
			Structures_HarvestTotem structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Remove(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
	}

	public override void GetLabel()
	{
		if (Structure.Type == global::StructureBrain.TYPES.HARVEST_TOTEM)
		{
			base.Label = "";
			return;
		}
		Interactable = StructureBrain.SoulCount > 0;
		string text = ((GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten) ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
		base.Label = sString + " " + text + " x" + StructureBrain.SoulCount + "/" + StructureBrain.SoulMax;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			IndicateHighlighted();
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
		if (!(XpBar == null) && StructureBrain != null)
		{
			float value = Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
			XpBar.UpdateBar(value);
			if (DevotionReady != null)
			{
				DevotionReady.SetActive(StructureBrain.SoulCount > 0);
			}
		}
	}

	private new void Update()
	{
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		GetLabel();
		if ((Delay -= Time.deltaTime) < 0f && Activating && StructureBrain.SoulCount > 0)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			Structures_HarvestTotem structureBrain = StructureBrain;
			int soulCount = structureBrain.SoulCount - 1;
			structureBrain.SoulCount = soulCount;
			Delay = 0.2f;
			UpdateBar();
		}
		if (StructureBrain != null && StructureBrain.Data.LastPrayTime != -1f && TimeManager.TotalElapsedGameTime > StructureBrain.Data.LastPrayTime && StructureBrain.SoulCount < StructureBrain.SoulMax)
		{
			Debug.Log("ADD to souls count: " + StructureBrain.SoulCount);
			StructureBrain.SoulCount++;
			StructureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + StructureBrain.TimeBetweenSouls;
		}
		if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval == 0 && !(PlayerFarming.Instance == null))
		{
			if (!GameManager.overridePlayerPosition)
			{
				_updatePos = PlayerFarming.Instance.transform.position;
				DistanceRadius = 1f;
			}
			else
			{
				_updatePos = PlacementRegion.Instance.PlacementPosition;
				DistanceRadius = EFFECTIVE_DISTANCE;
			}
			if (Vector3.Distance(_updatePos, base.transform.position) < DistanceRadius)
			{
				RangeSprite.gameObject.SetActive(true);
				RangeSprite.DOKill();
				RangeSprite.DOColor(StaticColors.OffWhiteColor, 0.5f).SetUpdate(true);
				distanceChanged = true;
			}
			else if (distanceChanged)
			{
				RangeSprite.DOKill();
				RangeSprite.DOColor(FadeOut, 0.5f).SetUpdate(true);
				distanceChanged = false;
			}
		}
	}

	public override void IndicateHighlighted()
	{
		XpBar.gameObject.SetActive(true);
	}

	public override void EndIndicateHighlighted()
	{
		XpBar.gameObject.SetActive(false);
	}

	private void GivePlayerSoul()
	{
		PlayerFarming.Instance.GetSoul(1);
	}
}
