using System;
using System.Collections;
using UnityEngine;

public class HUD_WorldToHUDHeart : BaseMonoBehaviour
{
	public RectTransform rectTransform;

	public float MaxSpeed = 50f;

	public float Acceleration = 5f;

	public float AngleModifier = 0.2f;

	public float v1 = 0.7f;

	public float v2 = 0.4f;

	private Action Callback;

	private void OnEnable()
	{
		StartCoroutine(MoveRoutine());
	}

	private IEnumerator MoveRoutine()
	{
		float Delay = 0f;
		rectTransform.localScale = Vector3.zero;
		while (true)
		{
			float num;
			Delay = (num = Delay + Time.deltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * 3f, Delay / 0.5f);
			yield return null;
		}
		float Speed = 0f - MaxSpeed;
		bool Looping = true;
		float Angle = 0f;
		while (Looping)
		{
			Angle = Utils.GetAngle(rectTransform.localPosition, Vector3.zero) * ((float)Math.PI / 180f);
			Speed += Acceleration;
			rectTransform.localPosition += new Vector3(Speed * Mathf.Cos(Angle), Speed * Mathf.Sin(Angle)) * Time.deltaTime;
			if (Vector2.Distance(rectTransform.localPosition, Vector3.zero) > Speed * Time.deltaTime)
			{
				yield return null;
			}
			else
			{
				Looping = false;
			}
		}
		CameraManager.shakeCamera(0.1f, Angle);
		StartCoroutine(ScaleRoutine());
	}

	private IEnumerator ScaleRoutine()
	{
		float ScaleSpeed = 0.1f;
		float Scale = rectTransform.localScale.x;
		bool Looping = true;
		while (Looping)
		{
			rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, Vector3.zero, 15f * Time.deltaTime);
			ScaleSpeed -= 0.02f * GameManager.DeltaTime;
			Scale += ScaleSpeed;
			rectTransform.localScale = Vector3.one * Scale;
			if (Scale < 0.8f)
			{
				CameraManager.shakeCamera(0.2f, UnityEngine.Random.Range(0, 360));
				Looping = false;
			}
			else
			{
				yield return null;
			}
		}
		rectTransform.localPosition = Vector3.zero;
		if (Callback != null)
		{
			Callback();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public static void Create(Vector3 HUDEndPosition, Vector3 WorldStartPosition, Action Callback)
	{
		GameObject gameObject = GameObject.FindWithTag("Canvas");
		HUD_WorldToHUDHeart component = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Hearts/World To Hud Heart"), HUDEndPosition, Quaternion.identity, gameObject.transform) as GameObject).GetComponent<HUD_WorldToHUDHeart>();
		component.Callback = Callback;
		component.rectTransform.position = Camera.main.WorldToScreenPoint(WorldStartPosition);
	}
}
