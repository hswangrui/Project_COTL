using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherMinibossManager : BaseMonoBehaviour
{
	public static int CurrentIndex;

	public List<MiniBossController> BossEncounters;

	private MiniBossController CurrentMiniBoss;

	public Interaction_Chest Chest;

	private bool revealed;

	private bool introPlayed;

	private GameObject Follower;

	private int DeathCount;

	private bool Completed;

	private GameObject Boss;

	private void Start()
	{
		BossEncounters = new List<MiniBossController>(GetComponentsInChildren<MiniBossController>());
		CurrentMiniBoss = BossEncounters[Random.Range(0, GameManager.CurrentDungeonLayer - 1)];
		foreach (MiniBossController bossEncounter in BossEncounters)
		{
			bossEncounter.gameObject.GetComponentInChildren<UnitObject>().CanHaveModifier = false;
			bossEncounter.gameObject.SetActive(bossEncounter == CurrentMiniBoss);
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

	public void Play()
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.MainBossA);
		foreach (Health item in CurrentMiniBoss.EnemiesToTrack)
		{
			item.OnDie += H_OnDie;
		}
		StartCoroutine(IntroRoutine());
	}

	private void OnEnable()
	{
		if (!introPlayed)
		{
			StartCoroutine(IntroRoutine());
		}
	}

	private IEnumerator IntroRoutine()
	{
		introPlayed = true;
		yield return new WaitForEndOfFrame();
		while (PlayerFarming.Instance == null || PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		StartCoroutine(CurrentMiniBoss.IntroRoutine());
		RoomLockController.CloseAll();
	}

	private void H_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		DeathCount++;
		Victim.OnDie -= H_OnDie;
		if (!Completed && (Victim.gameObject.CompareTag("Boss") || Health.team2.Count <= 1))
		{
			Completed = true;
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
		yield return new WaitForSeconds(0.5f);
		Chest.RevealBossReward(InventoryItem.ITEM_TYPE.NONE);
	}
}
