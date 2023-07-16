using System;
using System.Collections.Generic;
using UnityEngine;

public class Swipe : BaseMonoBehaviour
{
	private SpriteRenderer spriterenderer;

	private float alpha = 100f;

	private Color color;

	private float timer;

	private bool disabled;

	public Health Origin;

	public Health.Team team;

	public Action<Health, Health.AttackTypes> CallBack;

	private float Damage;

	private float CritChance;

	public float radius = 1f;

	private Health.AttackTypes AttackType;

	private Health.AttackFlags AttackFlags;

	public Collider2D damageCollider;

	public float Duration = 0.1f;

	private float frameTime;

	private List<Health> objsHitThisFrame = new List<Health>();

	public void Init(Vector3 Position, float Angle, Health.Team team, Health Origin, Action<Health, Health.AttackTypes> CallBack, float Radius, float Damage = 1f, float CritChance = 0f, Health.AttackTypes AttackType = Health.AttackTypes.Melee, Health.AttackFlags AttackFlags = (Health.AttackFlags)0)
	{
		base.transform.position = Position;
		base.transform.eulerAngles = new Vector3(0f, 0f, Angle);
		if (Angle > 90f || Angle < -90f)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y * -1f, base.transform.localScale.z);
		}
		this.team = team;
		this.Origin = Origin;
		this.CallBack = CallBack;
		CircleCollider2D circleCollider2D = damageCollider as CircleCollider2D;
		if (circleCollider2D != null)
		{
			circleCollider2D.radius = Radius;
		}
		this.Damage = Damage;
		this.CritChance = Mathf.Clamp01(CritChance);
		this.AttackType = AttackType;
		this.AttackFlags = AttackFlags;
		Invoke("DestroySelf", Duration);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		Health component = collider.gameObject.GetComponent<Health>();
		if (objsHitThisFrame.Count == 0)
		{
			frameTime = GameManager.GetInstance().CurrentTime;
		}
		if (GameManager.GetInstance().CurrentTime - frameTime < 0.1f)
		{
			if (objsHitThisFrame.Contains(component))
			{
				return;
			}
			objsHitThisFrame.Add(component);
		}
		else
		{
			objsHitThisFrame.Clear();
		}
		if (component != null && component.enabled && component != Origin && Origin != null && (team != component.team || Origin == PlayerFarming.Instance.health))
		{
			Health.AttackFlags attackFlags = AttackFlags;
			float num = Damage;
			if (PlayerFarming.Instance.playerWeapon.CriticalHitCharged || UnityEngine.Random.Range(0f, 1f) < CritChance)
			{
				num *= 3f;
				attackFlags |= Health.AttackFlags.Crit;
			}
			if (component.HasShield && PlayerFarming.Instance.playerWeapon.DoingHeavyAttack)
			{
				Debug.Log("Attack type switched to heavy for shielded enemy!");
				AttackType = Health.AttackTypes.Heavy;
			}
			component.DealDamage(num, Origin.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.8f), false, AttackType, false, attackFlags);
			Action<Health, Health.AttackTypes> callBack = CallBack;
			if (callBack != null)
			{
				callBack(component, AttackType);
			}
		}
	}

	private void DestroySelf()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (PlayerFarming.Instance != null && PlayerFarming.Instance.playerWeapon.CriticalHitCharged)
		{
			PlayerWeapon.CriticalHitTimer = 0f;
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, radius, Color.red);
	}
}
