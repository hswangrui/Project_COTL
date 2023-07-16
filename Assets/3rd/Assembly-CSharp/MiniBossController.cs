using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI.DeathScreen;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniBossController : BaseMonoBehaviour
{
	public class MiniBossData
	{
		public Enemy EnemyType;

		public List<EnemyModifier.ModifierType> EncounteredModifiers;
	}

	public static MiniBossController Instance;

	public BossIntro BossIntro;

	[TermsPopup("")]
	public string DisplayName;

	public bool ShowName = true;

	public List<Health> EnemiesToTrack = new List<Health>();

	public List<BaseMonoBehaviour> ComponentsToToggleEnabled = new List<BaseMonoBehaviour>();

	[Space]
	[SerializeField]
	private bool cycleModifiers = true;

	private EnemyModifier modifier;

	private bool shown;

	private bool bossDead;

	[SerializeField]
	private bool isBigBoss;

	[SerializeField]
	private bool isPostGameBoss;

	private bool PlayerHit;

	private bool WaitingForAnimationToComplete;

	public GameObject cameraTarget
	{
		get
		{
			return EnemiesToTrack[0].gameObject;
		}
	}

	private string Name
	{
		get
		{
			string text = LocalizationManager.Sources[0].GetTranslation(DisplayName);
			if (isPostGameBoss)
			{
				text += " II";
			}
			return text;
		}
	}

	private void Awake()
	{
		MiniBossData miniBossData = DataManager.Instance.GetMiniBossData(BossIntro.GetComponent<UnitObject>().EnemyType);
		if (DataManager.Instance != null && miniBossData != null && cycleModifiers && !DungeonSandboxManager.Active)
		{
			EnemyModifier modifierExcluding = EnemyModifier.GetModifierExcluding(miniBossData.EncounteredModifiers);
			if (modifierExcluding == null)
			{
				miniBossData.EncounteredModifiers.Clear();
				modifierExcluding = EnemyModifier.GetModifier(1f);
			}
			modifier = modifierExcluding;
			EnemiesToTrack[0].GetComponent<UnitObject>().ForceSetModifier(modifierExcluding);
			miniBossData.EncounteredModifiers.Add(modifierExcluding.Modifier);
		}
		EnemiesToTrack[0].GetComponent<Health>().OnDie += MiniBossController_OnDie;
		foreach (Health item in EnemiesToTrack)
		{
			item.totalHP *= DataManager.Instance.BossHealthMultiplier;
			item.HP *= DataManager.Instance.BossHealthMultiplier;
		}
		foreach (BaseMonoBehaviour item2 in ComponentsToToggleEnabled)
		{
			if (item2 != null)
			{
				item2.enabled = false;
				item2.transform.position = Vector3.zero;
			}
		}
	}

	private void MiniBossController_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		PlayerFarming.Instance.health.OnHitCallback.RemoveListener(OnPlayerHit);
		if (!PlayerHit)
		{
			DataManager.Instance.TakenBossDamage = false;
		}
		else
		{
			DataManager.Instance.TakenBossDamage = true;
		}
		Debug.Log("Taken Boss Damage = " + PlayerHit);
		GameManager.GetInstance().RemoveFromCamera(cameraTarget);
		UIBossHUD.Hide();
		bossDead = true;
		DataManager.Instance.BeatenFirstMiniBoss = true;
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.BossEntryAmbience);
		if (DataManager.Instance.GetMiniBossData(Victim.GetComponent<UnitObject>().EnemyType) == null)
		{
			MiniBossData item = new MiniBossData
			{
				EnemyType = Victim.GetComponent<UnitObject>().EnemyType,
				EncounteredModifiers = new List<EnemyModifier.ModifierType>()
			};
			DataManager.Instance.MiniBossData.Add(item);
		}
		if (isBigBoss)
		{
			if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) || (GameManager.Layer2 && !BeatenPostGameLeader()))
			{
				DataManager.Instance.LastRunResults = UIDeathScreenOverlayController.Results.BeatenBoss;
			}
		}
		else
		{
			DataManager.Instance.LastRunResults = UIDeathScreenOverlayController.Results.BeatenMiniBoss;
		}
		SimulationManager.UnPause();
		GameManager.GetInstance().StartCoroutine(KillEnemiesDelay());
		int num = -1;
		switch (SceneManager.GetActiveScene().name)
		{
		case "Dungeon Boss 1":
			num = 1;
			break;
		case "Dungeon Boss 2":
			num = 2;
			break;
		case "Dungeon Boss 3":
			num = 3;
			break;
		case "Dungeon Boss 4":
			num = 4;
			break;
		default:
			num = -1;
			break;
		}
		if (num != -1 && (bool)MonoSingleton<PlayerProgress_Analytics>.Instance)
		{
			MonoSingleton<PlayerProgress_Analytics>.Instance.CultLeaderComplete(num);
		}
	}

	private bool BeatenPostGameLeader()
	{
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_1 && DataManager.Instance.BeatenLeshyLayer2)
		{
			return true;
		}
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_2 && DataManager.Instance.BeatenHeketLayer2)
		{
			return true;
		}
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_3 && DataManager.Instance.BeatenKallamarLayer2)
		{
			return true;
		}
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_4 && DataManager.Instance.BeatenShamuraLayer2)
		{
			return true;
		}
		return false;
	}

	private IEnumerator KillEnemiesDelay()
	{
		yield return new WaitForSeconds(1f);
		Health health = ((EnemiesToTrack[0] != null) ? EnemiesToTrack[0].GetComponent<Health>() : null);
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null && Health.team2[num] != health && Health.team2[num].gameObject.activeInHierarchy)
			{
				Health.team2[num].enabled = true;
				Health.team2[num].invincible = false;
				Health.team2[num].untouchable = false;
				Health.team2[num].DealDamage(Health.team2[num].totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
	}

	private void OnEnable()
	{
		if (shown && !bossDead)
		{
			UIBossHUD.Play(EnemiesToTrack[0], Name);
			UIBossHUD.Instance.ForceHealthAmount(EnemiesToTrack[0].HP / EnemiesToTrack[0].totalHP);
			EnemiesToTrack[0].SlowMoOnkill = true;
			SetMusic();
		}
	}

	private void OnDisable()
	{
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.health.OnHitCallback.RemoveListener(OnPlayerHit);
		}
	}

	private void OnDestroy()
	{
		Instance = null;
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.health.OnHitCallback.RemoveListener(OnPlayerHit);
		}
	}

	private void SetMusic()
	{
		switch (BossIntro.GetComponent<UnitObject>().EnemyType)
		{
		case Enemy.Beholder1:
		case Enemy.Beholder2:
		case Enemy.Beholder3:
		case Enemy.Beholder4:
		case Enemy.Beholder5:
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.BeholderBattle);
			return;
		}
		if (!isBigBoss)
		{
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.MainBossA);
		}
		else
		{
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.MainBossB);
		}
	}

	private void TrackPlayerHealth()
	{
		PlayerHit = false;
		PlayerFarming.Instance.health.OnHitCallback.AddListener(OnPlayerHit);
	}

	private void OnPlayerHit()
	{
		PlayerHit = true;
	}

	public void Play(bool skipped = false)
	{
		StartCoroutine(IntroRoutine(skipped));
	}

	public IEnumerator IntroRoutine(bool skipped = false)
	{
		SimulationManager.Pause();
		Instance = this;
		TrackPlayerHealth();
		if (ShowName)
		{
			if (BossIntro.GetComponent<UnitObject>().EnemyType == Enemy.FrogBoss)
			{
				HUD_DisplayName.Play(Name, 2, HUD_DisplayName.Positions.Centre, HUD_DisplayName.textBlendMode.FrogBoss);
			}
			else
			{
				HUD_DisplayName.Play(Name, 2, HUD_DisplayName.Positions.Centre);
			}
		}
		foreach (Health item in EnemiesToTrack)
		{
			ShowHPBar component = item.GetComponent<ShowHPBar>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		yield return StartCoroutine(BossIntro.PlayRoutine(skipped));
		SetMusic();
		foreach (BaseMonoBehaviour item2 in ComponentsToToggleEnabled)
		{
			item2.enabled = true;
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		UIBossHUD.Play(EnemiesToTrack[0], Name);
		GameManager.GetInstance().AddToCamera(cameraTarget);
		shown = true;
		RoomLockController.CloseAll();
	}

	public IEnumerator OutroRoutine()
	{
		yield return null;
		foreach (Health item in Health.team2)
		{
			if ((bool)item)
			{
				item.DestroyNextFrame();
			}
		}
	}
}
