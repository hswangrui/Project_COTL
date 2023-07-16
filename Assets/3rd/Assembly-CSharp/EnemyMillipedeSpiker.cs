using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyMillipedeSpiker : EnemyMillipede
{
	[SerializeField]
	private float anticipation;

	[SerializeField]
	private float attackDistance;

	[SerializeField]
	private float cooldown;

	[SerializeField]
	private bool stopMovingOnAttack;

	[Space]
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackAnimation;

	protected bool attacking;

	public override void OnEnable()
	{
		base.OnEnable();
		attacking = false;
	}

	public override void Update()
	{
		if (CanAttack())
		{
			StartCoroutine(Attack());
		}
		base.Update();
	}

	protected bool CanAttack()
	{
		if (!attacking && (bool)GetClosestTarget())
		{
			return Vector3.Distance(GetCenterPosition(), GetClosestTarget().transform.position) < attackDistance;
		}
		return false;
	}

	private IEnumerator Attack()
	{
		attacking = true;
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/warning", base.gameObject);
		SetAnimation(attackAnticipationAnimation, true);
		yield return new WaitForEndOfFrame();
		if (stopMovingOnAttack)
		{
			moveVX = 0f;
			moveVY = 0f;
		}
		float t = 0f;
		while (t < anticipation)
		{
			float amt = t / anticipation;
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
		SetAnimation(attackAnimation);
		AddAnimation(idleAnimation, true);
		EnableDamageColliders();
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/attack", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < cooldown))
			{
				break;
			}
			yield return null;
		}
		attacking = false;
	}
}
