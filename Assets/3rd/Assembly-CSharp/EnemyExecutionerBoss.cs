using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyExecutionerBoss : BaseMonoBehaviour
{
	public List<FormationFighter> GruntsToEnable = new List<FormationFighter>();

	public List<GameObject> SummonList = new List<GameObject>();

	public AudioClip PreCombatMusic;

	public AudioClip CombatMusic;

	public TriggerCanvasGroup TriggerCanvasGroup;

	public SkeletonAnimation Spine;

	public Vector3 ActivateOffset;

	public float ActivateDistance = 5f;

	private bool Activated;

	private GameObject Player;

	public Health health;

	public EnemyBrute enemyBrute;

	public Interaction_MonsterHeart Interaction_MonsterHeart;

	private bool NotDead = true;

	private float Timer;

	private GameObject EnemySpawnerGO;

	private int SummonedCount;

	private void OnEnable()
	{
		health.OnDie += OnDie;
	}

	private void OnDisable()
	{
		health.OnDie += OnDie;
		Interaction_MonsterHeart.OnHeartTaken -= OnHeartTaken;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		int num = -1;
		while (++num < Health.team2.Count)
		{
			if (Health.team2[num] != null && Health.team2[num] != enemyBrute.health)
			{
				Health.team2[num].DestroyNextFrame();
			}
		}
		StopAllCoroutines();
		NotDead = false;
		GetComponent<Collider2D>().enabled = false;
		AmbientMusicController.StopCombat();
		AudioManager.Instance.SetMusicCombatState(false);
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.BossEntryAmbience);
		Interaction_MonsterHeart.Play();
		enemyBrute.enabled = false;
		Interaction_MonsterHeart.OnHeartTaken += OnHeartTaken;
		Spine.AnimationState.SetAnimation(0, "die", false);
		Spine.AnimationState.AddAnimation(0, "dead", true, 0f);
		foreach (FormationFighter item in GruntsToEnable)
		{
			if (item != null)
			{
				item.health.DealDamage(float.MaxValue, base.gameObject, base.transform.position);
			}
		}
	}

	private void OnHeartTaken()
	{
		DataManager.Instance.DefeatedExecutioner = true;
	}

	private void Update()
	{
		if (Activated)
		{
			if (NotDead && SummonedCount <= 1 && (Timer += Time.deltaTime) > 7f && enemyBrute.enabled && (enemyBrute.state.CURRENT_STATE == StateMachine.State.Moving || enemyBrute.state.CURRENT_STATE == StateMachine.State.Idle))
			{
				enemyBrute.StopAllCoroutines();
				StartCoroutine(Summon());
			}
		}
		else if (!((Player = GameObject.FindWithTag("Player")) == null) && Vector3.Distance(base.transform.position + ActivateOffset, Player.transform.position) < ActivateDistance)
		{
			Play();
		}
	}

	private IEnumerator Summon()
	{
		enemyBrute.enabled = false;
		enemyBrute.state.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, "summon", false);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(11f / 15f);
		int num = -1;
		while (++num < 3)
		{
			float f = (float)(120 * num) * ((float)Math.PI / 180f);
			Vector3 position = base.transform.position + new Vector3(3f * Mathf.Cos(f), 3f * Mathf.Sin(f));
			EnemySpawnerGO = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Enemy Spawner/EnemySpawner"), position, Quaternion.identity, base.transform.parent) as GameObject;
			EnemySpawnerGO.GetComponent<EnemySpawner>().InitAndInstantiate(SummonList[UnityEngine.Random.Range(0, SummonList.Count)]).GetComponent<Health>()
				.OnDie += RemoveSpawned;
			SummonedCount++;
		}
		yield return new WaitForSeconds(1.4666667f);
		Timer = 0f;
		enemyBrute.enabled = true;
		enemyBrute.state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForEndOfFrame();
		enemyBrute.StartCoroutine(enemyBrute.ChasePlayer());
	}

	private void RemoveSpawned(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		SummonedCount--;
		Victim.OnDie -= RemoveSpawned;
	}

	public void Play()
	{
		StartCoroutine(DoPlay());
	}

	private IEnumerator DoPlay()
	{
		Activated = true;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		BlockingDoor.CloseAll();
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.MainBossA);
		Spine.AnimationState.SetAnimation(0, "execute", false);
		Spine.AnimationState.AddAnimation(0, "wake-up", false, 0f);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(5f);
		foreach (FormationFighter item in GruntsToEnable)
		{
			item.enabled = true;
		}
		GameManager.GetInstance().OnConversationEnd();
		enemyBrute.enabled = true;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, ActivateDistance, Color.green);
	}
}
