using System;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class CritterBee : BaseMonoBehaviour
{
	private enum State
	{
		FlyingIn,
		Idle,
		FlyingOut
	}

	public static List<CritterBee> Bees = new List<CritterBee>();

	public bool IsFireFly;

	public static List<CritterBee> FireFlys = new List<CritterBee>();

	public bool IsButterFly;

	public static List<CritterBee> ButterFlys = new List<CritterBee>();

	private Vector3? StartingPosition;

	private Vector3 TargetPosition;

	public float MaximumRange = 5f;

	public float Speed = 0.03f;

	public float turningSpeed = 1f;

	public float angleNoiseAmplitude;

	public float angleNoiseFrequency;

	public float timestamp;

	public int RanDirection;

	private float Angle;

	public float DirectionChangeDelay;

	private Vector3 NewPosition;

	public bool AvoidPlayer;

	private Vector3 AvoidPlayerDirection = Vector3.zero;

	public float AvoidPlayerDistance = 1f;

	public float AvoidPlayerSafetyDistance = 5f;

	public float AvoidPlayerAccelerationSpeed = 10f;

	public float AvoidPlayerCooldownSpeed = 10f;

	public float AvoidSpeed = 5f;

	public bool AvoidWall;

	public LayerMask LayersToCheck;

	public float RayDistance;

	private Vector3 AvoidWallDirection = Vector3.zero;

	public float AvoidWallSpeed = 5f;

	public SpriteRenderer spriteRenderer;

	public float BaseHeight = 1f;

	public float WobbleHeight = 0.5f;

	public float VerticalWobbleSpeed = 5f;

	public float VerticalNoiseFrequency = 1f;

	public float VerticalNoiseAmplitude = 1f;

	private float VerticalWobble;

	private Vector3 NewHeightPosition;

	public bool StopAndAnimate;

	public Vector2 TimeBetweenAnimations = new Vector2(3f, 4f);

	public Vector2 AnimationDuration = new Vector2(2f, 3f);

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AnimationName;

	public float StopAndAnimateTimer;

	public bool StartRandomPositionWithinMaxRange;

	public bool HideAtnight = true;

	public bool HideAtday;

	public bool CanMove;

	private State CurrentState;

	private float FlyInDuration;

	public float FlyInSpeed = 0.5f;

	public float FlyOutSpeed = 1f;

	public Vector3 FlyOutPosition = new Vector3(0f, 0.5f, 0.5f);

	private float RandomOffset;

	private float changeDirectionTimestamp;

	private Vector3 PrevPosition;

	public bool OverrideAnimation;

	public Sprite Frame1;

	public Sprite Frame2;

	public List<Sprite> OverrideFrame1;

	public List<Sprite> OverrideFrame2;

	public int OverrideIndex;

	private void OnEnable()
	{
		if (IsFireFly)
		{
			FireFlys.Add(this);
		}
		else if (base.name.IndexOf("Butter") != -1)
		{
			IsButterFly = true;
			ButterFlys.Add(this);
		}
		else
		{
			Bees.Add(this);
		}
	}

	private void OnDisable()
	{
		if (IsFireFly)
		{
			FireFlys.Remove(this);
		}
		else if (IsButterFly)
		{
			ButterFlys.Remove(this);
		}
		else
		{
			Bees.Remove(this);
		}
	}

	private void Start()
	{
		Setup(base.transform.position);
	}

	public void Setup(Vector3 _TargetPosition)
	{
		CanMove = false;
		base.transform.position = _TargetPosition;
		TargetPosition = _TargetPosition;
		Debug.Log("Setup critters");
		if (StartRandomPositionWithinMaxRange)
		{
			base.transform.position = base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * MaximumRange;
		}
		if (GameManager.GetInstance() != null)
		{
			timestamp = GameManager.GetInstance().CurrentTime;
		}
		else
		{
			timestamp = Time.time;
		}
		turningSpeed += UnityEngine.Random.Range(-0.1f, 0.1f);
		angleNoiseFrequency += UnityEngine.Random.Range(-0.1f, 0.1f);
		angleNoiseAmplitude += UnityEngine.Random.Range(-0.1f, 0.1f);
		RanDirection = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		VerticalWobble = UnityEngine.Random.Range(0, 360);
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		if (OverrideAnimation)
		{
			OverrideIndex = UnityEngine.Random.Range(0, OverrideFrame1.Count);
		}
		CanMove = true;
	}

	private void OnNewPhaseStarted()
	{
		if ((!HideAtnight && !HideAtday) || !CanMove)
		{
			return;
		}
		if (HideAtnight)
		{
			if (TimeManager.CurrentPhase == DayPhase.Night)
			{
				FlyAway();
			}
			else if (CurrentState == State.FlyingOut)
			{
				FlyIn();
			}
		}
		if (HideAtday)
		{
			if (TimeManager.CurrentPhase != DayPhase.Night)
			{
				FlyAway();
			}
			else if (CurrentState == State.FlyingOut)
			{
				FlyIn();
			}
		}
	}

	private void FlyIn()
	{
		FlyInDuration = 0f;
		RandomOffset = UnityEngine.Random.Range(0f, 3f);
		CurrentState = State.FlyingIn;
	}

	private void FlyAway()
	{
		FlyInDuration = 0f;
		RandomOffset = UnityEngine.Random.Range(0f, 2f);
		CurrentState = State.FlyingOut;
	}

	private void Update()
	{
		if (!CanMove)
		{
			return;
		}
		float num = turningSpeed;
		Angle = Mathf.LerpAngle(Angle, Utils.GetAngle(base.transform.position, TargetPosition), Time.deltaTime * num);
		if (GameManager.GetInstance() != null && angleNoiseAmplitude > 0f && angleNoiseFrequency > 0f && Vector3.Distance(TargetPosition, base.transform.position) < MaximumRange)
		{
			Angle += (-0.5f + Mathf.PerlinNoise(GameManager.GetInstance().TimeSince(timestamp) * angleNoiseFrequency, 0f)) * angleNoiseAmplitude * (float)RanDirection;
		}
		else if (Vector3.Distance(TargetPosition, base.transform.position) >= MaximumRange)
		{
			Angle = Utils.GetAngle(base.transform.position, TargetPosition);
		}
		NewPosition = new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f))) * Time.deltaTime;
		if (AvoidPlayer && PlayerFarming.Instance != null)
		{
			float num2 = Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position);
			if (num2 < AvoidPlayerDistance)
			{
				Vector3 value = base.transform.position - PlayerFarming.Instance.transform.position;
				value.z = 0f;
				AvoidPlayerDirection = Vector3.Normalize(value) * AvoidSpeed * Time.deltaTime;
			}
			else if (num2 > AvoidPlayerSafetyDistance)
			{
				AvoidPlayerDirection = Vector3.Lerp(AvoidPlayerDirection, Vector3.zero, AvoidPlayerCooldownSpeed * Time.deltaTime);
			}
		}
		if (AvoidWall && (bool)Physics2D.Raycast(base.transform.position, (base.transform.position - NewPosition).normalized, RayDistance, LayersToCheck))
		{
			Vector3 value2 = Utils.DegreeToVector2(Mathf.Repeat(Utils.GetAngle(base.transform.position, NewPosition) + UnityEngine.Random.Range(-35f, 35f), 360f));
			value2.z = 0f;
			AvoidWallDirection = Vector3.Normalize(value2) * AvoidWallSpeed * Time.deltaTime;
		}
		PrevPosition = base.transform.position;
		if (Time.timeScale != 0f)
		{
			base.transform.position = base.transform.position + NewPosition + AvoidPlayerDirection + AvoidWallDirection;
		}
		Angle = Mathf.Repeat(Utils.GetAngle(PrevPosition, base.transform.position), 360f);
		if (Angle != 0f && Time.time > changeDirectionTimestamp)
		{
			base.transform.localScale = new Vector3((Angle > 90f && Angle < 270f) ? 1 : (-1), 1f, 1f);
			changeDirectionTimestamp = Time.time + DirectionChangeDelay;
		}
		switch (CurrentState)
		{
		case State.FlyingIn:
			if (FlyInDuration < 1f && (RandomOffset -= Time.deltaTime) < 0f)
			{
				NewHeightPosition = Vector3.Lerp(FlyOutPosition, new Vector3(0f, 0.5f, BaseHeight + WobbleHeight * Mathf.Cos(VerticalWobble)), Mathf.SmoothStep(0f, 1f, FlyInDuration += Time.deltaTime * FlyInSpeed));
			}
			if (FlyInDuration >= 1f)
			{
				CurrentState = State.Idle;
			}
			break;
		case State.FlyingOut:
			if (FlyInDuration < 1f && (RandomOffset -= Time.deltaTime) < 0f)
			{
				NewHeightPosition = Vector3.Lerp(new Vector3(0f, 0.5f, BaseHeight + WobbleHeight * Mathf.Cos(VerticalWobble)), FlyOutPosition, Mathf.SmoothStep(0f, 1f, FlyInDuration += Time.deltaTime * FlyOutSpeed));
			}
			if (FlyInDuration >= 1f)
			{
				base.gameObject.Recycle();
			}
			break;
		case State.Idle:
			NewHeightPosition = new Vector3(0f, 0.5f, BaseHeight + WobbleHeight * Mathf.Cos(VerticalWobble += Time.deltaTime * VerticalWobbleSpeed));
			NewHeightPosition.z += (-0.5f + Mathf.PerlinNoise(GameManager.GetInstance().TimeSince(timestamp) * VerticalNoiseFrequency, 0f)) * VerticalNoiseAmplitude;
			break;
		}
		if (spriteRenderer != null)
		{
			spriteRenderer.transform.localPosition = NewHeightPosition;
		}
	}

	public void SetSpeedWithAcceleration(float speed)
	{
		base.enabled = true;
		CanMove = true;
		DOTween.To(() => Speed, delegate(float x)
		{
			Speed = x;
		}, speed, 1.5f);
	}

	public void SetAvoidPlayer(bool avoidPlayer)
	{
		AvoidPlayer = avoidPlayer;
	}

	private void LateUpdate()
	{
		if (!(spriteRenderer == null))
		{
			if (spriteRenderer.sprite == Frame1)
			{
				spriteRenderer.sprite = OverrideFrame1[OverrideIndex];
			}
			else if (spriteRenderer.sprite == Frame2)
			{
				spriteRenderer.sprite = OverrideFrame2[OverrideIndex];
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			Utils.DrawCircleXY(base.transform.position, MaximumRange, Color.yellow);
		}
		else
		{
			Utils.DrawCircleXY(TargetPosition, MaximumRange, Color.yellow);
		}
	}
}
