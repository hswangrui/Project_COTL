using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FrogBossTongue : UnitObject
{
	[SerializeField]
	private Transform tongueTip;

	[SerializeField]
	private Transform tongueActualTip;

	[SerializeField]
	private Transform tongueBase;

	[SerializeField]
	private ColliderEvents tongueTipCollider;

	[SerializeField]
	private GameObject targetObject;

	[SerializeField]
	private AnimationCurve moveInCurve;

	[SerializeField]
	private AnimationCurve moveOutCurve;

	[SerializeField]
	private Renderer[] renderers;

	[SerializeField]
	private ParticleSystem tongueAOEParticles;

	private EnemyFrogBoss boss;

	private LineRenderer lineRenderer;

	private CircleCollider2D tipCollider;

	private Color[] originalColors;

	public override void Awake()
	{
		base.Awake();
		lineRenderer = GetComponent<LineRenderer>();
		boss = GetComponentInParent<EnemyFrogBoss>();
		originalColors = new Color[renderers.Length];
		tipCollider = tongueTipCollider.GetComponent<CircleCollider2D>();
		for (int i = 0; i < renderers.Length; i++)
		{
			originalColors[i] = renderers[i].material.color;
		}
		tongueTipCollider.OnTriggerEnterEvent += OnTriggerEnterEvent;
	}

	public void LateUpdate()
	{
		lineRenderer.SetPosition(0, boss.TonguePosition.transform.position);
		lineRenderer.SetPosition(1, tongueTip.position);
	}

	public IEnumerator SpitTongueIE(Vector3 targetPosition, float delay, float moveDuration, float waitDelay, float retrieveDuration)
	{
		Vector3 dir = (targetPosition - boss.TonguePosition.transform.position).normalized;
		dir.z = 0f;
		tongueActualTip.transform.localPosition = new Vector3(0f, 0f, -0.6f);
		tongueActualTip.transform.localScale = Vector3.zero;
		base.gameObject.SetActive(true);
		targetObject.SetActive(true);
		targetObject.transform.position = targetPosition;
		tongueTip.transform.position = boss.TonguePosition.transform.position;
		tongueTip.gameObject.SetActive(false);
		yield return new WaitForSeconds(delay);
		tongueTip.gameObject.SetActive(true);
		tongueActualTip.transform.DOScale(1f, 1f).SetEase(Ease.OutBack);
		float t2 = 0f;
		while (t2 < moveDuration + 0.1f)
		{
			float time = t2 / (moveDuration + 0.1f);
			tongueTip.transform.position = Vector3.Lerp(boss.TonguePosition.transform.position, targetPosition + dir * 0.3f, moveInCurve.Evaluate(time));
			t2 += Time.deltaTime;
			yield return null;
		}
		targetObject.SetActive(false);
		StartCoroutine(TurnOnDamageColliderForDuration(tongueTipCollider.gameObject, 0.25f));
		tongueAOEParticles.Play();
		CameraManager.instance.ShakeCameraForDuration(1f, 1f, 0.2f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_impact");
		yield return new WaitForSeconds(waitDelay);
		tongueActualTip.transform.DOScale(0f, 1f).SetEase(Ease.InBack);
		t2 = 0f;
		while (t2 < retrieveDuration)
		{
			float time2 = t2 / retrieveDuration;
			tongueTip.transform.position = Vector3.Lerp(tongueTip.transform.position, boss.TonguePosition.transform.position, moveOutCurve.Evaluate(time2));
			t2 += Time.deltaTime;
			yield return null;
		}
		base.gameObject.SetActive(false);
	}

	public void DealDamageToBoss(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		boss.health.DealDamage(PlayerWeapon.GetDamage(1f, DataManager.Instance.CurrentWeaponLevel) * 1.5f, Attacker, AttackLocation, false, AttackType);
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = Color.red;
			renderers[i].material.DOColor(originalColors[i], 0.25f);
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		DealDamageToBoss(Attacker, AttackLocation, AttackType, FromBehind);
	}

	private IEnumerator TurnOnDamageColliderForDuration(GameObject collider, float duration)
	{
		collider.SetActive(true);
		Collider2D[] array = Physics2D.OverlapCircleAll(tipCollider.transform.position, tipCollider.radius);
		for (int i = 0; i < array.Length; i++)
		{
			UnitObject component = array[i].GetComponent<UnitObject>();
			if ((bool)component && component.health.team == Health.Team.Team2 && component != boss)
			{
				component.DoKnockBack(collider.gameObject, 1f, 0.5f);
			}
		}
		yield return new WaitForSeconds(duration);
		collider.SetActive(false);
	}

	private void OnTriggerEnterEvent(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}
}
