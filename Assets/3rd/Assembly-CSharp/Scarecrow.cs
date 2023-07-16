using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Scarecrow : Interaction
{
	private Structures_Scarecrow _StructureInfo;

	public static Vector3 Centre = new Vector3(0f, 0.75f);

	public SpriteRenderer RangeSprite;

	private static List<Scarecrow> Scarecrows = new List<Scarecrow>();

	private Structure Structure;

	private LayerMask playerMask;

	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	private float DistanceRadius = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	private Vector3 _updatePos;

	private string sOpenTrap;

	public GameObject TrapOpen;

	public GameObject TrapShut;

	private bool InBirdRoutine;

	public GameObject Bird;

	public Structures_Scarecrow Brain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Scarecrow;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public static float EFFECTIVE_DISTANCE(StructureBrain.TYPES Type)
	{
		if (Type == StructureBrain.TYPES.SCARECROW || Type != StructureBrain.TYPES.SCARECROW_2)
		{
			return 9f;
		}
		return 11f;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (GetComponentInParent<PlacementObject>() == null)
		{
			RangeSprite.DOColor(FadeOut, 0f).SetUpdate(true);
		}
		Scarecrows.Add(this);
		Structure = GetComponentInChildren<Structure>();
		RangeSprite.size = new Vector2(EFFECTIVE_DISTANCE(Structure.Type), EFFECTIVE_DISTANCE(Structure.Type));
		HasSecondaryInteraction = false;
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		HoldToInteract = true;
		if (Bird != null)
		{
			Bird.SetActive(false);
		}
	}

	private void OnBrainAssigned()
	{
		if (Brain.HasBird)
		{
			ShutTrap();
		}
		Structures_Scarecrow brain = Brain;
		brain.OnCatchBird = (Action)Delegate.Combine(brain.OnCatchBird, new Action(CatchBird));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Scarecrows.Remove(this);
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		if (Brain != null)
		{
			Structures_Scarecrow brain = Brain;
			brain.OnCatchBird = (Action)Delegate.Remove(brain.OnCatchBird, new Action(CatchBird));
		}
	}

	public override void GetLabel()
	{
		if (Brain != null)
		{
			if (Brain.HasBird && !InBirdRoutine)
			{
				Interactable = true;
				base.Label = sOpenTrap;
			}
			else
			{
				Interactable = false;
				base.Label = "";
			}
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Brain.HasBird)
		{
			base.OnInteract(state);
			OpenTrap();
			Brain.EmptyTrap();
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, 1, base.transform.position);
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		playerMask = (int)playerMask | (1 << LayerMask.NameToLayer("Player"));
	}

	protected override void Update()
	{
		base.Update();
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
				DistanceRadius = EFFECTIVE_DISTANCE(Structure.Type);
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

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sOpenTrap = ScriptLocalization.Interactions.OpenTrap;
	}

	public void ShutTrap()
	{
		Debug.Log("SET TRAP!");
		TrapOpen.SetActive(false);
		TrapShut.SetActive(true);
		TrapShut.transform.DOPunchScale(new Vector3(0.1f, 0.3f), 0.5f);
	}

	public void OpenTrap()
	{
		TrapOpen.SetActive(true);
		TrapShut.SetActive(false);
		TrapOpen.transform.DOPunchScale(new Vector3(0.3f, 0.1f), 0.5f);
	}

	private void CatchBird()
	{
		StartCoroutine(BirdRoutine());
	}

	private IEnumerator BirdRoutine()
	{
		InBirdRoutine = true;
		Bird.SetActive(true);
		Bird.GetComponentInChildren<Animator>().SetTrigger("FLY");
		float num = UnityEngine.Random.Range(0, 360);
		Bird.transform.localScale = new Vector3((num < 90f && num > -90f) ? 1 : (-1), 1f, 1f);
		float num2 = UnityEngine.Random.Range(8, 10);
		Bird.transform.localPosition = new Vector3(num2 * Mathf.Cos(num * ((float)Math.PI / 180f)), num2 * Mathf.Cos(num * ((float)Math.PI / 180f)), -10f);
		Bird.transform.DOLocalMove(new Vector3(0f, 0.25f), 2f);
		yield return new WaitForSeconds(2f);
		ShutTrap();
		Bird.SetActive(false);
		InBirdRoutine = false;
	}
}
