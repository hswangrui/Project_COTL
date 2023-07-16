using System.Collections;
using UnityEngine;

public class EnemyBomb : BaseMonoBehaviour
{
	public GameObject BombVisual;

	public SpriteRenderer Target;

	public SpriteRenderer TargetWarning;

	public CircleCollider2D circleCollider2D;

	public GameObject BombShadow;

	private float moveDuration = 1f;

	public float bombArcHeight = 2f;

	public AnimationCurve bombArcCurve;

	public bool RotateBomb;

	public Transform rotationTransform;

	public float RotationSpeed = 1f;

	protected Health health;

	private void OnEnable()
	{
		StartCoroutine(ScaleCircle());
		BombVisual.SetActive(false);
		BombShadow.SetActive(false);
	}

	public virtual void Play(Vector3 Position, float moveDuration)
	{
		this.moveDuration = moveDuration;
		StartCoroutine(MoveRock(Position));
		StartCoroutine(FlashCircle());
	}

	private void Update()
	{
		Target.transform.Rotate(new Vector3(0f, 0f, 150f) * Time.deltaTime);
	}

	private IEnumerator MoveRock(Vector3 startPos)
	{
		BombVisual.SetActive(true);
		BombShadow.SetActive(true);
		BombVisual.transform.position = startPos;
		Vector2 targetPos = base.transform.position;
		float t = 0f;
		while (t < moveDuration)
		{
			if (!PlayerRelic.TimeFrozen)
			{
				t += Time.deltaTime;
				BombVisual.transform.position = Vector3.Lerp(startPos, targetPos, t / moveDuration);
				BombShadow.transform.position = Vector3.Lerp(startPos, targetPos, t / moveDuration);
				BombShadow.transform.localScale = Vector3.one * (1.5f - Mathf.Clamp01(bombArcCurve.Evaluate(t / moveDuration)) * 0.5f);
				BombVisual.transform.position += new Vector3(0f, 0f, (0f - bombArcCurve.Evaluate(t / moveDuration)) * bombArcHeight);
				if (RotateBomb && rotationTransform != null)
				{
					Vector3 eulerAngles = Quaternion.LookRotation(new Vector3(targetPos.x, targetPos.y, 0f) - startPos, Vector3.up).eulerAngles;
					rotationTransform.transform.rotation = Quaternion.Euler(eulerAngles);
					rotationTransform.transform.Rotate(Vector3.up, 500f * RotationSpeed * t, Space.Self);
				}
			}
			yield return null;
		}
		BombLanded();
		ObjectPool.Recycle(base.gameObject);
	}

	protected virtual void BombLanded()
	{
	}

	private IEnumerator ScaleCircle()
	{
		float Scale = 0f;
		while (true)
		{
			float num;
			Scale = (num = Scale + Time.deltaTime * 8f);
			if (num <= circleCollider2D.radius)
			{
				Target.transform.localScale = Vector3.one * Scale;
				TargetWarning.transform.localScale = Vector3.one * Scale;
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator FlashCircle()
	{
		while (Vector2.Distance(BombVisual.transform.localPosition, Vector3.zero) >= 6f)
		{
			yield return null;
		}
		float flashTickTimer = 0f;
		Color white = new Color(1f, 1f, 1f, 1f);
		Color color = white;
		while (Vector2.Distance(BombVisual.transform.localPosition, Vector3.zero) < 6f)
		{
			if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
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
}
