using System.Collections;
using UnityEngine;

public class TennisBall : MonoBehaviour
{
	[HideInInspector]
	public Health targetUnit;

	[HideInInspector]
	public Health ownerUnit;

	public Health health;

	public float turnSpeedBase = 16f;

	public float speedBase = 5f;

	public float speedMax = 8f;

	public float speedVolleyIncrease = 0.25f;

	public AnimationCurve volleyBurst;

	public float volleyBurstMultiplier = 6f;

	public float lifetimePerVolley = 10f;

	private float lifetime;

	private float currentAngle;

	private float speed;

	private float turnSpeed;

	[HideInInspector]
	public float invincibleTime;

	public Transform nav;

	public TrailRenderer goodTrail;

	public TrailRenderer badTrail;

	public TrailRenderer heavyTrail;

	public ParticleSystem vfxHitExplosion;

	public Transform tennisballSprite;

	[HideInInspector]
	public bool isActive;

	[HideInInspector]
	public bool destroyed = true;

	[HideInInspector]
	public bool heavy = true;

	private float volleyCount;

	private Health firstOwner;

	private void OnDestroy()
	{
		if (health != null)
		{
			health.OnHit -= OnHit;
		}
	}

	private void OnDisable()
	{
		InstantRemoveTennisBall();
	}

	public void Init(Health firstTarget, Health firstOwnerVar)
	{
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnHit += OnHit;
		}
		firstOwner = firstOwnerVar;
		base.transform.rotation = Quaternion.identity;
		SetTarget(firstTarget, firstOwnerVar);
		turnSpeed = turnSpeedBase;
		speed = speedBase;
		heavy = false;
		volleyCount = 0f;
		invincibleTime = 0.125f;
		Launch(firstOwner, firstTarget);
	}

	public void SetTarget(Health target, Health newOwner = null)
	{
		targetUnit = target;
		if ((bool)newOwner)
		{
			ownerUnit = newOwner;
		}
		bool flag = target.isPlayer || target.GetComponent<EnemyArcherTennis>() != null;
		bool flag2 = ownerUnit.isPlayer || ownerUnit.GetComponent<EnemyArcherTennis>() != null;
		Debug.Log("Target player or tennis? " + flag);
		if (!flag || !flag2)
		{
			Debug.Log("Somethign has gone wrong");
		}
	}

	private void SetTrailTeam()
	{
		goodTrail.emitting = ownerUnit.isPlayer && !heavy;
		badTrail.emitting = !ownerUnit.isPlayer && !heavy;
		heavyTrail.emitting = heavy;
	}

	public void Volley(Health attacker, bool heavyAttack = false)
	{
		if (attacker != ownerUnit)
		{
			volleyCount += 1f;
			SetTarget(ownerUnit, attacker);
			turnSpeed += 2f;
			speed += 1f;
			if (speed > speedMax)
			{
				speed = speedMax;
			}
			heavy = heavyAttack;
			if (heavy)
			{
				speed = speedMax;
			}
			Launch(ownerUnit, targetUnit);
		}
	}

	private void Launch(Health ownerUnitVar, Health targetUnitVar)
	{
		ownerUnit = ownerUnitVar;
		targetUnit = targetUnitVar;
		lifetime = 0f;
		nav.transform.LookAt(targetUnit.transform);
		SetTrailTeam();
		destroyed = false;
		isActive = true;
		base.gameObject.SetActive(true);
		tennisballSprite.gameObject.SetActive(true);
	}

	private void RemoveTennisBall()
	{
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnHit -= OnHit;
		}
		destroyed = true;
		tennisballSprite.gameObject.SetActive(false);
		StartCoroutine(RemoveTennisBallCoroutine());
	}

	private IEnumerator RemoveTennisBallCoroutine()
	{
		yield return new WaitForSeconds(1f);
		InstantRemoveTennisBall();
	}

	private void InstantRemoveTennisBall()
	{
		isActive = false;
		base.gameObject.SetActive(false);
	}

	private void FixedUpdate()
	{
		if (PlayerRelic.TimeFrozen || destroyed)
		{
			return;
		}
		invincibleTime -= Time.deltaTime;
		nav.localPosition = Vector3.zero;
		Quaternion rotation = nav.rotation;
		nav.LookAt(targetUnit.transform);
		nav.rotation = Quaternion.Lerp(rotation, nav.rotation, Time.deltaTime * turnSpeed);
		float num = volleyBurst.Evaluate(lifetime) * volleyBurstMultiplier;
		float num2 = speed + num;
		Vector3 vector = nav.forward * num2 * Time.deltaTime;
		nav.position += vector;
		base.transform.position = nav.position;
		float num3 = Vector3.Distance(base.transform.position, targetUnit.transform.position);
		float num4 = 0.4f;
		if (num3 < num4)
		{
			if (targetUnit == PlayerFarming.Instance.health)
			{
				Debug.Log("Is player");
				if (num3 < num4 / 2f)
				{
					Debug.Log("Is player vulnerable " + targetUnit.state.CURRENT_STATE);
					targetUnit.DealDamage(1f, base.gameObject, base.transform.position, true);
					vfxHitExplosion.Play();
					RemoveTennisBall();
				}
			}
			else if (targetUnit == firstOwner)
			{
				Debug.Log("Is not player, ask them if they'll swipe");
				EnemyArcherTennis component = targetUnit.GetComponent<EnemyArcherTennis>();
				if (component != null)
				{
					float num5 = component.chanceOfMissingTennisReturnBase + volleyCount * component.chanceOfMissingTennisReturnIncrease;
					if (Random.value < num5 || heavy)
					{
						Cower component2 = component.GetComponent<Cower>();
						if (component2 != null)
						{
							component2.enabled = true;
							component2.preventStandardStagger = false;
							component2.Health_OnHit(base.gameObject, base.transform.position, Health.AttackTypes.Projectile, false);
							Vector3 position = base.transform.position;
							position.y -= 0.5f;
							position.z -= 0.75f;
							BiomeConstants.Instance.EmitBlockImpact(position, component.Angle, base.transform, "Break");
						}
						else
						{
							component.OnHit(base.gameObject, base.transform.position, Health.AttackTypes.Projectile, false);
						}
						component.ReturnFire(false);
						RemoveTennisBall();
						vfxHitExplosion.Play();
					}
					else
					{
						component.ReturnFire(true);
						Volley(targetUnit);
					}
				}
				else
				{
					Debug.Log("Tennis ball got confused about owner");
				}
			}
		}
		lifetime += Time.deltaTime;
		if (lifetime > lifetimePerVolley)
		{
			RemoveTennisBall();
		}
	}

	protected void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (isActive && !destroyed && !(invincibleTime > 0f))
		{
			UnitObject component = Attacker.GetComponent<UnitObject>();
			if (component.health.isPlayer && component.health == targetUnit && component.health.state.CURRENT_STATE != StateMachine.State.Dodging && (AttackType == Health.AttackTypes.Melee || AttackType == Health.AttackTypes.Heavy || AttackType == Health.AttackTypes.Projectile))
			{
				Volley(component.health, AttackType == Health.AttackTypes.Heavy);
				AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
				CameraManager.shakeCamera(5f);
				Vector2 vector = component.transform.position - base.transform.position;
				StartCoroutine(RecoilUnit(component, vector, 0.25f, 0.1f));
			}
		}
	}

	private IEnumerator RecoilUnit(UnitObject unit, Vector3 dir, float power, float duration)
	{
		float elapsedTime = 0f;
		dir.Normalize();
		while (elapsedTime < duration)
		{
			if (SimulationManager.IsPaused)
			{
				yield return null;
			}
			elapsedTime += Time.deltaTime;
			unit.moveVX = dir.x * power;
			unit.moveVY = dir.y * power;
			yield return null;
		}
	}
}
