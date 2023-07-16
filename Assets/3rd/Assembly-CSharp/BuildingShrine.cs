using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using Unify;
using UnityEngine;

public class BuildingShrine : Interaction
{
	public static List<BuildingShrine> Shrines = new List<BuildingShrine>();

	public DevotionCounterOverlay devotionCounterOverlay;

	public GameObject ReceiveSoulPosition;

	public Structure Structure;

	private string sString;

	private string sResearch;

	public GameObject ShrineCanLevelUp;

	public GameObject ShrineCantLevelUp;

	public Animator shrineLevelUpAnimator;

	public Animator twitchLevelUpAnimator;

	public SpriteXPBar XPBar;

	[SerializeField]
	private Interaction_AddFuel addFuel;

	[SerializeField]
	private GameObject[] flameLevels;

	[Space]
	[SerializeField]
	private GameObject[] spawnPositions;

	[SerializeField]
	private GameObject godTearPrefab;

	public SpriteRenderer ShrineGlow;

	public ParticleSystem psDevotion;

	private bool hasUnlockAvailable;

	private ParticleSystem.EmissionModule emissionModule;

	private Coroutine cFadeGlowMaterial;

	private bool wasFull;

	private GameObject Player;

	private bool Activating;

	private float Delay;

	public float DistanceToTriggerDeposits = 5f;

	private float ReduceDelay = 0.1f;

	private float delayBetweenChecks;

	private const float DELAY_DELTA = 0.5f;

	public Structures_Shrine StructureBrain
	{
		get
		{
			return Structure.Brain as Structures_Shrine;
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
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_FlameIII))
		{
			addFuel.MaxFuel /= 2;
		}
	//	TwitchAuthentication.OnAuthenticated += TwitchAuthentication_OnAuthenticated;
//		TwitchTotem.TotemUpdated += TwitchTotem_TotemUpdated;
		UpgradeSystem.OnUpgradeUnlocked += OnUpgradeUnlocked;
		UpgradeSystem.OnAbilityPointDelta = (Action)Delegate.Combine(UpgradeSystem.OnAbilityPointDelta, new Action(CheckXP));
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		addFuel.OnFuelModified += OnFuelModified;
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		//TwitchAuthentication.OnAuthenticated -= TwitchAuthentication_OnAuthenticated;
	//	TwitchTotem.TotemUpdated -= TwitchTotem_TotemUpdated;
		UpgradeSystem.OnUpgradeUnlocked -= OnUpgradeUnlocked;
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		if (StructureBrain != null)
		{
			Structures_Shrine structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Remove(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
		if (Structure != null)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		UpgradeSystem.OnAbilityPointDelta = (Action)Delegate.Remove(UpgradeSystem.OnAbilityPointDelta, new Action(CheckXP));
		UpgradeSystem.OnUpgradeUnlocked -= OnUpgradeUnlocked;
		if (addFuel != null)
		{
			addFuel.OnFuelModified -= OnFuelModified;
		}
	}

	private void TwitchAuthentication_OnAuthenticated()
	{
		CheckXP();
	}

	private void TwitchTotem_TotemUpdated(int contributions)
	{
		CheckXP();
	}

	public override void OnEnableInteraction()
	{
		DataManager.Instance.ShrineLevel = 1;
		base.OnEnableInteraction();
		Shrines.Add(this);
		CheckXP();
	}

	private void OnUpgradeUnlocked(UpgradeSystem.Type upgradeType)
	{
		addFuel.gameObject.SetActive(UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_Flame));
		int num = 0;
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_FlameII))
		{
			num = 2;
		}
		else if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_FlameIII))
		{
			num = 1;
		}
		for (int i = 0; i < flameLevels.Length; i++)
		{
			flameLevels[i].SetActive(i == num);
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_Shrine structureBrain = StructureBrain;
		structureBrain.OnSoulsGained = (Action<int>)Delegate.Combine(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		UpdateBar();
		if (!DataManager.Instance.XPEnabled && StructureBrain.Data.Type == global::StructureBrain.TYPES.SHRINE)
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.RepairTheShrine);
			DataManager.Instance.XPEnabled = true;
			GameManager.RecalculatePaths(true);
		}
		if (Structure.Type == global::StructureBrain.TYPES.SHRINE)
		{
			DataManager.Instance.HasBuiltShrine1 = true;
		}
		if (Structure.Type == global::StructureBrain.TYPES.SHRINE_II)
		{
			DataManager.Instance.HasBuiltShrine2 = true;
		}
		if (Structure.Type == global::StructureBrain.TYPES.SHRINE_III)
		{
			DataManager.Instance.HasBuiltShrine3 = true;
		}
		if (Structure.Type == global::StructureBrain.TYPES.SHRINE_IV)
		{
			DataManager.Instance.HasBuiltShrine4 = true;
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FULLY_UPGRADED_SHRINE"));
		}
		OnUpgradeUnlocked(UpgradeSystem.Type.Ability_Eat);
		emissionModule = psDevotion.emission;
		CheckXP();
	}

	private IEnumerator NewFollowerAndTutorial()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 8f);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 6f);
		yield return new WaitForSeconds(0.5f);
		FollowerManager.CreateNewRecruit(FollowerLocation.Base, BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position);
		yield return new WaitForSeconds(1.5f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 10f);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
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
	}

	public void CheckXP()
	{
		//if (TwitchAuthentication.IsAuthenticated && TwitchTotem.TotemUnlockAvailable)
		//{
		//	if (!twitchLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show") && !twitchLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
		//	{
		//		twitchLevelUpAnimator.Play("Show");
		//	}
		//	if (shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
		//	{
		//		shrineLevelUpAnimator.Play("Hide");
		//	}
		//	else if (!shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
		//	{
		//		shrineLevelUpAnimator.Play("Hidden");
		//	}
		//	ShrineCanLevelUp.SetActive(false);
		//	ShrineCantLevelUp.SetActive(true);
		//}
		 if (UpgradeSystem.AbilityPoints > 0 && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten))
		{
			if ((bool)ShrineCanLevelUp && !ShrineCanLevelUp.activeSelf)
			{
				AudioManager.Instance.PlayOneShot("event:/upgrade_statue/upgrade_ready", base.gameObject);
			}
			if (!shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show") && !shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
			{
				shrineLevelUpAnimator.Play("Show");
			}
			if ((bool)ShrineCanLevelUp)
			{
				GameObject shrineCanLevelUp = ShrineCanLevelUp;
				if ((object)shrineCanLevelUp != null)
				{
					shrineCanLevelUp.SetActive(true);
				}
			}
			if ((bool)ShrineCantLevelUp)
			{
				GameObject shrineCantLevelUp = ShrineCantLevelUp;
				if ((object)shrineCantLevelUp != null)
				{
					shrineCantLevelUp.SetActive(false);
				}
			}
			int propertyID = Shader.PropertyToID("_Cutoff");
			ShrineGlow.material.DOKill();
			ShrineGlow.material.DOFloat(0.9f, propertyID, 1f);
			if (StructureBrain != null)
			{
				ParticleSystem.EmissionModule emission = psDevotion.emission;
				emission.rateOverTime = Mathf.Lerp(1f, 3f, Mathf.Cos(Time.deltaTime));
			}
			if (twitchLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
			{
				twitchLevelUpAnimator.Play("Hide");
			}
			else
			{
				twitchLevelUpAnimator.Play("Hidden");
			}
		}
		else
		{
			if (shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
			{
				shrineLevelUpAnimator.Play("Hide");
			}
			else
			{
				shrineLevelUpAnimator.Play("Hidden");
			}
			if (twitchLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
			{
				twitchLevelUpAnimator.Play("Hide");
			}
			else
			{
				twitchLevelUpAnimator.Play("Hidden");
			}
			if ((bool)ShrineCanLevelUp)
			{
				ShrineCanLevelUp.SetActive(false);
			}
			if ((bool)ShrineCantLevelUp)
			{
				ShrineCantLevelUp.SetActive(true);
			}
		}
	}

	private IEnumerator FadeGlowMaterial()
	{
		float Progress = 0f;
		float Duration = 1f;
		int CutOffID = Shader.PropertyToID("_Cutoff");
		float Target = ShrineGlow.sharedMaterial.GetFloat(CutOffID);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ShrineGlow.material.SetFloat(CutOffID, Mathf.Lerp(0f, Target, Progress / Duration));
			yield return null;
		}
		ShrineGlow.material.SetFloat(CutOffID, Target);
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
	}

	public override void GetLabel()
	{
		if (StructureBrain == null)
		{
			base.Label = "";
		}
		//else if (TwitchAuthentication.IsAuthenticated && TwitchTotem.TotemUnlockAvailable && !TwitchTotem.Deactivated)
		//{
		//	Interactable = true;
		//	HasSecondaryInteraction = false;
		//	base.Label = LocalizationManager.GetTranslation("UI/Twitch/Totem/CollectReward").Colour(StaticColors.TwitchPurple);
		//}
		else if (UpgradeSystem.AbilityPoints > 0 && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten))
		{
			Interactable = true;
			HasSecondaryInteraction = false;
			if (GameManager.HasUnlockAvailable())
			{
				base.Label = ScriptLocalization.Interactions.CollectNewAbility;
				return;
			}
			base.Label = ScriptLocalization.Interactions.Collect + " " + ScriptLocalization.Inventory.GOD_TEAR + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.GOD_TEAR);
		}
		else
		{
			Interactable = StructureBrain.SoulCount > 0;
			HasSecondaryInteraction = true;
			SecondaryInteractable = true;
			hasUnlockAvailable = GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten;
			string text = (hasUnlockAvailable ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
			base.Label = sString + " " + text + " " + StructureBrain.SoulCount + StaticColors.GreyColorHex + " / " + StructureBrain.SoulMax;
		}
	}

	public override void GetSecondaryLabel()
	{
		bool flag = false;
		//flag = TwitchAuthentication.IsAuthenticated && TwitchTotem.TotemUnlockAvailable && !TwitchTotem.Deactivated;
		if ((UpgradeSystem.AbilityPoints > 0 && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)) || flag)
		{
			base.SecondaryLabel = "";
		}
		else
		{
			base.SecondaryLabel = ScriptLocalization.Interactions.AbilityScreen;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activating)
		{
			return;
		}
		base.OnInteract(state);
		IndicateHighlighted();
		//if (TwitchAuthentication.IsAuthenticated && TwitchTotem.TotemUnlockAvailable && !TwitchTotem.Deactivated)
		if (false)
		{
			GiveTwitchTotemReward();
		}
		else if (UpgradeSystem.AbilityPoints > 0 && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten))
		{
			if (GameManager.HasUnlockAvailable())
			{
				AudioManager.Instance.PlayOneShot("event:/upgrade_statue/upgrade_statue_open", base.gameObject);
				MonoSingleton<UIManager>.Instance.ShowUpgradeTree(UpdateBar);
			}
			else
			{
				StartCoroutine(GiveGodTearIE());
			}
		}
		else
		{
			Activating = true;
		}
		if (StructureBrain.SoulCount >= StructureBrain.SoulMax)
		{
			wasFull = true;
		}
		hasUnlockAvailable = GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten;
	}

	private void GiveTwitchTotemReward()
	{
		List<UIRandomWheel.Segment> list = new List<UIRandomWheel.Segment>();
		if (DataManager.TwitchTotemRewardAvailable())
		{
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.LOG
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.MEAT
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.FOLLOWERS
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.STONE
			});
			list.Shuffle();
			list.Insert(0, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			list.Insert(2, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			list.Insert(4, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			list.Insert(6, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
		}
		else
		{
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.25f,
				reward = InventoryItem.ITEM_TYPE.LOG
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.25f,
				reward = InventoryItem.ITEM_TYPE.MEAT
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.25f,
				reward = InventoryItem.ITEM_TYPE.FOLLOWERS
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.25f,
				reward = InventoryItem.ITEM_TYPE.STONE
			});
			list.Shuffle();
		}
		GameManager.GetInstance().OnConversationNew();
		UITwitchTotemWheel wheel = MonoSingleton<UIManager>.Instance.TwitchTotemWheelController.Instantiate();
		wheel.Show(list.ToArray());
		UITwitchTotemWheel uITwitchTotemWheel = wheel;
		uITwitchTotemWheel.OnHidden = (Action)Delegate.Combine(uITwitchTotemWheel.OnHidden, (Action)delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			switch (wheel.ChosenSegment.reward)
			{
			case InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION:
			{
				List<StructureBrain.TYPES> availableTwitchTotemDecorations = DataManager.GetAvailableTwitchTotemDecorations();
				List<string> availableTwitchTotemSkins = DataManager.GetAvailableTwitchTotemSkins();
				StructureBrain.TYPES tYPES = global::StructureBrain.TYPES.NONE;
				string text = "";
				if (UnityEngine.Random.Range(0, availableTwitchTotemDecorations.Count + availableTwitchTotemSkins.Count) < availableTwitchTotemSkins.Count)
				{
					text = availableTwitchTotemSkins[UnityEngine.Random.Range(0, availableTwitchTotemSkins.Count)];
				}
				else
				{
					tYPES = availableTwitchTotemDecorations[UnityEngine.Random.Range(0, availableTwitchTotemDecorations.Count)];
				}
				UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
				if (tYPES != 0)
				{
					StructuresData.CompleteResearch(tYPES);
					StructuresData.SetRevealed(tYPES);
					uINewItemOverlayController.pickedBuilding = tYPES;
					uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.Decoration, base.transform.position, false);
				}
				else if (!string.IsNullOrEmpty(text))
				{
					DataManager.SetFollowerSkinUnlocked(text);
					uINewItemOverlayController.pickedBuilding = tYPES;
					uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.FollowerSkin, base.transform.position, text);
				}
				break;
			}
			case InventoryItem.ITEM_TYPE.LOG:
			{
				new List<InventoryItem.ITEM_TYPE>();
				int num = DataManager.Instance.BossesCompleted.Count + 1;
				int num2 = Mathf.Clamp(UnityEngine.Random.Range(15, 25) * num, 1, 50);
				for (int i = 0; i < num2; i++)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.LOG, 1, base.transform.position + Vector3.down).SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				}
				break;
			}
			case InventoryItem.ITEM_TYPE.STONE:
			{
				new List<InventoryItem.ITEM_TYPE>();
				int num3 = DataManager.Instance.BossesCompleted.Count + 1;
				int num4 = Mathf.Clamp(UnityEngine.Random.Range(5, 15) * num3, 1, 50);
				for (int j = 0; j < num4; j++)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.STONE, 1, base.transform.position + Vector3.down).SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				}
				break;
			}
			case InventoryItem.ITEM_TYPE.MEAT:
			{
				List<InventoryItem.ITEM_TYPE> list2 = new List<InventoryItem.ITEM_TYPE>();
				int num5 = DataManager.Instance.BossesCompleted.Count + 1;
				int num6 = Mathf.Clamp(UnityEngine.Random.Range(15, 20) * num5, 1, 50);
				for (int k = 0; k < num6; k++)
				{
					int num7 = UnityEngine.Random.Range(0, 100);
					if (num7 < 33)
					{
						num7 = UnityEngine.Random.Range(0, 100);
						if (num7 < 33)
						{
							list2.Add(InventoryItem.ITEM_TYPE.MEAT);
						}
						else
						{
							list2.Add(InventoryItem.ITEM_TYPE.MEAT_MORSEL);
						}
					}
					else if (num7 < 66)
					{
						num7 = UnityEngine.Random.Range(0, 100);
						if (num7 < 45)
						{
							list2.Add(InventoryItem.ITEM_TYPE.FISH_SMALL);
						}
						else if (num7 < 75)
						{
							list2.Add(InventoryItem.ITEM_TYPE.FISH);
						}
						else
						{
							list2.Add(InventoryItem.ITEM_TYPE.FISH_BIG);
						}
					}
					else
					{
						num7 = UnityEngine.Random.Range(0, 100);
						if (DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_4) && num7 < 25)
						{
							list2.Add(InventoryItem.ITEM_TYPE.BEETROOT);
						}
						else if (DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3) && num7 < 50)
						{
							list2.Add(InventoryItem.ITEM_TYPE.CAULIFLOWER);
						}
						else if (DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2) && num7 < 75)
						{
							list2.Add(InventoryItem.ITEM_TYPE.PUMPKIN);
						}
						else
						{
							list2.Add(InventoryItem.ITEM_TYPE.BERRY);
						}
					}
				}
				{
					foreach (InventoryItem.ITEM_TYPE item in list2)
					{
						InventoryItem.Spawn(item, 1, base.transform.position + Vector3.down).SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
					}
					break;
				}
			}
			case InventoryItem.ITEM_TYPE.FOLLOWERS:
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				DataManager.SetFollowerSkinUnlocked(followerInfo.SkinName);
				if (BiomeBaseManager.Instance.SpawnExistingRecruits && DataManager.Instance.Followers_Recruit.Count > 0)
				{
					DataManager.Instance.Followers_Recruit.Add(followerInfo);
				}
				else
				{
					StartCoroutine(GiveFollowerIE(followerInfo));
				}
				break;
			}
			}
		});
		//TwitchTotem.TotemRewardClaimed();
		CheckXP();
	}

	private IEnumerator GiveFollowerIE(FollowerInfo f)
	{
		BiomeBaseManager.Instance.SpawnExistingRecruits = false;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew(true, true, true);
		GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 6f);
		yield return new WaitForSeconds(0.5f);
		DataManager.Instance.Followers_Recruit.Add(f);
		FollowerManager.SpawnExistingRecruits(BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position);
		UnityEngine.Object.FindObjectOfType<FollowerRecruit>().ManualTriggerAnimateIn();
		BiomeBaseManager.Instance.SpawnExistingRecruits = true;
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 8f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		AudioManager.Instance.PlayOneShot("event:/upgrade_statue/upgrade_statue_open", base.gameObject);
		MonoSingleton<UIManager>.Instance.ShowUpgradeTree();
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		//if (TwitchAuthentication.IsAuthenticated && !TwitchTotem.TotemUnlockAvailable && !TwitchTotem.Deactivated)
		//{
		//	MonoSingleton<Indicator>.Instance.ShowTopInfo(string.Format("{0}: {1} / {2}", LocalizationManager.GetTranslation("UI/Twitch/Totem/Contributions"), Mathf.Clamp(TwitchTotem.Contributions, 0, 10), 10).Colour(StaticColors.TwitchPurple));
		//}
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		MonoSingleton<Indicator>.Instance.HideTopInfo();
	}

	private IEnumerator EndUpgradeRoutine()
	{
		yield return new WaitForSeconds(1.5f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 8f);
		yield return new WaitForSeconds(1.5f);
		GameManager.GetInstance().OnConversationEnd();
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
		float num = Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
		XPBar.UpdateBar(num);
		if (StructureBrain != null && (UpgradeSystem.AbilityPoints == 0 || !GameManager.HasUnlockAvailable()))
		{
			int num2 = Shader.PropertyToID("_Cutoff");
			ShrineGlow.sharedMaterial.GetFloat(num2);
			ShrineGlow.material.DOKill();
			ShrineGlow.material.DOFloat(num - 0.1f, num2, 1f);
			ParticleSystem.EmissionModule emission = psDevotion.emission;
			float num3 = Mathf.SmoothStep(0f, 0.5f, num);
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(num3, num3);
		}
	}

	private IEnumerator GiveGodTearIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameObject godTear = UnityEngine.Object.Instantiate(godTearPrefab, base.transform.position - Vector3.forward * 2f, Quaternion.identity, base.transform.parent);
		godTear.transform.localScale = Vector3.zero;
		godTear.transform.DOScale(Vector3.one, 1.5f).SetEase(Ease.OutBack);
		GameManager.GetInstance().OnConversationNext(godTear, 6f);
		AudioManager.Instance.PlayOneShot("event:/Stings/global_faith_up", godTear);
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", godTear);
		AudioManager.Instance.PlayOneShot("event:/player/float_follower", godTear);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		yield return new WaitForSeconds(1.5f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		ShortcutExtensions.DOMove(endValue: new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f), target: godTear.transform, duration: 0.5f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(0.25f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.FoundItem;
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.transform.position);
		Inventory.AddItem(119, 1);
		UpgradeSystem.AbilityPoints--;
		yield return new WaitForSeconds(1.25f);
		UnityEngine.Object.Destroy(godTear.gameObject);
		GameManager.GetInstance().OnConversationEnd();
	}

	private new void Update()
	{
		if ((delayBetweenChecks -= Time.deltaTime) >= 0f && !InputManager.Gameplay.GetInteractButtonHeld())
		{
			Activating = false;
			return;
		}
		delayBetweenChecks = 0.5f;
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		if (Activating && (StructureBrain.SoulCount <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
		{
			Activating = false;
			ReduceDelay = 0.1f;
			if (wasFull && StructureBrain.SoulCount <= 0)
			{
				wasFull = false;
				foreach (Follower follower in Follower.Followers)
				{
					if (follower.Brain.Info.FollowerRole == FollowerRole.Worshipper && follower.Brain.CurrentTaskType != FollowerTaskType.Sleep && follower.Brain.CurrentTaskType != FollowerTaskType.SleepBedRest)
					{
						follower.Brain.CompleteCurrentTask();
					}
				}
			}
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			if (hasUnlockAvailable)
			{
				SoulCustomTarget.Create(state.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
				Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			Structures_Shrine structureBrain = StructureBrain;
			int soulCount = structureBrain.SoulCount - 1;
			structureBrain.SoulCount = soulCount;
			Delay = Mathf.Max(ReduceDelay -= Time.deltaTime * 0.05f, 0.005f);
			UpdateBar();
		}
	}

	private void GivePlayerSoul()
	{
		PlayerFarming instance = PlayerFarming.Instance;
		if ((object)instance != null)
		{
			instance.GetSoul(1);
		}
	}
}
