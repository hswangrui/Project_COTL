using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MortarBomb : BaseMonoBehaviour
{
	public GameObject BombVisual;

	public SpriteRenderer Target;

	public SpriteRenderer TargetWarning;

	public CircleCollider2D circleCollider2D;

	public GameObject BombShadow;

	public ParticleSystem SmokeParticles;

	private DOTweenAnimation rotationAnimation;

	private float moveDuration = 1f;

	public float arcHeight = 2f;

	public AnimationCurve arcCurve;

	private Health.Team bombTeam;

	public bool destroyOnFinish = true;

	private void OnEnable()
	{
		StartCoroutine(ScaleCircle());
		BombVisual.SetActive(false);
		BombShadow.SetActive(false);
	}

	private void OnDisable()
	{
		if (destroyOnFinish)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Play(Vector3 Position, float moveDuration, Health.Team bombTeam)
	{
		this.moveDuration = moveDuration;
		this.bombTeam = bombTeam;
		StartCoroutine(MoveRock(Position));
		StartCoroutine(FlashCircle());
		AudioManager.Instance.PlayOneShot("event:/enemy/fly_spawn", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/frog_large/attack", base.gameObject);
	}

	private IEnumerator ScaleCircle()
	{
		float Scale = 0f;
		while (true)
		{
			float num;
			Scale = (num = Scale + Time.deltaTime * 8f);
			if (num <= 1f)
			{
				Target.transform.localScale = Vector3.one * circleCollider2D.radius * Scale;
				TargetWarning.transform.localScale = Vector3.one * circleCollider2D.radius * Scale;
				yield return null;
				continue;
			}
			break;
		}
	}

	private void Awake()
	{
		rotationAnimation = BombVisual.GetComponent<DOTweenAnimation>();
	}

	private void Update()
	{
		HandleRotationAnimation();
		Target.transform.Rotate(new Vector3(0f, 0f, 150f) * Time.deltaTime);
	}

	private void HandleRotationAnimation()
	{
		if (!(rotationAnimation == null))
		{
			if (PlayerRelic.TimeFrozen)
			{
				rotationAnimation.DOPause();
			}
			else if (!rotationAnimation.tween.IsPlaying())
			{
				rotationAnimation.DOPlay();
			}
		}
	}

	private IEnumerator FlashCircle()
	{
		while (Vector2.Distance(BombVisual.transform.localPosition, Vector3.zero) >= 6f)
		{
			yield return null;
		}
		Color white = new Color(1f, 1f, 1f, 1f);
		Color color = white;
		float flashTickTimer = 0f;
		while (Vector2.Distance(BombVisual.transform.localPosition, Vector3.zero) < 6f)
		{
			if (flashTickTimer >= 0.12f && Time.timeScale == 1f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				Material material = Target.material;
				Color value;
				color = (value = ((color == white) ? Color.red : white));
				material.SetColor("_Color", value);
				TargetWarning.material.SetColor("_Color", color);
				flashTickTimer = 0f;
			}
			flashTickTimer += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator MoveRock(Vector3 startPos)
	{
		BombVisual.SetActive(true);
		BombShadow.SetActive(true);
		BombVisual.transform.position = startPos;
		Vector2 targetPos = base.transform.position;
		Mathf.Max(moveDuration, moveDuration * Vector2.Distance(startPos, targetPos) / 3f);
		float t = 0f;
		while (t < moveDuration)
		{
			if (!PlayerRelic.TimeFrozen)
			{
				t += Time.deltaTime;
				BombVisual.transform.position = Vector3.Lerp(startPos, targetPos, t / moveDuration);
				BombShadow.transform.position = Vector3.Lerp(startPos, targetPos, t / moveDuration);
				BombShadow.transform.localScale = Vector3.one * circleCollider2D.radius * (1.5f - Mathf.Clamp01(arcCurve.Evaluate(t / moveDuration)) * 0.5f);
				BombVisual.transform.position += new Vector3(0f, 0f, (0f - arcCurve.Evaluate(t / moveDuration)) * arcHeight);
			}
			yield return null;
		}
		Explosion.CreateExplosion(base.transform.position, bombTeam, null, circleCollider2D.radius, 1f);
		if (SmokeParticles != null)
		{
			SmokeParticles.transform.parent = base.transform.parent;
			SmokeParticles.Stop();
		}
		if (destroyOnFinish)
		{
			Object.Destroy(base.gameObject);
		}
		else if (this != null)
		{
			base.gameObject.Recycle();
		}
	}
}
