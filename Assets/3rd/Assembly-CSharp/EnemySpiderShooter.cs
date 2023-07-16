using System.Collections;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;

public class EnemySpiderShooter : EnemySpider
{
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnimation;

	[SerializeField]
	private float shootAnticipation;

	[SerializeField]
	private float shootDuration;

	[SerializeField]
	private float shootCooldown;

	[SerializeField]
	private float minDistanceToShoot;

	[SerializeField]
	private Vector2 timeBetweenShooting;

	private float shootTimestamp;

	private ProjectilePattern projectilePattern;

	private void Start()
	{
		projectilePattern = GetComponentInChildren<ProjectilePattern>();
	}

	public override void Update()
	{
		base.Update();
		if (ShouldShoot())
		{
			Shoot();
		}
	}

	private bool ShouldShoot()
	{
		if (state.CURRENT_STATE == StateMachine.State.Idle)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > shootTimestamp && (bool)TargetEnemy && Vector3.Distance(base.transform.position, TargetEnemy.transform.position) > minDistanceToShoot)
			{
				return GameManager.RoomActive;
			}
		}
		return false;
	}

	protected override bool ShouldAttack()
	{
		return false;
	}

	private void Shoot()
	{
		StartCoroutine(ShootIE());
	}

	private IEnumerator ShootIE()
	{
		updateDirection = false;
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		ClearPaths();
		SetAnimation(shootAnticipationAnimation);
		LookAtTarget();
		AudioManager.Instance.PlayOneShot(warningSfx, base.transform.position);
		yield return new WaitForEndOfFrame();
		float t = 0f;
		while (t < shootAnticipation)
		{
			float amt = t / shootAnticipation;
			SimpleSpineFlash.FlashWhite(amt);
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		LookAtTarget();
		SetAnimation(shootAnimation);
		AddAnimation(IdleAnimation, true);
		state.CURRENT_STATE = StateMachine.State.Attacking;
		AudioManager.Instance.PlayOneShot(attackSfx, base.transform.position);
		projectilePattern.Shoot(0.1f, TargetEnemy ? TargetEnemy.gameObject : null, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < shootCooldown))
			{
				break;
			}
			yield return null;
		}
		TargetEnemy = null;
		state.CURRENT_STATE = StateMachine.State.Idle;
		shootTimestamp = GameManager.GetInstance().CurrentTime + Random.Range(timeBetweenShooting.x, timeBetweenShooting.y);
		updateDirection = true;
	}
}
