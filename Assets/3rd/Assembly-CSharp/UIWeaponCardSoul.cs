using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWeaponCardSoul : BaseMonoBehaviour
{
	public delegate void CardComplete(Buttons CurrentButton);

	public static List<UIWeaponCardSoul> uIWeaponCardSouls = new List<UIWeaponCardSoul>();

	public CardComplete OnCardComplete;

	private Canvas canvas;

	private RectTransform rectTransform;

	public float StartSpeed = 1000f;

	public float MaxSpeed = 2000f;

	private float Speed;

	private float Easing = 5f;

	private Vector3 NewPosition;

	private float Angle;

	private Vector3 TargetPosition;

	private Buttons CurrentButtons;

	public RectTransform Trail;

	private Vector3 PreviousPosition;

	public RectTransform TrailTarget;

	private float TrailScale = 1f;

	private float TrailAngle;

	public float ScaleModifier = 6f;

	private Coroutine cMoveRoutine;

	private float Scale = 1f;

	public void Play(Buttons CurrentButtons, Vector3 StartPosition, Vector3 TargetPosition)
	{
		canvas = GetComponentInParent<Canvas>();
		this.CurrentButtons = CurrentButtons;
		this.TargetPosition = TargetPosition;
		rectTransform = GetComponent<RectTransform>();
		rectTransform.position = StartPosition;
		PreviousPosition = rectTransform.position;
		OnPlay();
	}

	private void OnEnable()
	{
		uIWeaponCardSouls.Add(this);
	}

	private void OnDisable()
	{
		uIWeaponCardSouls.Remove(this);
	}

	public void OnPlay()
	{
		rectTransform = GetComponent<RectTransform>();
		Speed = 0f - StartSpeed;
		Angle = (Utils.GetAngle(TargetPosition, rectTransform.position) + (float)UnityEngine.Random.Range(-180, 180)) * ((float)Math.PI / 180f);
		cMoveRoutine = StartCoroutine(MoveRoutine());
	}

	private void Stop()
	{
		if (cMoveRoutine != null)
		{
			StopCoroutine(cMoveRoutine);
		}
	}

	private IEnumerator MoveRoutine()
	{
		float TimeLimit = 0f;
		while (Vector3.Distance(rectTransform.position, TargetPosition) > MaxSpeed * (1f / 60f))
		{
			TrailAngle = Utils.GetAngle(Trail.position, PreviousPosition);
			Trail.eulerAngles = new Vector3(0f, 0f, TrailAngle);
			TrailScale = Vector3.Distance(Trail.position, PreviousPosition) / 40f;
			Trail.localScale = new Vector3(TrailScale * ScaleModifier, 1f, 1f);
			PreviousPosition = rectTransform.position;
			Scale = Speed / MaxSpeed * 0.5f;
			rectTransform.localScale = new Vector3(Scale + 0.5f, Scale + 0.5f, 1f);
			if (Speed < MaxSpeed)
			{
				Speed += 100f * (Time.deltaTime * 60f);
			}
			float num;
			TimeLimit = (num = TimeLimit + Time.deltaTime);
			if (num < 0.7f)
			{
				Angle = Utils.SmoothAngle(Angle, Utils.GetAngle(rectTransform.position, TargetPosition) * ((float)Math.PI / 180f), Easing);
			}
			else
			{
				Angle = Utils.GetAngle(rectTransform.position, TargetPosition) * ((float)Math.PI / 180f);
			}
			NewPosition = new Vector3(Speed * Mathf.Cos(Angle), Speed * Mathf.Sin(Angle)) * Time.deltaTime;
			rectTransform.position += NewPosition;
			yield return null;
		}
		CardComplete onCardComplete = OnCardComplete;
		if (onCardComplete != null)
		{
			onCardComplete(CurrentButtons);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
