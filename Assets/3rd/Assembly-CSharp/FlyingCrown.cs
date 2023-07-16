using System;
using System.Collections;
using DG.Tweening;
using MMBiomeGeneration;
using Spine;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;

public class FlyingCrown : MonoBehaviour
{
	private bool Active;

	public GameObject StartPosition;

	public GameObject TargetPosition;

	public SkeletonAnimation Spine;

	public MeshRenderer SpineMeshRenderer;

	public Transform ParentTransform;

	public SkeletonAnimation PlayerSpine;

	[SpineAttachment(true, false, false, "", "", "", true, false, dataField = "PlayerSpine")]
	public string CrownAttachment;

	[SpineSlot("", "", false, true, false, dataField = "PlayerSpine")]
	public string CrownAttachmentSlot;

	private Transform FlyingTransform;

	[SpineAttachment(true, false, false, "", "", "", true, false, dataField = "PlayerSpine")]
	public string CrownEyeAttachment;

	[SpineSlot("", "", false, true, false, dataField = "PlayerSpine")]
	public string CrownEyeAttachmentSlot;

	public Vector2 StartingTurnSpeed = new Vector2(10f, 15f);

	public float StartingTurnAcceleration = 20f;

	public float TurnAcceleration = 20f;

	public float StartingSpeed = 10f;

	public float StartingDelay = 0.5f;

	public float Acceleration;

	public float StartingAcceleration = 0.01f;

	public float MaxSpeed;

	public float StartingMaxSpeed = 5f;

	public float ZSpeed = 1f;

	public float StartingZSpeed = 1f;

	public float TargetDistance = 0.025f;

	private float Speed;

	private float Timer;

	private float Delay;

	private float TargetAngle;

	private float Angle;

	private float TurnSpeed;

	private float ZPosition;

	private float ZTime;

	private float StartingZPosition;

	private bool Hiding;

	private void Start()
	{
		base.gameObject.SetActive(false);
		PlayerSpine.AnimationState.Event += HandleAnimationStateEvent;
		BiomeGenerator.OnBiomeChangeRoom += Close;
		PlayerFarming.OnCrownReturn = (Action)Delegate.Combine(PlayerFarming.OnCrownReturn, new Action(Play));
		PlayerFarming.OnCrownReturnSubtle = (Action)Delegate.Combine(PlayerFarming.OnCrownReturnSubtle, new Action(PlayWeapon));
		PlayerFarming.OnHideCrown = (Action)Delegate.Combine(PlayerFarming.OnHideCrown, new Action(Hide));
	}

	private void OnDestroy()
	{
		PlayerSpine.AnimationState.Event -= HandleAnimationStateEvent;
		PlayerFarming.OnCrownReturn = (Action)Delegate.Remove(PlayerFarming.OnCrownReturn, new Action(Play));
		PlayerFarming.OnCrownReturnSubtle = (Action)Delegate.Remove(PlayerFarming.OnCrownReturnSubtle, new Action(PlayWeapon));
		BiomeGenerator.OnBiomeChangeRoom -= Close;
		PlayerFarming.OnHideCrown = (Action)Delegate.Remove(PlayerFarming.OnHideCrown, new Action(Hide));
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		string text = e.Data.Name;
		if (!(text == "CROWN_HIDE_CANCEL"))
		{
			if (text == "CROWN_HIDE")
			{
				PlayWeapon();
			}
		}
		else
		{
			Close();
		}
	}

	public void Play()
	{
		PlayWeapon();
	}

	private void PlayWeapon()
	{
		Delay = 0f;
		Angle = UnityEngine.Random.Range(0, 360);
		Speed = 1f;
		TurnSpeed = UnityEngine.Random.Range(10f, 15f);
		TurnAcceleration = 5f;
		Acceleration = 10f;
		MaxSpeed = 100f;
		ZSpeed = 2f;
		InitPlay();
	}

	private void InitPlay()
	{
		base.transform.DOKill();
		StopAllCoroutines();
		if (!(ParentTransform == null))
		{
			base.transform.parent = ParentTransform.parent;
			base.transform.localScale = Vector3.one;
			base.transform.localRotation = quaternion.identity;
			Active = true;
			base.transform.position = StartPosition.transform.position;
			base.gameObject.SetActive(true);
			Timer = 0f;
			Spine.AnimationState.SetAnimation(0, "return", false);
			SpineMeshRenderer.enabled = true;
			PlayerSpine.Skeleton.SetAttachment("CROWN", null);
			PlayerSpine.Skeleton.SetAttachment("CROWN_EYE", null);
			StartingZPosition = StartPosition.transform.position.z;
			ZTime = 0f;
		}
	}

	private IEnumerator AnimateAndClose()
	{
		Active = false;
		base.transform.parent = TargetPosition.transform;
		Spine.AnimationState.SetAnimation(0, "land", false);
		base.transform.DOLocalMove(Vector3.zero, 0.25f);
		yield return new WaitForSeconds(0.5f);
		Close();
	}

	public void Close()
	{
		if (PlayerSpine != null)
		{
			PlayerSpine.Skeleton.SetAttachment(CrownAttachmentSlot, CrownAttachment);
			PlayerSpine.Skeleton.SetAttachment(CrownEyeAttachmentSlot, CrownEyeAttachment);
		}
		base.gameObject.SetActive(false);
		Active = false;
		Hiding = false;
	}

	public void Hide()
	{
		Debug.Log("HIDE!");
		Hiding = true;
		StopAllCoroutines();
		Active = false;
		PlayerFarming.Instance.StartCoroutine(HideCrown());
		base.gameObject.SetActive(false);
	}

	private IEnumerator HideCrown()
	{
		Debug.Log("routine: HideCrown");
		while (Hiding)
		{
			Debug.Log("Hiding".Colour(Color.yellow));
			if (PlayerSpine != null)
			{
				PlayerSpine.Skeleton.SetAttachment(CrownAttachmentSlot, null);
				PlayerSpine.Skeleton.SetAttachment(CrownEyeAttachmentSlot, null);
			}
			yield return null;
		}
	}

	public void Update()
	{
		if (!Active)
		{
			return;
		}
		if (TargetPosition == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Delay -= Time.deltaTime;
		if (Delay <= 0f)
		{
			TurnSpeed = Lerp(TurnSpeed, 1f, TurnAcceleration * Time.deltaTime);
		}
		TargetAngle = Utils.GetAngle(base.transform.position, TargetPosition.transform.position);
		Angle += Mathf.Atan2(Mathf.Sin((TargetAngle - Angle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - Angle) * ((float)Math.PI / 180f))) * 57.29578f / TurnSpeed;
		if (Speed < MaxSpeed)
		{
			Speed += Acceleration * Time.deltaTime;
		}
		Speed = Mathf.Clamp(Speed, 0f, MaxSpeed);
		ZPosition = Mathf.Lerp(StartingZPosition, TargetPosition.transform.position.z, Mathf.SmoothStep(0f, 1f, ZTime += ZSpeed * Time.deltaTime));
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, ZPosition) + new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f))) * Time.deltaTime;
		PlayerSpine.Skeleton.SetAttachment("CROWN", null);
		PlayerSpine.Skeleton.SetAttachment("CROWN_EYE", null);
		if (Distance(base.transform.position, TargetPosition.transform.position) < TargetDistance)
		{
			StartCoroutine(AnimateAndClose());
		}
	}

	private float Lerp(float firstFloat, float secondFloat, float by)
	{
		return firstFloat + (secondFloat - firstFloat) * by;
	}

	public static float Distance(Vector3 a, Vector3 b)
	{
		return (a - b).sqrMagnitude;
	}
}
