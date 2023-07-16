using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using UnityEngine;

public class Grave : Interaction
{
	public static List<Grave> Graves = new List<Grave>();

	public Structure Structure;

	private Structures_Grave _StructureInfo;

	public GameObject EmptyGraveGameObject;

	public GameObject FullGraveGameObject;

	public GameObject Flowers;

	public SpriteXPBar XPBar;

	private string sString;

	public bool PlayerGotBody;

	private bool Activating;

	private float DistanceToTriggerDeposits = 3f;

	private float Delay;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Grave structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Grave;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public override void OnEnableInteraction()
	{
		XPBar.gameObject.SetActive(false);
		Graves.Add(this);
		SetGameObjects();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(UpdateStructure));
		HasThirdInteraction = true;
		ThirdInteractable = true;
		if (StructureInfo != null && FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID) != null && FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID).HadFuneral)
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
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Type == StructureBrain.TYPES.GRAVE2)
		{
			structureBrain.UpgradedGrave = true;
		}
		if (StructureInfo.FollowerID != -1 && FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID) == null)
		{
			StructureInfo.FollowerID = -1;
		}
		if (FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID) != null && FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID).HadFuneral)
		{
			Structures_Grave structures_Grave = structureBrain;
			structures_Grave.OnSoulsGained = (Action<int>)Delegate.Combine(structures_Grave.OnSoulsGained, new Action<int>(OnSoulsGained));
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

	private void OnSoulsGained(int count)
	{
		UpdateBar();
	}

	private void OnStructureRemoved(StructuresData structure)
	{
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
	}

	public override void OnDisableInteraction()
	{
		Graves.Remove(this);
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(UpdateStructure));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (structureBrain != null)
		{
			Structures_Grave structures_Grave = structureBrain;
			structures_Grave.OnSoulsGained = (Action<int>)Delegate.Remove(structures_Grave.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
	}

	private void Start()
	{
		SetGameObjects();
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.HereLies;
	}

	public void SetGameObjects()
	{
		if (StructureInfo == null)
		{
			return;
		}
		if (StructureInfo.FollowerID == -1 || FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID) == null)
		{
			EmptyGraveGameObject.SetActive(true);
			FullGraveGameObject.SetActive(false);
			XPBar.gameObject.SetActive(false);
			return;
		}
		EmptyGraveGameObject.SetActive(false);
		FullGraveGameObject.SetActive(true);
		if (FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID) != null && FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID).HadFuneral)
		{
			if (!Flowers.activeSelf)
			{
				Flowers.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
			}
			Flowers.SetActive(true);
		}
		else
		{
			Flowers.SetActive(false);
		}
	}

	public override void GetLabel()
	{
		Interactable = false;
		SecondaryInteractable = false;
		HasSecondaryInteraction = false;
		HasThirdInteraction = false;
		ThirdInteractable = false;
		if (Activating)
		{
			base.Label = string.Empty;
			return;
		}
		if (StructureInfo.FollowerID == -1)
		{
			if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Idle_CarryingBody || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Moving_CarryingBody)
			{
				base.Label = ScriptLocalization.Interactions.BuryBody;
				PlayerGotBody = true;
				return;
			}
			base.Label = string.Empty;
			base.SecondaryLabel = string.Empty;
			base.ThirdLabel = string.Empty;
			PlayerGotBody = false;
			return;
		}
		FollowerInfo deadFollowerInfoByID = FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID);
		if (deadFollowerInfoByID != null)
		{
			base.Label = "";
			base.ThirdLabel = LocalizationManager.GetTranslation("Interactions/DigGrave");
			HasThirdInteraction = true;
			ThirdInteractable = true;
			if (deadFollowerInfoByID.HadFuneral)
			{
				XPBar.gameObject.SetActive(true);
				string text = ((GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten) ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
				string receiveDevotion = ScriptLocalization.Interactions.ReceiveDevotion;
				base.SecondaryLabel = receiveDevotion + " " + text + " " + _StructureInfo.SoulCount + StaticColors.GreyColorHex + " / " + structureBrain.SoulMax;
				SecondaryInteractable = true;
				HasSecondaryInteraction = true;
			}
		}
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		FollowerInfo deadFollowerInfoByID = FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID);
		if (deadFollowerInfoByID == null)
		{
			MonoSingleton<Indicator>.Instance.ShowTopInfo(string.Empty);
		}
		else
		{
			MonoSingleton<Indicator>.Instance.ShowTopInfo(HereLiesText(deadFollowerInfoByID));
		}
	}

	private string HereLiesText(FollowerInfo f)
	{
		return "<sprite name=\"img_SwirleyLeft\"> " + sString + " <color=yellow>" + f.Name + "</color> <sprite name=\"img_SwirleyRight\">";
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		MonoSingleton<Indicator>.Instance.HideTopInfo();
	}

	public override void OnThirdInteract(StateMachine state)
	{
		base.OnThirdInteract(state);
		if (!Activating)
		{
			StartCoroutine(DigUpBodyIE());
		}
	}

	private IEnumerator DigUpBodyIE()
	{
		Activating = true;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
		PlayerFarming.Instance.GoToAndStop(base.transform.position);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "actions/dig", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(0.5835f);
		CameraManager.shakeCamera(1f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
		SpawnDeadBody();
		CultFaithManager.AddThought((TimeManager.CurrentPhase == DayPhase.Night) ? Thought.Cult_DigUpBody_Night : Thought.Cult_DigUpBody_Day, -1, 1f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		Activating = false;
	}

	public void SpawnDeadBody()
	{
		BiomeConstants.Instance.EmitSmokeInteractionVFX(base.transform.position, Vector3.one);
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		infoByType.BodyWrapped = true;
		infoByType.FollowerID = StructureInfo.FollowerID;
		StructureManager.BuildStructure(FollowerLocation.Base, infoByType, base.transform.position, Vector2Int.one, false, delegate(GameObject g)
		{
			DeadWorshipper component = g.GetComponent<DeadWorshipper>();
			component.WrapBody();
			int mask = LayerMask.GetMask("Island");
			Collider2D collider2D = Physics2D.OverlapCircle(base.transform.position, 1f, mask);
			if ((bool)collider2D)
			{
				component.BounceOutFromPosition(UnityEngine.Random.Range(3f, 5f), (collider2D.transform.position - base.transform.position).normalized);
			}
			else
			{
				component.BounceOutFromPosition(UnityEngine.Random.Range(3f, 5f));
			}
			PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(component.transform.position);
			if (closestTileGridTileAtWorldPosition != null && closestTileGridTileAtWorldPosition.CanPlaceStructure)
			{
				component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
			}
		});
		StructureInfo.FollowerID = -1;
		SetGameObjects();
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

	public void SpawnZombie(FollowerInfo deadBody)
	{
		if (deadBody != null)
		{
			StartCoroutine(SpawnZombieIE(deadBody));
		}
	}

	private IEnumerator SpawnZombieIE(FollowerInfo f)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
		FollowerBrain resurrectingFollower = FollowerBrain.GetOrCreateBrain(f);
		resurrectingFollower.ResetStats();
		resurrectingFollower.HardSwapToTask(new FollowerTask_ManualControl());
		resurrectingFollower.Location = FollowerLocation.Base;
		resurrectingFollower.DesiredLocation = FollowerLocation.Base;
		resurrectingFollower.LastPosition = StructureInfo.Position;
		resurrectingFollower.CurrentTask.Arrive();
		Follower revivedFollower = FollowerManager.CreateNewFollower(resurrectingFollower._directInfoAccess, StructureInfo.Position);
		revivedFollower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		revivedFollower.Spine.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		revivedFollower.Spine.gameObject.SetActive(true);
		revivedFollower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
		revivedFollower.SetBodyAnimation("Sermons/sermon-heal", false);
		revivedFollower.AddBodyAnimation("Insane/be-weird", false, 0f);
		revivedFollower.AddBodyAnimation("Insane/idle-insane", true, 0f);
		StructureInfo.FollowerID = -1;
		SetGameObjects();
		yield return new WaitForSeconds(4.5f);
		revivedFollower.Brain.ApplyCurseState(Thought.Zombie);
		resurrectingFollower.HardSwapToTask(new FollowerTask_Zombie());
		yield return new WaitForSeconds(2f);
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Zombies))
		{
			MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Zombies);
		}
	}
}
