using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using src.UI.Menus.CryptMenu;
using UnityEngine;

public class Interaction_Crypt : Interaction
{
	public static List<Interaction_Crypt> Crypts = new List<Interaction_Crypt>();

	public Structure Structure;

	private Structures_Crypt _StructureInfo;

	private string _label;

	private bool activated;

	[SerializeField]
	private GameObject doorOpen;

	[SerializeField]
	private GameObject doorClosed;

	[SerializeField]
	private GameObject holePosition;

	[SerializeField]
	private ItemGauge ItemGauge;

	[SerializeField]
	private GameObject flowers;

	public SpriteXPBar XPBar;

	private bool Activating;

	private int _bodyDirection;

	private List<Vector3> _bodyDirections = new List<Vector3>
	{
		new Vector3(-1f, 0f, 0f),
		new Vector3(-1f, -1f, 0f),
		new Vector3(0f, -1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(1f, -1f, 0f)
	};

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Crypt structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Crypt;
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
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		_bodyDirections.Shuffle();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (structureBrain != null)
		{
			Structures_Crypt structures_Crypt = structureBrain;
			structures_Crypt.OnSoulsGained = (Action<int>)Delegate.Remove(structures_Crypt.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		XPBar.gameObject.SetActive(false);
		Crypts.Add(this);
		UpdateLocalisation();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(UpdateStructure));
		if (_StructureInfo != null && _StructureInfo.FollowersFuneralCount() > 0)
		{
			if (structureBrain != null && structureBrain.Data != null && structureBrain.Data.LastPrayTime == -1f)
			{
				structureBrain.SoulCount = structureBrain.SoulMax;
				structureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + structureBrain.TimeBetweenSouls;
			}
			UpdateBar();
		}
	}

	private void UpdateBar()
	{
		if (!(XPBar == null) && structureBrain != null)
		{
			XPBar.gameObject.SetActive(true);
			float value = Mathf.Clamp((float)structureBrain.SoulCount / (float)structureBrain.SoulMax, 0f, 1f);
			XPBar.UpdateBar(value);
		}
	}

	private void OnBrainAssigned()
	{
		ItemGauge.SetPosition((float)structureBrain.Data.MultipleFollowerIDs.Count / (float)_StructureInfo.DEAD_BODY_SLOTS);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (_StructureInfo != null && _StructureInfo.FollowersFuneralCount() > 0)
		{
			Structures_Crypt structures_Crypt = structureBrain;
			structures_Crypt.OnSoulsGained = (Action<int>)Delegate.Combine(structures_Crypt.OnSoulsGained, new Action<int>(OnSoulsGained));
			if (structureBrain.Data.LastPrayTime == -1f)
			{
				structureBrain.SoulCount = structureBrain.SoulMax;
				structureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + structureBrain.TimeBetweenSouls;
			}
			UpdateBar();
			UpdateStructure();
		}
		SetGameObjects();
	}

	private void UpdateStructure()
	{
		if (structureBrain != null && structureBrain.Data.LastPrayTime != -1f && TimeManager.TotalElapsedGameTime > structureBrain.Data.LastPrayTime && structureBrain.SoulCount < structureBrain.SoulMax)
		{
			base.HasChanged = true;
			int num = 1;
			float num2 = TimeManager.TotalElapsedGameTime - structureBrain.Data.LastPrayTime;
			num += Mathf.FloorToInt(num2 / structureBrain.TimeBetweenSouls);
			structureBrain.SoulCount = Mathf.Clamp(structureBrain.SoulCount + num, 0, structureBrain.SoulMax);
			structureBrain.Data.LastPrayTime = TimeManager.TotalElapsedGameTime + structureBrain.TimeBetweenSouls;
		}
	}

	private void OnSoulsGained(int count)
	{
		UpdateBar();
	}

	public void SetGameObjects()
	{
		if (StructureInfo == null)
		{
			return;
		}
		bool flag = false;
		foreach (int multipleFollowerID in structureBrain.Data.MultipleFollowerIDs)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(multipleFollowerID, true);
			if (infoByID != null && infoByID.HadFuneral)
			{
				flag = true;
			}
		}
		XPBar.gameObject.SetActive(false);
		if (flag)
		{
			XPBar.gameObject.SetActive(true);
			if (!flowers.activeSelf)
			{
				flowers.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
			}
			flowers.SetActive(true);
		}
		else
		{
			flowers.SetActive(false);
		}
	}

	public override void OnDisableInteraction()
	{
		Crypts.Remove(this);
		base.OnDisableInteraction();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(UpdateStructure));
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		_label = ScriptLocalization.Interactions.Look;
	}

	public override void GetLabel()
	{
		base.GetLabel();
		SecondaryInteractable = false;
		HasSecondaryInteraction = false;
		base.SecondaryLabel = "";
		if (Activating)
		{
			base.Label = string.Empty;
			return;
		}
		if (structureBrain != null && structureBrain.Data != null && structureBrain.Data.MultipleFollowerIDs.Count <= 0)
		{
			Interactable = false;
			base.Label = LocalizationManager.GetTranslation("Interactions/Crypt/BuryFollowers");
			return;
		}
		Interactable = true;
		base.Label = _label + " (" + structureBrain.Data.MultipleFollowerIDs.Count + "/" + structureBrain.DEAD_BODY_SLOTS + ")";
		if (_StructureInfo.FollowersFuneralCount() > 0)
		{
			XPBar.gameObject.SetActive(true);
			string text = ((GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten) ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
			string receiveDevotion = ScriptLocalization.Interactions.ReceiveDevotion;
			base.SecondaryLabel = receiveDevotion + " " + text + " " + _StructureInfo.SoulCount + StaticColors.GreyColorHex + " / " + structureBrain.SoulMax;
			SecondaryInteractable = true;
			HasSecondaryInteraction = true;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (activated)
		{
			return;
		}
		SetDoors(true);
		activated = true;
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		UICryptMenuController uICryptMenuController = MonoSingleton<UIManager>.Instance.CryptMenuTemplate.Instantiate();
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo item in DataManager.Instance.Followers_Dead)
		{
			if (structureBrain.Data.MultipleFollowerIDs.Contains(item.ID))
			{
				list.Add(item);
			}
		}
		uICryptMenuController.Show(list);
		uICryptMenuController.Configure(this);
		uICryptMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uICryptMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			HUD_Manager.Instance.Show();
			GameManager.GetInstance().OnConversationEnd();
			StructureInfo.MultipleFollowerIDs.Remove(followerInfo.ID);
			SpawnBody(followerInfo);
			activated = false;
			base.HasChanged = true;
		});
		uICryptMenuController.OnHidden = (Action)Delegate.Combine(uICryptMenuController.OnHidden, (Action)delegate
		{
			Time.timeScale = 1f;
			activated = false;
			HUD_Manager.Instance.Show();
			GameManager.GetInstance().OnConversationEnd();
			SetDoors(false);
			base.HasChanged = true;
		});
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		if (structureBrain.SoulCount > 0)
		{
			if (!Activating)
			{
				StartCoroutine(GiveReward());
			}
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator Delay(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public void SetDoors(bool open)
	{
		doorOpen.SetActive(open);
		doorClosed.SetActive(!open);
		ItemGauge.SetPosition((float)structureBrain.Data.MultipleFollowerIDs.Count / (float)_StructureInfo.DEAD_BODY_SLOTS);
		SetGameObjects();
	}

	private void SpawnBody(FollowerInfo body)
	{
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		infoByType.Position = holePosition.transform.position;
		infoByType.BodyWrapped = true;
		infoByType.FollowerID = body.ID;
		StructureManager.BuildStructure(FollowerLocation.Base, infoByType, holePosition.transform.position, Vector2Int.one, false, delegate(GameObject g)
		{
			DeadWorshipper component = g.GetComponent<DeadWorshipper>();
			component.WrapBody();
			int mask = LayerMask.GetMask("Island");
			Collider2D collider2D = Physics2D.OverlapCircle(base.transform.position, 2f, mask);
			if ((bool)collider2D)
			{
				component.BounceOutFromPosition(10f, (collider2D.transform.position - base.transform.position).normalized);
			}
			else
			{
				component.BounceOutFromPosition(10f, _bodyDirections[_bodyDirection]);
			}
			if (_bodyDirection < _bodyDirections.Count - 1)
			{
				_bodyDirection++;
			}
			else
			{
				_bodyDirection = 0;
			}
			PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(component.transform.position);
			if (closestTileGridTileAtWorldPosition != null)
			{
				component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
			}
		});
		SetGameObjects();
	}

	private IEnumerator GiveReward()
	{
		Debug.Log("_StructureInfo.SoulCount: " + _StructureInfo.SoulCount.ToString().Colour(Color.yellow));
		Activating = true;
		for (int i = 0; i < _StructureInfo.SoulCount; i++)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, Color.white, delegate
				{
					PlayerFarming.Instance.GetSoul(1);
				});
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			float value = Mathf.Clamp((float)(_StructureInfo.SoulCount - i) / (float)structureBrain.SoulMax, 0f, 1f);
			XPBar.UpdateBar(value);
			yield return new WaitForSeconds(0.1f);
		}
		_StructureInfo.SoulCount = 0;
		XPBar.UpdateBar(0f);
		Activating = false;
		base.HasChanged = true;
	}
}
