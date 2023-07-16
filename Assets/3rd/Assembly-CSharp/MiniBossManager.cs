using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Map;
using MMBiomeGeneration;
using Unify;
using UnityEngine;

public class MiniBossManager : BaseMonoBehaviour
{
	public static int CurrentIndex;

	public List<MiniBossController> BossEncounters;

	private MiniBossController CurrentMiniBoss;

	public Interaction_Chest Chest;

	public List<SingleChoiceRewardOption> SingleChoiceRewardOptions = new List<SingleChoiceRewardOption>();

	public List<Interaction_WeaponSelectionPodium> WeaponPodiums = new List<Interaction_WeaponSelectionPodium>();

	public GameObject FollowerToSpawn;

	public int ForcedIndex = -1;

	private bool revealed;

	private bool introPlayed;

	private InventoryItem.ITEM_TYPE ForceReward;

	private GameObject Follower;

	private int DeathCount;

	private bool Completed;

	private GameObject Boss;

	private static readonly int GlobalDitherIntensity = Shader.PropertyToID("_GlobalDitherIntensity");

	private void Start()
	{
		BossEncounters = new List<MiniBossController>(GetComponentsInChildren<MiniBossController>());
		if (ForcedIndex != -1)
		{
			CurrentMiniBoss = BossEncounters[ForcedIndex];
		}
		else
		{
			CurrentMiniBoss = BossEncounters[GameManager.CurrentDungeonLayer - 1];
			if ((DataManager.Instance.DungeonCompleted(PlayerFarming.Location) && !GameManager.Layer2) || DungeonSandboxManager.Active)
			{
				if (!DataManager.Instance.CheckKilledBosses(BossEncounters[BossEncounters.Count - 1].name) && !DungeonSandboxManager.Active)
				{
					CurrentMiniBoss = BossEncounters[BossEncounters.Count - 1];
				}
				else
				{
					do
					{
						CurrentMiniBoss = BossEncounters[Random.Range(0, BossEncounters.Count)];
					}
					while (CurrentMiniBoss.name == DataManager.Instance.PreviousMiniBoss);
				}
			}
			else if (GameManager.Layer2)
			{
				if (DataManager.Instance.DungeonCompleted(PlayerFarming.Location, true) && !DataManager.Instance.CheckKilledBosses(BossEncounters[BossEncounters.Count - 1].name + "_P2") && !DungeonSandboxManager.Active)
				{
					CurrentMiniBoss = BossEncounters[BossEncounters.Count - 1];
					ForceReward = InventoryItem.ITEM_TYPE.GOD_TEAR;
				}
				else
				{
					CurrentMiniBoss = null;
					if (!DataManager.Instance.CheckKilledBosses(BossEncounters[BossEncounters.Count - 1].name + "_P2"))
					{
						BossEncounters[BossEncounters.Count - 1].gameObject.SetActive(false);
						BossEncounters.RemoveAt(BossEncounters.Count - 1);
					}
					BossEncounters.Shuffle();
					foreach (MiniBossController bossEncounter in BossEncounters)
					{
						if (!DataManager.Instance.CheckKilledBosses(bossEncounter.name + "_P2") && !DungeonSandboxManager.Active)
						{
							CurrentMiniBoss = bossEncounter;
							ForceReward = InventoryItem.ITEM_TYPE.GOD_TEAR;
							break;
						}
					}
					if (CurrentMiniBoss == null)
					{
						do
						{
							CurrentMiniBoss = BossEncounters[Random.Range(0, BossEncounters.Count)];
						}
						while (CurrentMiniBoss.name + "_P2" == DataManager.Instance.PreviousMiniBoss);
					}
				}
			}
		}
		DataManager.Instance.PreviousMiniBoss = CurrentMiniBoss.name + (GameManager.Layer2 ? "_P2" : "");
		if (DungeonSandboxManager.Active)
		{
			if (MapManager.Instance.CurrentMap.GetFinalBossNode() == MapManager.Instance.CurrentNode)
			{
				ForceReward = InventoryItem.ITEM_TYPE.GOD_TEAR;
			}
			int num = 0;
			while (num < 100)
			{
				num++;
				CurrentMiniBoss = BossEncounters[Random.Range(0, 4)];
				if (!DungeonSandboxManager.Instance.EncounteredMiniBosses.Contains(CurrentMiniBoss.name))
				{
					break;
				}
			}
			if (!DungeonSandboxManager.Instance.EncounteredMiniBosses.Contains(CurrentMiniBoss.name))
			{
				DungeonSandboxManager.Instance.EncounteredMiniBosses.Add(CurrentMiniBoss.name);
			}
		}
		foreach (MiniBossController bossEncounter2 in BossEncounters)
		{
			bossEncounter2.gameObject.GetComponentInChildren<UnitObject>().CanHaveModifier = false;
			bossEncounter2.gameObject.SetActive(bossEncounter2 == CurrentMiniBoss);
			bossEncounter2.gameObject.transform.position = Vector3.zero;
			if (ForceReward == InventoryItem.ITEM_TYPE.GOD_TEAR)
			{
				bossEncounter2.GetComponentInChildren<Health>().SlowMoOnkill = true;
			}
		}
	}

	private void OnEnable()
	{
		if (Follower != null)
		{
			StartCoroutine(WaitForFollowerToBeRecruited());
		}
	}

	private void FixedUpdate()
	{
		if (CurrentMiniBoss != null && !introPlayed)
		{
			CurrentMiniBoss.BossIntro.transform.position = Vector3.zero;
		}
	}

	private void GetAndNameBosses(int DungeonNumber)
	{
		BossEncounters = new List<MiniBossController>(GetComponentsInChildren<MiniBossController>());
		int num = -1;
		while (++num < BossEncounters.Count)
		{
			BossEncounters[num].DisplayName = "NAMES/MiniBoss/Dungeon" + DungeonNumber + "/MiniBoss" + (num + 1);
		}
	}

	private void SetDither(float value)
	{
		Shader.SetGlobalFloat(GlobalDitherIntensity, value);
	}

	public void Play()
	{
		DOTween.Kill(this);
		DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance + 1.5f, 1f).SetEase(Ease.OutQuart);
		foreach (Health item in CurrentMiniBoss.EnemiesToTrack)
		{
			item.transform.position = Vector3.zero;
			item.OnDie += H_OnDie;
		}
		StartCoroutine(IntroRoutine());
	}

	private IEnumerator IntroRoutine()
	{
		introPlayed = true;
		yield return new WaitForEndOfFrame();
		while (PlayerFarming.Instance == null || PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(CurrentMiniBoss.IntroRoutine());
	}

	private void H_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		DeathCount++;
		Victim.OnDie -= H_OnDie;
		TrapPoison.RemoveAllPoison();
		Projectile.ClearProjectiles();
		if (ForceReward == InventoryItem.ITEM_TYPE.GOD_TEAR)
		{
			Vector3 vector = Victim.transform.position + Vector3.back;
			vector = Vector3.ClampMagnitude(vector, 4f);
			PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GOD_TEAR, 1, vector, 0f);
			pickUp.SetInitialSpeedAndDiraction(4f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			pickUp.MagnetToPlayer = false;
			ForceReward = InventoryItem.ITEM_TYPE.NONE;
			PlayerReturnToBase.Disabled = true;
		}
		if (!Completed && (Victim.gameObject.CompareTag("Boss") || Health.team2.Count <= 1))
		{
			DOTween.Kill(this);
			DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance, 1f).SetEase(Ease.OutQuart);
			Completed = true;
			if (!DataManager.Instance.CheckKilledBosses(CurrentMiniBoss.name) && !DungeonSandboxManager.Active)
			{
				PlayerReturnToBase.Disabled = true;
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_FIRST_BOSS"));
				AudioManager.Instance.PlayOneShot("event:/Stings/boss_kill", PlayerFarming.Instance.gameObject);
				Boss = Victim.gameObject;
				Follower = Object.Instantiate(FollowerToSpawn, Boss.transform.position, Quaternion.identity, Boss.transform.parent);
				Follower.GetComponent<Interaction_FollowerSpawn>().Play(CurrentMiniBoss.name, LocalizationManager.GetTranslation(CurrentMiniBoss.DisplayName));
				DataManager.SetFollowerSkinUnlocked(CurrentMiniBoss.name);
			}
			string text = CurrentMiniBoss.name;
			if (GameManager.Layer2)
			{
				text += "_P2";
			}
			if (!DataManager.Instance.CheckKilledBosses(text) && !DungeonSandboxManager.Active)
			{
				switch (CurrentMiniBoss.name)
				{
				case "Boss Beholder 1":
				case "Boss Beholder 2":
				case "Boss Beholder 3":
				case "Boss Beholder 4":
					ForceReward = InventoryItem.ITEM_TYPE.BEHOLDER_EYE;
					break;
				}
			}
		}
		if (GameManager.CurrentDungeonLayer < 4)
		{
			DataManager.Instance.SetDungeonLayer(BiomeGenerator.Instance.DungeonLocation, GameManager.CurrentDungeonLayer + 1);
		}
	}

	private void Update()
	{
		if (introPlayed && !revealed && DeathCount >= CurrentMiniBoss.EnemiesToTrack.Count)
		{
			revealed = true;
			StartCoroutine(RevealRoutine());
		}
	}

	private IEnumerator RevealRoutine()
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.BossEntryAmbience);
		yield return StartCoroutine(CurrentMiniBoss.OutroRoutine());
		yield return new WaitForSecondsRealtime(1.25f);
		if (!DataManager.Instance.CheckKilledBosses(CurrentMiniBoss.name) && !DungeonSandboxManager.Active)
		{
			yield return new WaitForSeconds(2.5f);
		}
		StartCoroutine(WaitForFollowerToBeRecruited());
	}

	private IEnumerator WaitForFollowerToBeRecruited()
	{
		while (Follower != null)
		{
			yield return null;
		}
		while (Interaction_DivineCrystal.Instance != null)
		{
			yield return null;
		}
		if (DungeonSandboxManager.Active)
		{
			if (MapManager.Instance.CurrentMap.GetFinalBossNode() != MapManager.Instance.CurrentNode)
			{
				foreach (Interaction_WeaponSelectionPodium s in WeaponPodiums)
				{
					s.Reveal();
					s.OnInteraction += delegate
					{
						foreach (Interaction_WeaponSelectionPodium weaponPodium in WeaponPodiums)
						{
							if (weaponPodium != s)
							{
								weaponPodium.Interactable = false;
								weaponPodium.Lighting.SetActive(false);
								weaponPodium.IconSpriteRenderer.enabled = false;
								weaponPodium.weaponBetterIcon.enabled = false;
								weaponPodium.podiumOn.SetActive(false);
								weaponPodium.podiumOff.SetActive(true);
								weaponPodium.particleEffect.Stop();
								weaponPodium.AvailableGoop.gameObject.SetActive(false);
								weaponPodium.enabled = false;
							}
						}
						StartCoroutine(WaitForUIToFinish(ForceReward, 1f));
					};
				}
			}
			else
			{
				StartCoroutine(WaitForUIToFinish(ForceReward));
			}
		}
		else if (SingleChoiceRewardOptions.Count > 0)
		{
			foreach (SingleChoiceRewardOption singleChoiceRewardOption in SingleChoiceRewardOptions)
			{
				singleChoiceRewardOption.Reveal();
				singleChoiceRewardOption.Callback.AddListener(delegate
				{
					StartCoroutine(WaitForUIToFinish(ForceReward));
				});
			}
		}
		else
		{
			Chest.RevealBossReward(ForceReward, delegate
			{
				if (!DataManager.Instance.CheckKilledBosses(CurrentMiniBoss.name) && !DungeonSandboxManager.Active)
				{
					DataManager.Instance.AddKilledBoss(CurrentMiniBoss.name);
				}
				else if (GameManager.Layer2 && !DungeonSandboxManager.Active)
				{
					DataManager.Instance.AddKilledBoss(CurrentMiniBoss.name + "_P2");
				}
			});
		}
		PlayerReturnToBase.Disabled = false;
	}

	private IEnumerator WaitForUIToFinish(InventoryItem.ITEM_TYPE ForceReward, float delay = 0f)
	{
		yield return null;
		while (FoundItemPickUp.FoundItemPickUps.Count > 0)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		Chest.RevealBossReward(ForceReward, delegate
		{
			if (!DataManager.Instance.CheckKilledBosses(CurrentMiniBoss.name) && !DungeonSandboxManager.Active)
			{
				DataManager.Instance.AddKilledBoss(CurrentMiniBoss.name);
			}
			else if (GameManager.Layer2 && !DungeonSandboxManager.Active)
			{
				DataManager.Instance.AddKilledBoss(CurrentMiniBoss.name + "_P2");
			}
		});
	}
}
