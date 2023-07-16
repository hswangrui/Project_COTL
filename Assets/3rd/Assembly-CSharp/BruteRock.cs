using System;
using System.Collections;
using UnityEngine;

public class BruteRock : BaseMonoBehaviour
{
	public float Grav = 0.7f;

	public GameObject Rock;

	public SpriteRenderer Target;

	public SpriteRenderer TargetWarning;

	public CircleCollider2D circleCollider2D;

	private void OnEnable()
	{
		StartCoroutine(ScaleCircle());
		Rock.SetActive(false);
	}

	public void Play(Vector3 Position)
	{
		StartCoroutine(MoveRock(Position));
		StartCoroutine(FlashCircle());
	}

	private IEnumerator ScaleCircle()
	{
		float Scale = 0f;
		while (true)
		{
			float num;
			Scale = (num = Scale + Time.deltaTime * 5f);
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

	private void Update()
	{
		Target.transform.Rotate(new Vector3(0f, 0f, 150f) * Time.deltaTime);
	}

	private IEnumerator FlashCircle()
	{
		while (Vector2.Distance(Rock.transform.localPosition, Vector3.zero) >= 6f)
		{
			yield return null;
		}
		float flashTickTimer = 0f;
		Color white = new Color(1f, 1f, 1f, 1f);
		Color color = white;
		while (Vector2.Distance(Rock.transform.localPosition, Vector3.zero) < 6f)
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

	private IEnumerator MoveRock(Vector3 Position)
	{
		Rock.SetActive(true);
		Rock.transform.position = Position;
		float Speed = 7f;
		float num = Vector2.Distance(Rock.transform.localPosition, Vector3.zero);
		float num2 = 0f;
		float num3 = 0f;
		while ((num3 += Speed / 60f) < num)
		{
			num2 += Grav;
		}
		float yVel = (0f - num2) / 2f;
		float num4 = Position.z;
		while ((num4 -= yVel * Time.deltaTime) >= 0f)
		{
			yVel += Grav * GameManager.DeltaTime;
		}
		while (Vector2.Distance(Rock.transform.localPosition, Vector3.zero) > Speed * Time.deltaTime)
		{
			float f = Utils.GetAngle(Rock.transform.localPosition, Vector3.zero) * ((float)Math.PI / 180f);
			yVel += Grav * GameManager.DeltaTime;
			Vector3 vector = new Vector3(Speed * Mathf.Cos(f), Speed * Mathf.Sin(f), yVel) * Time.deltaTime;
			Rock.transform.localPosition = Rock.transform.localPosition + vector;
			yield return null;
		}
		if (Target.isVisible)
		{
			CameraManager.instance.ShakeCameraForDuration(0.2f, 0.5f, 0.3f);
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, circleCollider2D.radius);
		for (int i = 0; i < array.Length; i++)
		{
			Health component = array[i].gameObject.GetComponent<Health>();
			if (component != null)
			{
				component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.8f));
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
