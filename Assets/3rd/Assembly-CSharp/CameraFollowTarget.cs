using System;
using System.Collections;
using System.Collections.Generic;
using MMTools;
using UnityEngine;

public class CameraFollowTarget : BaseMonoBehaviour
{
	[Serializable]
	public class Target
	{
		public GameObject gameObject;

		public float Weight = 1f;

		public Target(GameObject gameObject, float Weight)
		{
			this.gameObject = gameObject;
			this.Weight = Weight;
		}
	}

	private const float kIsMovingThreshold = 0.05f;

	public float LookDistance = 0.5f;

	public static CameraFollowTarget Instance;

	public float CamWobbleSettings = 1f;

	public Transform target;

	public List<Target> targets = new List<Target>();

	public Vector3 targetPosition = Vector3.zero;

	public float targetDistance = 10f;

	public float distance = 10f;

	public float MaxZoom = 13f;

	public float MinZoom = 11f;

	public float ZoomLimiter = 5f;

	public float ZoomSpeed = 2f;

	public float ZoomSpeedConversation = 0.0833f;

	private bool init;

	[HideInInspector]
	public float angle = -45f;

	public Vector3 TargetOffset = Vector3.zero;

	public Vector3 CurrentOffset = Vector3.zero;

	public bool SnappyMovement;

	public bool IN_CONVERSATION;

	public Vector3 Look;

	private Vector3 CamWobble;

	private float Wobble;

	private StateMachine PlayerState;

	private float LookSpeed = 5f;

	public bool DisablePlayerLook;

	public float MaxZoomInConversation = 4f;

	private Vector3 CurrentPositionVelocity;

	private float CurrentPositionMaxSpeed = 14f;

	private float SmoothZoom;

	public Bounds bounds;

	public bool UseCameraLimit;

	public Bounds CameraLimitBounds;

	[HideInInspector]
	public Camera TargetCamera;

	private AnimationCurve TargetCameraAnimationCurve;

	private float TargetCameraTransitionDuration;

	private Coroutine cSetTargetCamera;

	private Coroutine cResetTargetCamera;

	private float CamWobbleIntensity;

	public Vector3 CurrentPosition { get; set; }

	public bool IsMoving { get; private set; }

	public Vector3 CurrentTargetCameraPosition { get; set; }

	public void AddTarget(GameObject g, float Weight)
	{
		if (!Contains(g))
		{
			targets.Add(new Target(g, Weight));
		}
	}

	public bool Contains(GameObject gameObject)
	{
		if (targets != null)
		{
			foreach (Target target in targets)
			{
				if (target != null && target.gameObject == gameObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveTarget(GameObject gameObject)
	{
		if (targets == null)
		{
			return;
		}
		foreach (Target target in targets)
		{
			if (target.gameObject == gameObject)
			{
				targets.Remove(target);
				break;
			}
		}
	}

	public void ClearAllTargets()
	{
		targets.Clear();
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		IN_CONVERSATION = false;
		if (SettingsManager.Settings != null)
		{
			CamWobbleSettings = 1 - SettingsManager.Settings.Accessibility.ReduceCameraMotion.ToInt();
		}
	}

	public void SnapTo(Vector3 position)
	{
		Transform obj = base.transform;
		Vector3 position2 = (CurrentPosition = position + new Vector3(0f, distance * Mathf.Sin((float)Math.PI / 180f * angle), (0f - distance) * Mathf.Cos((float)Math.PI / 180f * angle)));
		obj.position = position2;
	}

	public void ForceSnapTo(Vector3 position)
	{
		Transform obj = base.transform;
		Vector3 position2 = (CurrentPosition = position);
		obj.position = position2;
	}

	public void SetOffset(Vector3 Offset)
	{
		TargetOffset = Offset;
	}

	private void LateUpdate()
	{
		if (!init)
		{
			if (targets.Count <= 0)
			{
				GameObject gameObject = GameObject.FindGameObjectWithTag("Player Camera Bone");
				if (gameObject != null)
				{
					AddTarget(gameObject, 1f);
				}
				if (targets.Count <= 0)
				{
					return;
				}
			}
			init = true;
		}
		else if (targets != null && targets.Count > 0)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if (targets[i] == null || targets[i].gameObject == null)
				{
					RemoveTarget(targets[i].gameObject);
				}
			}
			if (!SettingsManager.Settings.Accessibility.ReduceCameraMotion && targets.Count == 1 && !LetterBox.IsPlaying && targets[0] != null && targets[0].gameObject != null && targets[0].gameObject.CompareTag("Player Camera Bone"))
			{
				if (PlayerState != null && !DisablePlayerLook && Time.timeScale > 0f)
				{
					Look = Vector3.Lerp(Look, new Vector3(LookDistance * Mathf.Cos(PlayerState.facingAngle * ((float)Math.PI / 180f)), LookDistance * Mathf.Sin(PlayerState.facingAngle * ((float)Math.PI / 180f))), Time.unscaledDeltaTime * LookSpeed);
				}
				else if (DisablePlayerLook)
				{
					Look = Vector3.Lerp(Look, Vector3.zero, Time.unscaledDeltaTime * LookSpeed);
				}
				else
				{
					PlayerState = targets[0].gameObject.GetComponentInParent<StateMachine>();
				}
			}
			else
			{
				Look = Vector3.zero;
			}
			targetPosition = GetCentrePoint();
			if (MaxZoom <= 0f)
			{
				MaxZoom = 13f;
			}
			if (MinZoom <= 0f)
			{
				MinZoom = 11f;
			}
			if (ZoomLimiter <= 0f)
			{
				ZoomLimiter = 5f;
			}
			if (!MMConversation.isPlaying || SnappyMovement)
			{
				if (targets.Count > 1 && !SnappyMovement)
				{
					distance = Mathf.SmoothDamp(distance, Mathf.Lerp(MinZoom, MaxZoom, Mathf.Max(bounds.size.x, bounds.size.y) / ZoomLimiter), ref SmoothZoom, 1f / 60f * ZoomSpeed, float.PositiveInfinity, SnappyMovement ? Time.unscaledDeltaTime : Time.deltaTime);
				}
				else
				{
					distance = Mathf.SmoothDamp(distance, targetDistance, ref SmoothZoom, 1f / 60f * ZoomSpeed, float.PositiveInfinity, SnappyMovement ? Time.unscaledDeltaTime : Time.deltaTime);
				}
				if (TargetCamera == null)
				{
					CamWobble = Vector3.zero;
				}
				else
				{
					CamWobble += Vector3.forward * 0.0005f * Mathf.Cos(Wobble += Time.unscaledDeltaTime * 2f);
				}
			}
			else
			{
				distance = Mathf.SmoothDamp(distance, targetDistance, ref SmoothZoom, 1f / 60f * ZoomSpeedConversation, MaxZoomInConversation, Time.unscaledDeltaTime);
				CamWobble += Vector3.forward * 0.0005f * Mathf.Cos(Wobble += Time.unscaledDeltaTime * 2f);
			}
			if (MMConversation.isPlaying)
			{
				targetPosition += Vector3.up * 0.5f;
			}
			if (float.IsNaN(distance))
			{
				distance = 10f;
			}
			if (float.IsNaN(SmoothZoom))
			{
				SmoothZoom = 1f;
			}
			Vector3 vector = targetPosition + new Vector3(0f, distance * Mathf.Sin((float)Math.PI / 180f * angle), (0f - distance) * Mathf.Cos((float)Math.PI / 180f * angle)) + CamWobble * CamWobbleSettings + Look + (CurrentOffset = Vector3.Lerp(CurrentOffset, TargetOffset, 5f * Time.unscaledDeltaTime));
			if (SettingsManager.Settings.Accessibility.ReduceCameraMotion && Vector3.Distance(vector, CurrentPosition) < 0.05f)
			{
				vector = CurrentPosition;
			}
			if (!MMConversation.isPlaying || SnappyMovement)
			{
				CurrentPosition = Vector3.Lerp(CurrentPosition, vector, 5f * Time.unscaledDeltaTime);
			}
			else
			{
				CurrentPosition = Vector3.SmoothDamp(CurrentPosition, vector, ref CurrentPositionVelocity, 0.2f, CurrentPositionMaxSpeed, Time.unscaledDeltaTime);
			}
			IsMoving = Vector3.Distance(CurrentPosition, vector) > 0.05f;
		}
		else
		{
			IsMoving = false;
		}
		Vector3 position = ((!(TargetCamera == null) || float.IsNaN(CurrentPosition.x) || float.IsNaN(CurrentPosition.y) || float.IsNaN(CurrentPosition.z)) ? (CurrentTargetCameraPosition + CamWobble * 0.1f * CamWobbleSettings) : CurrentPosition);
		base.transform.position = position;
	}

	public void SetCameraLimits(Bounds Limits)
	{
		UseCameraLimit = true;
		CameraLimitBounds = Limits;
	}

	public void DisableCameraLimits()
	{
		UseCameraLimit = false;
	}

	private Vector3 GetCentrePoint()
	{
		switch (targets.Count)
		{
		case 0:
			return base.transform.position;
		case 1:
			if (targets[0] != null && targets[0].gameObject != null)
			{
				Vector3 result = targets[0].gameObject.transform.position + Vector3.back * 0.5f;
				if (UseCameraLimit)
				{
					if (result.x >= CameraLimitBounds.center.x + CameraLimitBounds.extents.x)
					{
						Look = Vector3.zero;
					}
					result.x = Mathf.Min(CameraLimitBounds.center.x + CameraLimitBounds.extents.x, result.x);
					if (result.x <= CameraLimitBounds.center.x - CameraLimitBounds.extents.x)
					{
						Look = Vector3.zero;
					}
					result.x = Mathf.Max(CameraLimitBounds.center.x - CameraLimitBounds.extents.x, result.x);
					if (result.y >= CameraLimitBounds.center.y + CameraLimitBounds.extents.y)
					{
						Look = Vector3.zero;
					}
					result.y = Mathf.Min(CameraLimitBounds.center.x + CameraLimitBounds.extents.y, result.y);
					if (result.y <= CameraLimitBounds.center.y - CameraLimitBounds.extents.y)
					{
						Look = Vector3.zero;
					}
					result.y = Mathf.Max(CameraLimitBounds.center.x - CameraLimitBounds.extents.y, result.y);
				}
				return result;
			}
			return Vector3.zero;
		default:
			if (targets.Count > 0 && targets[0] != null)
			{
				bounds = new Bounds(targets[0].gameObject.transform.position, Vector3.zero);
				int num = 0;
				float num2 = 0f;
				while (++num < targets.Count)
				{
					if (targets[num] != null)
					{
						bounds.Encapsulate(targets[num].gameObject.transform.position);
						num2 += targets[num].Weight;
					}
				}
			}
			return bounds.center + Vector3.back * 0.5f;
		}
	}

	private void OnDrawGizmos()
	{
		Bounds bound = bounds;
		Gizmos.DrawCube(bounds.center, bounds.size);
	}

	public void SetTargetCamera(Camera camera, float Duration, AnimationCurve animationCurve)
	{
		TargetCamera = camera;
		TargetCameraTransitionDuration = Duration;
		TargetCameraAnimationCurve = animationCurve;
		CamWobbleIntensity = 0f;
		if (cSetTargetCamera != null)
		{
			StopCoroutine(cSetTargetCamera);
		}
		if (cResetTargetCamera != null)
		{
			StopCoroutine(cResetTargetCamera);
		}
		cSetTargetCamera = StartCoroutine(SetTargetCameraRoutine());
	}

	private IEnumerator SetTargetCameraRoutine()
	{
		float Progress = 0f;
		Vector3 StartingPosition = base.transform.position;
		Quaternion StartingAngle = base.transform.rotation;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < TargetCameraTransitionDuration))
			{
				break;
			}
			CurrentTargetCameraPosition = Vector3.Lerp(StartingPosition, TargetCamera.transform.position, TargetCameraAnimationCurve.Evaluate(Progress / TargetCameraTransitionDuration));
			base.transform.rotation = Quaternion.Slerp(StartingAngle, TargetCamera.transform.rotation, TargetCameraAnimationCurve.Evaluate(Progress / TargetCameraTransitionDuration));
			yield return null;
		}
		Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			CamWobbleIntensity = Mathf.Lerp(0f, 1f, Progress / Duration);
			yield return null;
		}
		CamWobbleIntensity = 1f;
	}

	public void ResetTargetCamera(float Duration)
	{
		TargetCameraTransitionDuration = Duration;
		if (cSetTargetCamera != null)
		{
			StopCoroutine(cSetTargetCamera);
		}
		if (cResetTargetCamera != null)
		{
			StopCoroutine(cResetTargetCamera);
		}
		cResetTargetCamera = StartCoroutine(ResetTargetCameraRoutine());
	}

	private IEnumerator ResetTargetCameraRoutine()
	{
		float Progress = 0f;
		Vector3 StartingPosition = base.transform.position;
		Quaternion StartingAngle = base.transform.rotation;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < TargetCameraTransitionDuration))
			{
				break;
			}
			CamWobbleIntensity = Mathf.Lerp(1f, 0f, Progress / 0.1f);
			CurrentTargetCameraPosition = Vector3.Lerp(StartingPosition, CurrentPosition, TargetCameraAnimationCurve.Evaluate(Progress / TargetCameraTransitionDuration));
			base.transform.rotation = Quaternion.Slerp(StartingAngle, Quaternion.Euler(new Vector3(-45f, 0f, 0f)), TargetCameraAnimationCurve.Evaluate(Progress / TargetCameraTransitionDuration));
			yield return null;
		}
		TargetCamera = null;
	}

	private void OnPreRender()
	{
		GL.wireframe = CheatConsole.WIREFRAME_ENABLED;
	}

	private void OnPostRender()
	{
		GL.wireframe = false;
	}
}
