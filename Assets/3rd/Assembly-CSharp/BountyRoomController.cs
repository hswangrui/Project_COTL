using System.Collections;
using MMBiomeGeneration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BountyRoomController : BaseMonoBehaviour
{
	[SerializeField]
	private int difficulty;

	[SerializeField]
	private EnemyRoundsBase enemyRounds;

	[SerializeField]
	private BarricadeLine barricadeLine;

	[SerializeField]
	private MissionRewardChest rewardChest;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TMP_Text bountyText;

	[Space]
	[SerializeField]
	private UnityEvent onBountyBeaten;

	private bool triggered;

	private void Awake()
	{
		MissionManager.Mission mission = DataManager.Instance.ActiveMissions[0];
		rewardChest.Play(mission);
	}

	private void BeginBountyRoom()
	{
		enemyRounds.BeginCombat(true, BountyRoomComplete);
		BiomeGenerator.Instance.CurrentRoom.Active = true;
		BiomeGenerator.Instance.RoomBecameActive();
		PlayerController.CanRespawn = false;
		PlayerFarming.Instance.health.OnDie += OnPlayerDied;
		BlockingDoor.CloseAll();
		RoomLockController.CloseAll();
	}

	private void OnPlayerDied(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		StartCoroutine(BountyFinishedIE(false));
	}

	private void BountyRoomComplete()
	{
		barricadeLine.Open();
		StartCoroutine(BountyCompleteIE());
	}

	private IEnumerator BountyCompleteIE()
	{
		RemoveBounty();
		yield return new WaitForSeconds(1f);
		canvas.gameObject.SetActive(true);
		bountyText.text = "Bounty Completed";
		yield return new WaitForSeconds(2f);
		canvas.gameObject.SetActive(false);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(rewardChest.gameObject, 5f);
		yield return new WaitForSeconds(0.5f);
		rewardChest.ShowReward();
		UnityEvent unityEvent = onBountyBeaten;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator BountyFinishedIE(bool success)
	{
		yield return new WaitForSeconds(1f);
		canvas.gameObject.SetActive(true);
		bountyText.text = (success ? "Bounty Completed" : "Bounty Failed");
		yield return new WaitForSeconds(2f);
		RemoveBounty();
		GameManager.ToShip();
		PlayerController.CanRespawn = true;
	}

	private void RemoveBounty()
	{
		foreach (MissionManager.Mission activeMission in DataManager.Instance.ActiveMissions)
		{
			if (activeMission.MissionType == MissionManager.MissionType.Bounty && activeMission.Difficulty == difficulty)
			{
				MissionManager.RemoveMission(activeMission);
				break;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!triggered && other.gameObject.tag == "Player")
		{
			BeginBountyRoom();
			triggered = true;
		}
	}
}
