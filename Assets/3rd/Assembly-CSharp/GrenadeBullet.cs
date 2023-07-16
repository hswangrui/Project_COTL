using System;
using System.Collections;
using Ara;
using UnityEngine;

public class GrenadeBullet : BaseMonoBehaviour
{
	public Transform t;

	private float GravSpeed = 0.2f;

	public ColliderEvents damageColliderEvents;

	private Health.Team Team = Health.Team.Team2;

	public SpriteRenderer ShadowSpriteRenderer;

	[SerializeField]
	private SpriteRenderer indicatorIcon;

	[SerializeField]
	private AraTrail trail;

	[SerializeField]
	private TrailRenderer lowQualityTrail;

	private float time;

	private float flashTickTimer;

	private Color indicatorColor = Color.white;

	private Vector3 NewPosition;

	private Coroutine damageColliderRoutine;

	public float Angle { get; set; }

	public float Speed { get; set; }

	public float Grav { get; set; }

	private void OnEnable()
	{
		trail = indicatorIcon.transform.parent.Find("GrenadeBullet_Tail").GetComponent<AraTrail>();
		if ((bool)trail)
		{
			if ((bool)lowQualityTrail)
			{
				lowQualityTrail.enabled = false;
			}
			trail.enabled = true;
			trail.Clear();
			trail.emit = true;
		}
		damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		damageColliderEvents.SetActive(false);
	}

	private void OnDisable()
	{
		damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		damageColliderEvents.SetActive(false);
	}

	public void Play(float StartingHeight, float Angle, float Speed, float Grav, Health.Team team = Health.Team.Team2)
	{
		trail.Clear();
		t.localPosition = Vector3.forward * StartingHeight;
		time = Grav;
		this.Angle = Angle * ((float)Math.PI / 180f);
		this.Speed = Speed;
		this.Grav = Grav * (1f / 60f);
		Team = team;
		indicatorIcon.gameObject.SetActive(true);
		t.gameObject.SetActive(true);
		ShadowSpriteRenderer.enabled = true;
		StartCoroutine(MoveRoutine());
	}

	private void ResetPosition()
	{
		base.transform.position = Vector3.zero;
	}

	private void Update()
	{
		if (Time.timeScale != 0f)
		{
			if (indicatorIcon.gameObject.activeSelf && flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				indicatorColor = ((indicatorColor == Color.white) ? Color.red : Color.white);
				indicatorIcon.material.SetColor("_Color", indicatorColor);
				flashTickTimer = 0f;
			}
			flashTickTimer += Time.deltaTime;
		}
	}

	private IEnumerator MoveRoutine()
	{
		Vector3 vector = Utils.RadianToVector2(Angle);
		float num = 5.9999995f;
		num += t.localPosition.z / 2f;
		num -= 1f - Mathf.Sqrt(Mathf.Abs(t.localPosition.z));
		num = Mathf.Abs(time) / num;
		indicatorIcon.gameObject.SetActive(true);
		Vector3 targetPosition = base.transform.position + vector * (Mathf.Abs(Speed) * num);
		while (true)
		{
			if (!PlayerRelic.TimeFrozen)
			{
				NewPosition = new Vector3(Speed * Mathf.Cos(Angle), Speed * Mathf.Sin(Angle)) * Time.fixedDeltaTime;
				base.transform.position = base.transform.position + NewPosition;
				Grav += GravSpeed * Time.fixedDeltaTime;
				t.localPosition += Vector3.forward * Grav;
				indicatorIcon.transform.position = targetPosition + Vector3.back * 0.03f;
				if (t.position.z > 0f)
				{
					break;
				}
				yield return new WaitForFixedUpdate();
			}
			else
			{
				yield return null;
			}
		}
		indicatorIcon.gameObject.SetActive(false);
		t.gameObject.SetActive(false);
		ShadowSpriteRenderer.enabled = false;
		if (damageColliderRoutine != null)
		{
			StopCoroutine(damageColliderRoutine);
		}
		damageColliderRoutine = StartCoroutine(TurnOnDamageColliderForDuration(0.2f));
		GameObject obj = BiomeConstants.Instance.GrenadeBulletImpact_A.Spawn();
		obj.transform.position = t.transform.position;
		obj.transform.rotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one * 0.5f;
		CameraManager.shakeCamera(0.5f, false);
		AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile");
		StartCoroutine(DestroyAfterWait(1f));
	}

	private IEnumerator TurnOnDamageColliderForDuration(float duration)
	{
		damageColliderEvents.SetActive(true);
		yield return new WaitForSeconds(duration);
		damageColliderEvents.SetActive(false);
	}

	private IEnumerator DestroyAfterWait(float Delay)
	{
		float num = 0f;
		if ((bool)trail)
		{
			num = trail.time;
			trail.emit = false;
		}
		yield return new WaitForSeconds(Delay + num);
		base.gameObject.Recycle();
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != Team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}
}
