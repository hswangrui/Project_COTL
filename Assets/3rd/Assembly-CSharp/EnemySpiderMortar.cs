using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemySpiderMortar : EnemySpider
{
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnimation;

	[SerializeField]
	private EnemyBomb shootPrefab;

	[SerializeField]
	private float shootAnticipation;

	[SerializeField]
	private float shootDuration;

	[SerializeField]
	private float shootCooldown;

	[SerializeField]
	private float minDistanceToShoot;

	[SerializeField]
	private float timeBetweenShooting;

	[SerializeField]
	private bool shootAtTarget;

	private float shootTimestamp;

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
		if (shootAtTarget)
		{
			LookAtTarget();
		}
		AudioManager.Instance.PlayOneShot(warningSfx, base.transform.position);
		yield return new WaitForEndOfFrame();
		if ((bool)Body.Spine)
		{
			Body.Spine.skeleton.ScaleX = 1f;
		}
		Body.transform.localScale = new Vector3(Spine.skeleton.ScaleX, 1f, 1f);
		float t = 0f;
		while (t < shootAnticipation)
		{
			float amt = t / shootAnticipation;
			SimpleSpineFlash.FlashWhite(amt);
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		SetAnimation(shootAnimation);
		AddAnimation(IdleAnimation, true);
		state.CURRENT_STATE = StateMachine.State.Attacking;
		Vector3 position = ((shootAtTarget && TargetEnemy != null) ? TargetEnemy.transform.position : ((Vector3)Random.insideUnitCircle * 5f));
		Object.Instantiate(shootPrefab, position, Quaternion.identity, base.transform.parent).Play(base.transform.position, shootDuration);
		AudioManager.Instance.PlayOneShot(attackSfx, base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.gameObject);
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
		shootTimestamp = GameManager.GetInstance().CurrentTime + timeBetweenShooting;
		updateDirection = true;
	}
}
