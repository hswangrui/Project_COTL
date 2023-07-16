using System;
using System.Collections;
using UnityEngine;

public class SpiderWebBomb : BaseMonoBehaviour
{
	public float Grav = 0.7f;

	public GameObject Rock;

	public Transform RotateRock;

	public SpriteRenderer Target;

	public CircleCollider2D circleCollider2D;

	private GameObject ToSpawn;

	private float Rotate;

	private Vector3 ShadowPosition;

	private void OnEnable()
	{
		Rock.SetActive(false);
		Target.gameObject.SetActive(false);
		Rotate = UnityEngine.Random.Range(0, 360);
	}

	public void Play(Vector3 Position, GameObject ToSpawn)
	{
		this.ToSpawn = ToSpawn;
		StartCoroutine(MoveRock(Position));
		StartCoroutine(DoShadow());
	}

	private IEnumerator DoShadow()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			Rotate += Time.deltaTime * 0.001f;
			RotateRock.localEulerAngles += new Vector3(0f, 0f, Rotate);
			ShadowPosition = Rock.transform.localPosition;
			ShadowPosition.z = 0f;
			Target.transform.localPosition = ShadowPosition;
			Target.transform.localScale = Vector3.one * Mathf.Clamp(5f + Rock.transform.localPosition.z, 0f, 3f);
			yield return null;
		}
	}

	private IEnumerator MoveRock(Vector3 Position)
	{
		Rock.SetActive(true);
		Target.gameObject.SetActive(true);
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
			CameraManager.shakeCamera(0.2f, UnityEngine.Random.Range(0, 360));
		}
		UnityEngine.Object.Instantiate(ToSpawn, base.transform.position, Quaternion.identity, base.transform.parent);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
