using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class PlayerSpirit : BaseMonoBehaviour
{
	private float vx;

	private float vy;

	private float Speed;

	private float FacingAngle;

	private float TargetAngle;

	public float MaxSpeed = 5f;

	public float Acceleration = 0.5f;

	public float TurnSpeed = 7f;

	public GameObject CameraBone;

	private Rigidbody2D rb;

	private float DodgeDelay;

	private StateMachine state;

	public ParticleSystem Particles;

	public SkeletonAnimation Spine;

	public static PlayerSpirit Instance;

	private bool _PlayingParticles;

	private bool Begin;

	private float CollisionDelay;

	private bool PlayingParticles
	{
		get
		{
			return _PlayingParticles;
		}
		set
		{
			_PlayingParticles = value;
			if (_PlayingParticles)
			{
				Particles.Play();
			}
			else
			{
				Particles.Stop();
			}
		}
	}

	private void Start()
	{
		rb = base.gameObject.GetComponent<Rigidbody2D>();
		state = GetComponent<StateMachine>();
		PlayingParticles = false;
		GameObject.FindWithTag("Player Camera Bone");
		GameManager.GetInstance().CameraSetZoom(2f);
		GameManager.GetInstance().AddToCamera(CameraBone);
		GameManager.GetInstance().CameraSetTargetZoom(3f);
		Camera.main.GetComponent<CameraFollowTarget>().DisablePlayerLook = true;
		Begin = false;
		StartCoroutine(SpawnIn());
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private IEnumerator SpawnIn()
	{
		Spine.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		Spine.gameObject.SetActive(true);
		state.CURRENT_STATE = StateMachine.State.InActive;
		Spine.AnimationState.SetAnimation(0, "spawn-in", false);
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		yield return new WaitForSeconds(2.3f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().CameraSetTargetZoom(8f);
	}

	private void Update()
	{
		if (state.CURRENT_STATE == StateMachine.State.InActive)
		{
			return;
		}
		CollisionDelay -= Time.deltaTime;
		if (CollisionDelay < 0f)
		{
			DodgeDelay -= Time.deltaTime;
		}
		if (state.CURRENT_STATE == StateMachine.State.InActive)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		if (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) <= 0.3f && Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) <= 0.3f)
		{
			if (CollisionDelay < 0f)
			{
				Speed += (0f - Speed) / 7f * GameManager.DeltaTime;
			}
		}
		else
		{
			if (Speed < MaxSpeed)
			{
				Speed += Acceleration * GameManager.DeltaTime;
			}
			if (CollisionDelay < 0f)
			{
				TargetAngle = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()));
				FacingAngle = Utils.SmoothAngle(FacingAngle, TargetAngle, TurnSpeed);
			}
			if (!Particles.isPlaying)
			{
				Particles.Play();
			}
		}
		if (Speed > 2f)
		{
			if (!PlayingParticles)
			{
				PlayingParticles = true;
			}
		}
		else if (PlayingParticles)
		{
			PlayingParticles = false;
		}
		vx = Speed * Mathf.Cos(FacingAngle * ((float)Math.PI / 180f));
		vy = Speed * Mathf.Sin(FacingAngle * ((float)Math.PI / 180f));
	}

	private void FixedUpdate()
	{
		rb.MovePosition(rb.position + new Vector2(vx, vy) * Time.fixedDeltaTime);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			TargetAngle = (FacingAngle = Utils.GetAngle(collision.contacts[0].point, base.transform.position));
			Speed = MaxSpeed * 1f;
			CollisionDelay = 0.1f;
		}
	}
}
