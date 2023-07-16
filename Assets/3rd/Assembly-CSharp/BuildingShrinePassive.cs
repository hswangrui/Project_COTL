using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class BuildingShrinePassive : Interaction
{
	public static List<BuildingShrinePassive> Shrines = new List<BuildingShrinePassive>();

	public DevotionCounterOverlay devotionCounterOverlay;

	public GameObject ReceiveSoulPosition;

	public Structure Structure;

	[SerializeField]
	private Interaction_AddFuel addFuel;

	[Space]
	[SerializeField]
	private GameObject[] spawnPositions;

	[SerializeField]
	private SpriteRenderer rangeCircle;

	[SerializeField]
	private SpriteXPBar XpBar;

	private string sString;

	[SerializeField]
	private GameObject shrineEyes;

	private Coroutine cShowRangeSprite;

	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	private GameObject Player;

	private bool Activating;

	private float Delay;

	private float Distance;

	private bool InRange;

	private float RangeDistance = 100f;

	public float DistanceToTriggerDeposits = 5f;

	private float DistanceRadius = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	private Vector3 _updatePos;

	public Structures_Shrine_Passive StructureBrain
	{
		get
		{
			return Structure.Brain as Structures_Shrine_Passive;
		}
	}

	public GameObject[] SpawnPositions
	{
		get
		{
			return spawnPositions;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		ContinuouslyHold = true;
		GameObject[] array = spawnPositions;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}

	public override void OnEnableInteraction()
	{
		if (shrineEyes != null)
		{
			shrineEyes.SetActive(false);
		}
		DataManager.Instance.ShrineLevel = 1;
		rangeCircle.DOColor(FadeOut, 0f).SetUpdate(true);
		base.OnEnableInteraction();
		Shrines.Add(this);
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		addFuel.OnFuelModified += OnFuelModified;
	}

	private void OnUpgradeUnlocked(UpgradeSystem.Type upgradeType)
	{
		addFuel.gameObject.SetActive(UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_PassiveShrinesFlames));
	}

	private void OnBrainAssigned()
	{
		Structures_Shrine_Passive structureBrain = StructureBrain;
		structureBrain.OnSoulsGained = (Action<int>)Delegate.Combine(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		UpdateBar();
		OnUpgradeUnlocked(UpgradeSystem.Type.Ability_Eat);
		UpgradeSystem.OnUpgradeUnlocked += OnUpgradeUnlocked;
		RangeDistance = Structures_Shrine_Passive.Range(StructureBrain.Data.Type);
		rangeCircle.size = new Vector2(RangeDistance, RangeDistance);
	}

	public IEnumerator ShowRangeSprite(Color TargetColor, bool Disable)
	{
		rangeCircle.gameObject.SetActive(true);
		rangeCircle.enabled = true;
		float Progress = 0f;
		float Duration = 0.3f;
		Color StartColor = rangeCircle.color;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			rangeCircle.color = Color.Lerp(StartColor, TargetColor, Progress / Duration);
			yield return null;
		}
		rangeCircle.color = TargetColor;
		if (Disable)
		{
			rangeCircle.enabled = false;
		}
	}

	private void OnStructuresPlaced()
	{
		UpdateBar();
		DataManager.Instance.ShrineLevel = 1;
	}

	private void OnFuelModified(float fuel)
	{
		if (Structure.Structure_Info.FullyFueled)
		{
			addFuel.Interactable = false;
		}
		else
		{
			addFuel.Interactable = true;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Shrines.Remove(this);
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		if (StructureBrain != null)
		{
			Structures_Shrine_Passive structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Remove(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
		UpgradeSystem.OnUpgradeUnlocked -= OnUpgradeUnlocked;
		addFuel.OnFuelModified -= OnFuelModified;
	}

	public override void GetLabel()
	{
		Interactable = StructureBrain.SoulCount > 0;
		string text = ((GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten) ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
		base.Label = sString + " " + text + " " + StructureBrain.SoulCount + StaticColors.GreyColorHex + " / " + StructureBrain.SoulMax;
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
		float value = Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
		if (shrineEyes != null)
		{
			if (StructureBrain.SoulCount == StructureBrain.SoulMax)
			{
				shrineEyes.SetActive(true);
			}
			else
			{
				shrineEyes.SetActive(false);
			}
		}
		XpBar.UpdateBar(value);
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
	}

	private new void Update()
	{
		if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval == 0)
		{
			if (PlayerFarming.Instance == null)
			{
				return;
			}
			if (!GameManager.overridePlayerPosition)
			{
				_updatePos = PlayerFarming.Instance.transform.position;
				DistanceRadius = 1f;
			}
			else
			{
				_updatePos = PlacementRegion.Instance.PlacementPosition;
				DistanceRadius = Structures_Shrine_Passive.Range(StructureBrain.Data.Type);
			}
			if (Vector3.Distance(_updatePos, base.transform.position) < DistanceRadius)
			{
				rangeCircle.gameObject.SetActive(true);
				rangeCircle.DOKill();
				rangeCircle.DOColor(StaticColors.OffWhiteColor, 0.5f).SetUpdate(true);
				distanceChanged = true;
			}
			else if (distanceChanged)
			{
				rangeCircle.DOKill();
				rangeCircle.DOColor(FadeOut, 0.5f).SetUpdate(true);
				distanceChanged = false;
			}
		}
		if (Activating && (StructureBrain.SoulCount <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Distance > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(state.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			Structures_Shrine_Passive structureBrain = StructureBrain;
			int soulCount = structureBrain.SoulCount - 1;
			structureBrain.SoulCount = soulCount;
			UpdateBar();
			Delay = 0.1f;
		}
	}

	private void GivePlayerSoul()
	{
		UpdateBar();
		PlayerFarming.Instance.GetSoul(1);
	}
}
