using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class TrapGrabber : BaseMonoBehaviour
{
	private enum State
	{
		Idle,
		Warning,
		Grabbing,
		Grabbed,
		Hit,
		Die
	}

	public GameObject Teeth;

	public Transform SortingGroup;

	private Transform CachePlayerParent;

	private int HP = 6;

	private State _CurrentState;

	private BoxCollider2D boxCollider2D;

	private List<Collider2D> colliders;

	private ContactFilter2D contactFilter2D;

	public SkeletonAnimation Spine;

	private PlayerFarming playerFarming;

	private float Angle;

	private float Speed;

	private Vector3 Position;

	private Coroutine ReturnToCenter;

	private Health EnemyHealth;

	private State CurrentState
	{
		get
		{
			return _CurrentState;
		}
		set
		{
			if (_CurrentState != value)
			{
				switch (_CurrentState)
				{
				case State.Idle:
					ReturnToCenter = StartCoroutine(DoReturnToCenter());
					Spine.AnimationState.SetAnimation(0, "hidden", true);
					break;
				case State.Grabbing:
					if (ReturnToCenter != null)
					{
						StopCoroutine(DoReturnToCenter());
					}
					Spine.AnimationState.SetAnimation(0, "grab", false);
					Spine.AnimationState.AddAnimation(0, "grabbed", true, 0f);
					playerFarming.simpleSpineAnimator.Animate("grabber-grab", 0, false);
					playerFarming.simpleSpineAnimator.AddAnimate("grabber-grabbed", 0, false, 0f);
					break;
				case State.Hit:
					Spine.AnimationState.SetAnimation(0, "hit", false);
					Spine.AnimationState.AddAnimation(0, "grabbed", true, 0f);
					playerFarming.simpleSpineAnimator.Animate("grabber-hit", 0, false);
					playerFarming.simpleSpineAnimator.AddAnimate("grabber-grabbed", 0, false, 0f);
					break;
				case State.Die:
					Spine.AnimationState.SetAnimation(0, "kill", false);
					playerFarming.simpleSpineAnimator.Animate("grabber-kill", 0, false);
					break;
				}
			}
			_CurrentState = value;
		}
	}

	private void Start()
	{
		boxCollider2D = GetComponent<BoxCollider2D>();
		contactFilter2D = default(ContactFilter2D);
		contactFilter2D.NoFilter();
		CurrentState = State.Idle;
	}

	private IEnumerator DoReturnToCenter()
	{
		yield return new WaitForSeconds(1f);
		float Distance = Vector3.Distance(Teeth.transform.localPosition, Vector3.zero);
		float Speed = 5f;
		Vector3 Start = Teeth.transform.localPosition;
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < Distance / Speed)
			{
				Teeth.transform.localPosition = Vector3.Lerp(Start, Vector3.zero, Timer / (Distance / Speed));
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator DoAttack()
	{
		CurrentState = State.Warning;
		GameObject player = GameObject.FindWithTag("Player");
		Speed = 0f;
		while (Vector3.Distance(Teeth.transform.position, player.transform.position) > Speed * Time.deltaTime)
		{
			if (Speed < 5f)
			{
				Speed += 1f;
			}
			Angle = Utils.GetAngle(Teeth.transform.position, player.transform.position) * ((float)Math.PI / 180f);
			Position = new Vector3(Speed * Time.deltaTime * Mathf.Cos(Angle), Speed * Time.deltaTime * Mathf.Sin(Angle));
			Teeth.transform.position = Teeth.transform.position + Position;
			if (Vector3.Distance(base.transform.position, player.transform.position) > 4f)
			{
				CurrentState = State.Idle;
				yield break;
			}
			yield return null;
		}
		EnemyHealth = player.GetComponent<Health>();
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, EnemyHealth.transform.position));
		if (EnemyHealth.isPlayer)
		{
			GameManager.GetInstance().HitStop();
		}
		Spine.transform.position = EnemyHealth.transform.position;
		CachePlayerParent = EnemyHealth.transform.parent;
		EnemyHealth.transform.parent = SortingGroup;
		playerFarming = EnemyHealth.GetComponent<PlayerFarming>();
		playerFarming.state.CURRENT_STATE = StateMachine.State.Grabbed;
		StartCoroutine(DoGrabbing());
		StartCoroutine(DamagePlayer(EnemyHealth));
		Teeth.SetActive(false);
	}

	private IEnumerator DamagePlayer(Health h)
	{
		while (CurrentState != State.Die)
		{
			float Timer = 0f;
			while (true)
			{
				float num;
				Timer = (num = Timer + Time.deltaTime);
				if (!(num < 1f))
				{
					break;
				}
				yield return null;
			}
			if (CurrentState != State.Die)
			{
				h.DealDamage(1f, Spine.gameObject, Spine.transform.position);
			}
			if (h.HP <= 0f)
			{
				Clear();
			}
			yield return null;
		}
	}

	private IEnumerator DoGrabbing()
	{
		CurrentState = State.Grabbing;
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			playerFarming.transform.position = Vector3.Lerp(playerFarming.transform.position, Spine.transform.position, Timer / 0.5f);
			yield return null;
		}
		StartCoroutine(DoGrabbed());
	}

	private IEnumerator DoGrabbed()
	{
		CurrentState = State.Grabbed;
		playerFarming.transform.position = Spine.transform.position;
		while (!InputManager.Gameplay.GetAttackButtonDown())
		{
			yield return null;
		}
		if (--HP > 0)
		{
			StartCoroutine(DoHit());
		}
		else
		{
			StartCoroutine(DoKill());
		}
	}

	private IEnumerator DoHit()
	{
		CurrentState = State.Hit;
		FlashRed();
		CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.2f))
			{
				break;
			}
			yield return null;
		}
		StartCoroutine(DoGrabbed());
	}

	private IEnumerator DoKill()
	{
		CurrentState = State.Die;
		GameManager.GetInstance().HitStop();
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 1f))
			{
				break;
			}
			if (Timer < 0.3f)
			{
				CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
			}
			yield return null;
		}
		playerFarming.state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		EnemyHealth = collision.gameObject.GetComponent<Health>();
		if (CurrentState == State.Idle && EnemyHealth != null && EnemyHealth.isPlayer)
		{
			StartCoroutine(DoAttack());
		}
	}

	public void FlashRed()
	{
		StopCoroutine(DoFlashRed());
		StartCoroutine(DoFlashRed());
	}

	private IEnumerator DoFlashRed()
	{
		MeshRenderer meshRenderer = Spine.gameObject.GetComponent<MeshRenderer>();
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(block);
		int fillAlpha = Shader.PropertyToID("_FillAlpha");
		int nameID = Shader.PropertyToID("_FillColor");
		block.SetFloat(fillAlpha, 1f);
		block.SetColor(nameID, Color.red);
		meshRenderer.SetPropertyBlock(block);
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + 0.05f);
			if (num <= 1f)
			{
				block.SetFloat(fillAlpha, 1f - Progress);
				meshRenderer.SetPropertyBlock(block);
				yield return null;
				continue;
			}
			break;
		}
	}

	private void OnDestroy()
	{
		Clear();
	}

	private void Clear()
	{
		StopAllCoroutines();
		CurrentState = State.Idle;
		if (playerFarming != null)
		{
			playerFarming.transform.parent = CachePlayerParent;
			playerFarming.HealingParticles.Stop();
		}
	}
}
