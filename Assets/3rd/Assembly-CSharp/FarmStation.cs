using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FarmStation : Interaction
{
	public static Vector3 Centre = new Vector3(0f, 0f);

	public SpriteRenderer RangeSprite;

	public static List<FarmStation> FarmStations = new List<FarmStation>();

	public Structure Structure;

	private Structures_FarmerStation _StructureInfo;

	public GameObject WorshipperPosition;

	private LayerMask playerMask;

	private string sRequireLevel2;

	private string sMoreActions;

	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	private float DistanceRadius = 1f;

	private float Distance = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	private Vector3 _updatePos;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_FarmerStation StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_FarmerStation;
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
		Interactable = false;
		HasSecondaryInteraction = true;
		RangeSprite.size = new Vector2(6f, 6f);
		playerMask = (int)playerMask | (1 << LayerMask.NameToLayer("Player"));
		FrameIntervalOffset = Random.Range(0, UpdateInterval);
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		FarmStations.Add(this);
		if (GetComponentInParent<PlacementObject>() == null)
		{
			RangeSprite.DOColor(FadeOut, 0f).SetUpdate(true);
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		FarmStations.Remove(this);
	}

	public override void GetLabel()
	{
		base.Label = "";
		Interactable = false;
	}

	public override void GetSecondaryLabel()
	{
		base.SecondaryLabel = "";
		SecondaryInteractable = false;
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
				DistanceRadius = 6f;
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

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
	}
}
