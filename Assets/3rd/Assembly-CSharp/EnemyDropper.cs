using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using Spine;
using Spine.Unity;
using UnityEngine;

public class EnemyDropper : MonoBehaviour
{
	public enum DropperState
	{
		none,
		asleep,
		onFloor,
		lifting,
		pauseBeforeScan,
		scanning,
		locked,
		dropping,
		roomComplete
	}

	public GameObject sprite;

	public Transform spriteRaisedHeight;

	public Transform spriteLoweredHeight;

	public Transform targetReticle;

	public Transform damageCollider;

	private DropperState dropperState;

	public Bouncer bouncer;

	public float distanceToAlert = 5f;

	public float floorTimeBase = 1f;

	public float floorTimeRange = 1f;

	public float liftTime = 0.5f;

	public float pauseTime = 1f;

	public float scanTimeBase = 2f;

	public float scanTimeRange = 2f;

	public float lockTime = 1f;

	public float dropTime = 0.5f;

	public float scanSpeed = 3f;

	public bool quickLiftOnFloorTouch = true;

	private float scanTime;

	private float floorTime;

	public Vector3 dropperShadowMinScale;

	public Vector3 dropperShadowMaxScale;

	public Transform shadowTransform;

	public ParticleSystem AOEParticles;

	public SkeletonAnimation Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string fallingAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string landAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string prepareAnimation;

	private float timePassed;

	private EventInstance loopedSound;

	private bool foundPlayer;

	private bool playedGoUpSfx;

	private bool playedGoDownSfx;

	private void Start()
	{
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		targetReticle.gameObject.SetActive(false);
		damageCollider.gameObject.SetActive(false);
		shadowTransform.localScale = dropperShadowMinScale;
		SetDropperState(DropperState.asleep);
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Combine(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(ChestAppeared));
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		Spine.AnimationState.Event -= SpineEventHandler;
	}

	private void OnEnable()
	{
		Spine.AnimationState.Event += SpineEventHandler;
		StartCoroutine(WaitForPlayerLoop());
	}

	private void SpineEventHandler(TrackEntry track, global::Spine.Event e)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/stomp_head/stomphead_close_teeth", base.gameObject);
	}

	private IEnumerator WaitForPlayerLoop()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		loopedSound = AudioManager.Instance.CreateLoop("event:/enemy/stomp_head/stomphead_loop", base.gameObject, true);
		foundPlayer = true;
	}

	private void FixedUpdate()
	{
		if (!PlayerRelic.TimeFrozen)
		{
			timePassed += Time.deltaTime;
			switch (dropperState)
			{
			case DropperState.asleep:
				UpdateAsleep();
				break;
			case DropperState.onFloor:
				UpdateOnFloor();
				break;
			case DropperState.lifting:
				UpdateLifting();
				break;
			case DropperState.pauseBeforeScan:
				UpdatePauseBeforeScan();
				break;
			case DropperState.scanning:
				UpdateScanning();
				break;
			case DropperState.locked:
				UpdateLocked();
				break;
			case DropperState.dropping:
				UpdateDropping();
				break;
			case DropperState.roomComplete:
				UpdateRoomComplete();
				break;
			}
		}
	}

	private void SetDropperState(DropperState targetState)
	{
		if (targetState == DropperState.scanning)
		{
			AudioManager.Instance.PlayLoop(loopedSound);
		}
		else
		{
			loopedSound.stop(STOP_MODE.ALLOWFADEOUT);
		}
		timePassed = 0f;
		dropperState = targetState;
		bouncer.gameObject.SetActive(targetState == DropperState.onFloor || targetState == DropperState.asleep || targetState == DropperState.none);
	}

	private void UpdateAsleep(bool causeToWake = false)
	{
		if (!(PlayerFarming.Instance == null) && (Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) < distanceToAlert || causeToWake))
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/stomp_head/stomphead_wakeup", base.gameObject);
			SetDropperState(DropperState.none);
			Spine.AnimationState.SetAnimation(0, prepareAnimation, true);
			base.transform.DOShakeScale(1f, 0.5f).OnComplete(WakeUp);
		}
	}

	private void WakeUp()
	{
		ResetScale();
		SetDropperState(DropperState.lifting);
	}

	private void UpdateOnFloor()
	{
		if (timePassed > floorTime)
		{
			SetDropperState(DropperState.lifting);
		}
	}

	private void UpdateLifting()
	{
		if (!playedGoUpSfx)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/stomp_head/stomphead_go_up", base.gameObject);
			playedGoUpSfx = true;
		}
		float t = 1f / liftTime * timePassed;
		sprite.transform.position = Vector3.Lerp(spriteLoweredHeight.transform.position, spriteRaisedHeight.transform.position, t);
		shadowTransform.localScale = Vector3.Lerp(dropperShadowMinScale, dropperShadowMaxScale, t);
		Spine.AnimationState.SetAnimation(0, fallingAnimation, true);
		if (timePassed > liftTime)
		{
			playedGoUpSfx = false;
			SetDropperState(DropperState.pauseBeforeScan);
		}
	}

	private void UpdatePauseBeforeScan()
	{
		if (timePassed > pauseTime)
		{
			scanTime = scanTimeBase + scanTimeRange * UnityEngine.Random.value;
			SetDropperState(DropperState.scanning);
		}
	}

	private void UpdateScanning()
	{
		Vector3 position = PlayerFarming.Instance.transform.position;
		base.transform.position = Vector3.Lerp(base.transform.position, position, Time.deltaTime * scanSpeed);
		if (timePassed > scanTime)
		{
			PlayTargetReticle();
			SetDropperState(DropperState.locked);
		}
	}

	private void PlayTargetReticle()
	{
		targetReticle.gameObject.SetActive(true);
		targetReticle.localScale = Vector3.zero;
		Quaternion rotation = targetReticle.rotation;
		targetReticle.Rotate(0f, 0f, -180f);
		float num = lockTime / 6f;
		targetReticle.DOScale(2.5f, num * 2f).SetDelay(num);
		targetReticle.DORotateQuaternion(rotation, num * 2f).SetDelay(num);
	}

	private void HideTargetReticle()
	{
		targetReticle.DOKill();
		targetReticle.DOScale(0f, dropTime / 4f).OnComplete(EndTargetReticleAnimation);
	}

	private void EndTargetReticleAnimation()
	{
		targetReticle.gameObject.SetActive(false);
	}

	private void UpdateLocked()
	{
		if (!playedGoDownSfx)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/stomp_head/stomphead_go_down", base.gameObject);
			playedGoDownSfx = true;
		}
		if (timePassed > lockTime)
		{
			playedGoDownSfx = false;
			HideTargetReticle();
			SetDropperState(DropperState.dropping);
		}
	}

	private void UpdateDropping()
	{
		float t = 1f / dropTime * timePassed;
		sprite.transform.position = Vector3.Lerp(spriteRaisedHeight.transform.position, spriteLoweredHeight.transform.position, t);
		shadowTransform.localScale = Vector3.Lerp(dropperShadowMaxScale, dropperShadowMinScale, t);
		Spine.AnimationState.SetAnimation(0, fallingAnimation, true);
		if (timePassed >= dropTime)
		{
			AudioManager.Instance.PlayOneShot("event:/boss/frog/land", base.gameObject);
			Spine.AnimationState.SetAnimation(0, landAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			CameraManager.shakeCamera(2f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
			AOEParticles.Play();
			ShowDamageCollider();
			floorTime = floorTimeBase + floorTimeRange * UnityEngine.Random.value;
			SetDropperState(DropperState.onFloor);
		}
	}

	private void ShowDamageCollider()
	{
		damageCollider.gameObject.SetActive(true);
		damageCollider.localScale = Vector3.zero;
		damageCollider.DOScale(1f, 0.2f).OnComplete(RemoveDamageCollider);
	}

	private void RemoveDamageCollider()
	{
		damageCollider.gameObject.SetActive(false);
	}

	private void ChestAppeared()
	{
		SetDropperState(DropperState.roomComplete);
		UnityEngine.Object.Destroy(base.gameObject, 2f);
	}

	private void UpdateRoomComplete()
	{
		sprite.transform.position = Vector3.Lerp(sprite.transform.position, spriteRaisedHeight.transform.position, Time.deltaTime * 3f);
		sprite.transform.localScale = Vector3.Lerp(sprite.transform.localScale, Vector3.zero, Time.deltaTime);
		shadowTransform.localScale = Vector3.Lerp(shadowTransform.localScale, Vector3.zero, Time.deltaTime * 3f);
		HideTargetReticle();
	}

	private void OnDestroy()
	{
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(ChestAppeared));
		AudioManager.Instance.StopLoop(loopedSound);
	}

	public void BounceUnit()
	{
		if (quickLiftOnFloorTouch && dropperState == DropperState.onFloor && timePassed > floorTime / 2f)
		{
			SetDropperState(DropperState.lifting);
		}
		if (dropperState == DropperState.asleep)
		{
			UpdateAsleep(true);
		}
		base.transform.DOShakeScale(0.5f, 0.5f).OnComplete(ResetScale);
	}

	private void ResetScale()
	{
		base.transform.localScale = Vector3.one;
	}
}
