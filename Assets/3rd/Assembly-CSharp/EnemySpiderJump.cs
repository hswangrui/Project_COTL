using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class EnemySpiderJump : EnemySpider
{
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string swinAwayAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string swinInAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string StuckAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string UnstuckAnimation;

	[SerializeField]
	private float slamAttackMinRange;

	[SerializeField]
	private float timeBetweenSlams;

	[SerializeField]
	private float slamInAirDuration;

	[SerializeField]
	private float slamLandDuration;

	[SerializeField]
	private float slamCooldown;

	[SerializeField]
	private float randomOffsetRadius = 0.25f;

	[SerializeField]
	private float screenShake;

	[SerializeField]
	private GameObject slamParticlePrefab;

	[SerializeField]
	private SpriteRenderer indicatorIcon;

	private float SlamTimer;

	private float flashTickTimer;

	private Color indicatorColor = Color.red;

	private ShowHPBar hpBar;

	public override void OnEnable()
	{
		SlamTimer = timeBetweenSlams + Random.Range(0f, 3f);
		hpBar = GetComponent<ShowHPBar>();
		health.DontCombo = false;
		SpriteRenderer spriteRenderer = indicatorIcon;
		if ((object)spriteRenderer != null)
		{
			spriteRenderer.gameObject.SetActive(false);
		}
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		StopAllCoroutines();
	}

	protected override bool ShouldAttack()
	{
		if ((SlamTimer -= Time.deltaTime) < 0f && TargetEnemy != null && !base.Attacking && Vector3.Distance(base.transform.position, TargetEnemy.transform.position) > slamAttackMinRange)
		{
			return GameManager.GetInstance().CurrentTime > initialAttackDelayTimer;
		}
		return false;
	}

	public override void Update()
	{
		base.Update();
		if (Time.timeScale == 0f)
		{
			return;
		}
		base.AttackingTargetPosition = base.transform.position - Vector3.up;
		if (indicatorIcon.gameObject.activeSelf)
		{
			if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				indicatorColor = ((indicatorColor == Color.white) ? Color.red : Color.white);
				indicatorIcon.material.SetColor("_Color", indicatorColor);
				flashTickTimer = 0f;
			}
			else
			{
				flashTickTimer += Time.deltaTime;
			}
		}
	}

	private void Slam()
	{
		StartCoroutine(AttackRoutine());
	}

	protected override IEnumerator AttackRoutine()
	{
		Spine.ForceVisible = true;
		base.Attacking = true;
		updateDirection = false;
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.Attacking;
		SetAnimation(swinAwayAnimation);
		AudioManager.Instance.PlayOneShot(warningSfx, base.transform.position);
		Vector3 targetShadowScale = Vector3.zero;
		if ((bool)ShadowSpriteRenderer)
		{
			targetShadowScale = ShadowSpriteRenderer.transform.localScale;
			ShadowSpriteRenderer.transform.DOScale(0f, 1f);
		}
		yield return new WaitForEndOfFrame();
		if ((bool)Body.Spine)
		{
			Body.Spine.skeleton.ScaleX = 1f;
		}
		Body.transform.localScale = new Vector3(Spine.skeleton.ScaleX, 1f, 1f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		hpBar.Hide();
		health.invincible = true;
		AudioManager.Instance.PlayOneShot(jumpSfx, base.transform.position);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < (slamInAirDuration - 1f) / 2f))
			{
				break;
			}
			yield return null;
		}
		ColliderRadius.enabled = false;
		indicatorIcon.gameObject.SetActive(true);
		Vector3 vector = ((TargetEnemy != null) ? TargetEnemy.transform.position : base.transform.position) + (Vector3)Random.insideUnitCircle * randomOffsetRadius;
		Vector3 normalized = (vector - base.transform.position).normalized;
		float distance = Vector3.Distance(vector, base.transform.position);
		if (Physics2D.Raycast(base.transform.position, normalized, distance, layerToCheck).collider != null)
		{
			vector = ((TargetEnemy != null) ? TargetEnemy.transform.position : base.transform.position);
		}
		base.transform.position = vector;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < (slamInAirDuration - 1f) / 2f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(attackSfx, base.transform.position);
		SpriteRenderer shadowSpriteRenderer = ShadowSpriteRenderer;
		if ((object)shadowSpriteRenderer != null)
		{
			shadowSpriteRenderer.transform.DOScale(targetShadowScale, slamLandDuration);
		}
		SetAnimation(swinInAnimation);
		AddAnimation(StuckAnimation);
		AddAnimation(UnstuckAnimation);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < slamLandDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(stuckSfx, base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/impact_blunt", base.transform.position);
		indicatorIcon.gameObject.SetActive(false);
		CameraManager.instance.ShakeCameraForDuration(screenShake, screenShake, 0.1f);
		Object.Instantiate(slamParticlePrefab, base.transform.position, Quaternion.identity);
		ColliderRadius.enabled = true;
		damageColliderEvents.gameObject.SetActive(true);
		health.invincible = false;
		health.DontCombo = true;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.gameObject.SetActive(false);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < slamCooldown - 0.5f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(breakFreeSfx, base.transform.position);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		health.DontCombo = false;
		TargetEnemy = null;
		IdleWait = 0f;
		SlamTimer = timeBetweenSlams;
		base.Attacking = false;
		updateDirection = true;
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.ForceVisible = false;
	}

	public override void DoKnockBack(GameObject Attacker, float KnockbackModifier, float Duration, bool appendForce = true)
	{
		if (state.CURRENT_STATE != StateMachine.State.Attacking)
		{
			base.DoKnockBack(Attacker, KnockbackModifier, Duration, appendForce);
		}
	}

	public override void DoKnockBack(float angle, float KnockbackModifier, float Duration, bool appendForce = true)
	{
		if (state.CURRENT_STATE != StateMachine.State.Attacking)
		{
			base.DoKnockBack(angle, KnockbackModifier, Duration, appendForce);
		}
	}
}
