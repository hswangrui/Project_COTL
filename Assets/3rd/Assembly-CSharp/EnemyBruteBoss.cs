using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyBruteBoss : EnemyBrute
{
	[Serializable]
	private class SpawnSet
	{
		public GameObject[] Spawnables;

		public Vector3[] Positions;
	}

	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string spawnAnimation;

	[SerializeField]
	private float hammerAttackAnticipation;

	[SerializeField]
	private float hammerAttackCooldown;

	[SerializeField]
	private int maxEnemies = 15;

	[SerializeField]
	private SpawnSet[] spawnSets = new SpawnSet[0];

	[SerializeField]
	private float spawnAnticipation;

	[SerializeField]
	private float spawnCooldown;

	[Space]
	[SerializeField]
	private int attacksUntilSpawn = 4;

	[SerializeField]
	private Vector2 timeBetweenAttacks;

	[SerializeField]
	private Vector2 timeBetweenSpawns;

	private bool attacking;

	private float timestamp;

	private int counter;

	private float checkPlayerTimestamp;

	private float checkPlayerInterval = 0.3f;

	public override void OnEnable()
	{
		base.OnEnable();
		simpleSpineFlash.FlashWhite(false);
		TargetWarning.gameObject.SetActive(false);
		attacking = false;
		counter = 0;
		if ((bool)PlayerFarming.Instance)
		{
			givePath(PlayerFarming.Instance.transform.position);
		}
	}

	public override void Update()
	{
		base.Update();
		if ((bool)PlayerFarming.Instance)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > timestamp && !attacking)
			{
				if (counter == attacksUntilSpawn - 1 && Health.team2.Count - 1 < maxEnemies)
				{
					Spawn();
				}
				else
				{
					HammerAttack();
				}
				counter++;
				if (counter > attacksUntilSpawn)
				{
					counter = 0;
				}
			}
		}
		if (!attacking)
		{
			UpdateMoving();
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null && Health.team2[num] != health)
			{
				Health.team2[num].enabled = true;
				Health.team2[num].invincible = false;
				Health.team2[num].DealDamage(Health.team2[num].totalHP, base.gameObject, base.transform.position);
			}
		}
	}

	private void UpdateMoving()
	{
		if ((bool)PlayerFarming.Instance)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > checkPlayerTimestamp && GameManager.RoomActive)
			{
				checkPlayerTimestamp = GameManager.GetInstance().CurrentTime + checkPlayerInterval;
				givePath(PlayerFarming.Instance.transform.position);
				float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
				LookAtAngle(angle);
			}
		}
	}

	private void LookAtAngle(float angle)
	{
		state.facingAngle = angle;
		state.LookAngle = angle;
	}

	private void HammerAttack()
	{
		StartCoroutine(HammerAttackIE());
	}

	private IEnumerator HammerAttackIE()
	{
		attacking = true;
		TargetWarning.gameObject.SetActive(true);
		simpleSpineAnimator.Animate("attack-charge", 0, false);
		ClearPaths();
		float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		LookAtAngle(angle);
		float t = 0f;
		while (t < hammerAttackAnticipation)
		{
			float amt = t / hammerAttackAnticipation;
			simpleSpineFlash.FlashWhite(amt);
			t += Time.deltaTime;
			yield return null;
		}
		simpleSpineFlash.FlashWhite(false);
		simpleSpineAnimator.Animate("attack-impact", 0, false);
		simpleSpineAnimator.FlashWhite(false);
		CameraManager.instance.ShakeCameraForDuration(0.2f, 0.5f, 0.3f);
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		if (!string.IsNullOrEmpty(areaAttackSoundPath))
		{
			AudioManager.Instance.PlayOneShot(areaAttackSoundPath, base.transform.position);
		}
		GameManager.GetInstance().HitStop();
		ParticleSystem.Play();
		TargetWarning.gameObject.SetActive(false);
		damageColliderEvents.SetActive(true);
		yield return new WaitForSeconds(0.1f);
		damageColliderEvents.SetActive(false);
		yield return new WaitForSeconds(hammerAttackCooldown);
		attacking = false;
		timestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
	}

	private void Spawn()
	{
		StartCoroutine(SpawnIE());
	}

	private IEnumerator SpawnIE()
	{
		attacking = true;
		simpleSpineAnimator.Animate(spawnAnimation, 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		ClearPaths();
		yield return new WaitForSeconds(spawnAnticipation);
		SpawnEnemies(spawnSets[UnityEngine.Random.Range(0, spawnSets.Length)]);
		yield return new WaitForSeconds(spawnCooldown);
		attacking = false;
		timestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
	}

	private void SpawnEnemies(SpawnSet set)
	{
		for (int i = 0; i < set.Spawnables.Length; i++)
		{
			EnemySpawner.Create(set.Positions[i], base.transform.parent, set.Spawnables[i].gameObject);
		}
	}

	public override IEnumerator ChasePlayer()
	{
		yield break;
	}

	private void OnDrawGizmos()
	{
		SpawnSet[] array = spawnSets;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3[] positions = array[i].Positions;
			for (int j = 0; j < positions.Length; j++)
			{
				Utils.DrawCircleXY(positions[j], 0.5f, Color.green);
			}
		}
	}
}
