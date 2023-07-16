using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyMillipedeSplitterMiniboss : EnemyMillipedeSpiker
{
	[SerializeField]
	private float speedIncrementPerHit;

	[SerializeField]
	private GameObject projectile;

	[SerializeField]
	private float shootAnticipation;

	[SerializeField]
	private Vector2 amountToShoot;

	[SerializeField]
	private Vector2 delayBetweenShots;

	[SerializeField]
	private float shootingCooldown;

	[SerializeField]
	private float gravSpeed;

	[SerializeField]
	private float shootOffset;

	[SerializeField]
	private GameObject shootBone;

	[SerializeField]
	private float height;

	[SerializeField]
	private float duration;

	[SerializeField]
	private float partRadius;

	[SerializeField]
	private AnimationCurve heightCurve;

	[SerializeField]
	private LayerMask avoidMask;

	[SerializeField]
	private AssetReferenceGameObject[] enemies;

	[Space]
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnimation;

	private List<MillipedeBodyPart> parts;

	private float startingHP;

	private int totalBodyParts;

	private float shootTimestamp;

	public override void Awake()
	{
		base.Awake();
		parts = GetComponentsInChildren<MillipedeBodyPart>().ToList();
		totalBodyParts = parts.Count - 1;
		startingHP = health.totalHP;
	}

	public override void Update()
	{
		base.Update();
		GameManager instance = GameManager.GetInstance();
		if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > shootTimestamp / Spine.timeScale && !attacking)
		{
			StartCoroutine(ShootProjectiles());
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		float num = startingHP - health.HP;
		float num2 = startingHP / (float)totalBodyParts;
		int num3 = Mathf.RoundToInt(num / num2);
		int num4 = totalBodyParts - num3;
		maxSpeed += (float)(parts.Count - num4) * speedIncrementPerHit;
		turnDamper -= (float)(parts.Count - num4) * 0.05f;
		if (parts.Count > num4 && parts.Count > 1)
		{
			for (int i = num4; i < parts.Count; i++)
			{
				DropBodyPart(parts[i]);
			}
			parts.RemoveRange(num4, parts.Count - num4);
		}
	}

	private void DropBodyPart(MillipedeBodyPart bodyPart)
	{
		bodyPart.GetComponent<FollowAsTail>().enabled = false;
		bodyPart.GetComponent<Health>().enabled = false;
		flashes.RemoveAt(flashes.Count - 1);
		bodyPart.DroppedPart();
		spines.Remove(bodyPart.GetComponent<SkeletonAnimation>());
		StartCoroutine(ThrowBodyPart(bodyPart));
	}

	private IEnumerator ThrowBodyPart(MillipedeBodyPart bodyPart)
	{
		Vector3 fromPosition = bodyPart.transform.position;
		Vector3 targetPosition = GetRandomPosition();
		targetPosition.z = 0f;
		float t = 0f;
		while (t < duration)
		{
			float num = t / duration;
			Vector3 position = Vector3.Lerp(fromPosition, targetPosition, num);
			position.z = heightCurve.Evaluate(num) * height * -1f;
			bodyPart.transform.position = position;
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		bodyPart.SpawnEnemy(enemies, base.transform.parent);
	}

	private Vector3 GetRandomPosition()
	{
		return (Vector3)Random.insideUnitCircle * 4f;
	}

	private IEnumerator ShootProjectiles()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/warning", base.gameObject);
		DisableForces = true;
		attacking = true;
		SetAnimation(shootAnticipationAnimation, true);
		yield return new WaitForEndOfFrame();
		moveVX = 0f;
		moveVY = 0f;
		float t = 0f;
		while (t < shootAnticipation)
		{
			float amt = t / shootAnticipation;
			foreach (SimpleSpineFlash flash in flashes)
			{
				flash.FlashWhite(amt);
			}
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		foreach (SimpleSpineFlash flash2 in flashes)
		{
			flash2.FlashWhite(false);
		}
		SetAnimation(shootAnimation);
		AddAnimation(idleAnimation, true);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.4f);
		int i = -1;
		int total = (int)Random.Range(amountToShoot.x, amountToShoot.y + 1f);
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= total)
			{
				break;
			}
			float angle = Random.Range(0f, 360f);
			float num2 = Random.Range(2f, 3f);
			if (GetClosestTarget() != null)
			{
				num2 = Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) / 1.5f + Random.Range(-2f, 2f);
				angle = Mathf.Repeat(Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) + Random.Range(-20f, 20f), 360f);
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", AudioManager.Instance.Listener);
			Vector3 position = shootBone.transform.position;
			ObjectPool.Spawn(projectile, position, Quaternion.identity).GetComponent<GrenadeBullet>().Play(shootOffset, angle, num2, gravSpeed);
			float dur = Random.Range(delayBetweenShots.x, delayBetweenShots.y);
			float time = 0f;
			while (true)
			{
				float num3;
				time = (num3 = time + Time.deltaTime * Spine.timeScale);
				if (!(num3 < dur))
				{
					break;
				}
				yield return null;
			}
		}
		shootTimestamp = GameManager.GetInstance().CurrentTime + shootingCooldown;
		attacking = false;
		DisableForces = false;
	}
}
