using System;
using FMODUnity;
using MMTools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class AnimateOnCollision : MonoBehaviour
{
	private bool Activated;

	public float ActivatedTimer = 3f;

	private float Progress;

	public SkeletonAnimation Spine;

	private string DefaultAnimation;

	private UnitObject player;

	public float PushForceMultiplier = 1f;

	private Collider2D PlayerCollision;

	[EventRef]
	public string VOtoPlay = "event:/enemy/vocals/humanoid/warning";

	private GameObject p;

	public float ActivateDistance = 0.666f;

	public bool UseCollider;

	public bool OnlyIfPlayerAttacked;

	public bool UsePlayerPrisoner = true;

	[SerializeField]
	[SpineAnimation("", "", true, false)]
	private string AnimationToChangTo = "jeer";

	[SerializeField]
	[SpineAnimation("", "", true, false)]
	private string AnimationToChangToIfUp = "jeer-up";

	public UnityEvent Callback;

	public float Distance;

	private bool foundPlayer;

	public void PushPlayer()
	{
		AudioManager.Instance.PlayOneShot(VOtoPlay, base.gameObject);
		if (UsePlayerPrisoner)
		{
			MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		}
		if (Spine != null)
		{
			if (DefaultAnimation == "idle-up")
			{
				Spine.AnimationState.SetAnimation(0, AnimationToChangToIfUp, false);
			}
			else
			{
				Spine.AnimationState.SetAnimation(0, AnimationToChangTo, false);
			}
			if (!string.IsNullOrEmpty(DefaultAnimation))
			{
				Spine.AnimationState.AddAnimation(0, DefaultAnimation, true, 0f);
			}
			if ((bool)player)
			{
				float angle = Utils.GetAngle(base.gameObject.transform.position, player.gameObject.transform.position) * ((float)Math.PI / 180f);
				player.DoKnockBack(angle, 0.1f, 0.33f);
			}
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}

	private void Start()
	{
		if (Spine != null)
		{
			DefaultAnimation = Spine.state.GetCurrent(0).ToString();
		}
		FindPlayer();
	}

	private void FindPlayer()
	{
		if (!(p == null))
		{
			return;
		}
		if (UsePlayerPrisoner)
		{
			if (PlayerPrisonerController.Instance != null)
			{
				p = PlayerPrisonerController.Instance.gameObject;
				player = p.GetComponent<UnitObject>();
				foundPlayer = true;
			}
		}
		else if (PlayerFarming.Instance != null)
		{
			p = PlayerFarming.Instance.gameObject;
			player = p.GetComponent<UnitObject>();
			foundPlayer = true;
		}
	}

	private void Update()
	{
		if (player == null)
		{
			if (!foundPlayer)
			{
				FindPlayer();
			}
		}
		else
		{
			if (MMConversation.isPlaying)
			{
				return;
			}
			Distance = Vector3.Distance(base.gameObject.transform.position, player.gameObject.transform.position);
			if (Vector3.Distance(base.gameObject.transform.position, player.gameObject.transform.position) < ActivateDistance && !Activated && !UseCollider)
			{
				Progress = 0f;
				Activated = true;
				PushPlayer();
			}
			if (Activated)
			{
				if (Progress < ActivatedTimer)
				{
					Progress += Time.deltaTime;
				}
				else
				{
					Activated = false;
				}
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (UseCollider && collision.gameObject.tag == "Player" && base.enabled)
		{
			Progress = 0f;
			Activated = true;
			PushPlayer();
		}
	}
}
