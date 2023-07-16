using System.Collections;
using FMOD.Studio;
using UnityEngine;

public class PlayerAbility : BaseMonoBehaviour
{
	private float TimeScaleDelay;

	private PlayerFarming playerFarming;

	private StateMachine state;

	private Health health;

	private float KeyDown;

	private bool KeyReleased;

	private EventInstance? loopingSoundInstance;

	public string LoopSound = "event:/unlock_building/unlock_hold";

	private Coroutine cHealRoutine;

	private Coroutine cZoom;

	private void Start()
	{
		playerFarming = GetComponent<PlayerFarming>();
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		GameManager.GetInstance().CameraResetTargetZoom();
		if (cHealRoutine != null)
		{
			StopCoroutine(cHealRoutine);
		}
		if (cZoom != null)
		{
			StopCoroutine(cZoom);
		}
		if (loopingSoundInstance.HasValue)
		{
			AudioManager.Instance.StopLoop(loopingSoundInstance.Value);
			loopingSoundInstance = null;
		}
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		GameManager.GetInstance().CameraResetTargetZoom();
		if (cHealRoutine != null)
		{
			StopCoroutine(cHealRoutine);
		}
		if (cZoom != null)
		{
			StopCoroutine(cZoom);
		}
		if (loopingSoundInstance.HasValue)
		{
			AudioManager.Instance.StopLoop(loopingSoundInstance.Value);
			loopingSoundInstance = null;
		}
	}

	private void Update()
	{
		if (Time.timeScale <= 0f || playerFarming.GoToAndStopping)
		{
			return;
		}
		bool fleeceAbilityButtonHeld = InputManager.Gameplay.GetFleeceAbilityButtonHeld();
		if (fleeceAbilityButtonHeld)
		{
			KeyReleased = false;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		case StateMachine.State.Moving:
			if (fleeceAbilityButtonHeld && !KeyReleased && (KeyDown += Time.deltaTime) > 0.2f && PlayerFleeceManager.BleatToHeal() && CanHeal())
			{
				cHealRoutine = StartCoroutine(DoHeal());
			}
			break;
		case StateMachine.State.Heal:
			if (!fleeceAbilityButtonHeld)
			{
				StopCoroutine(cHealRoutine);
				StartCoroutine(EndHeal());
			}
			break;
		}
		if (!fleeceAbilityButtonHeld)
		{
			KeyDown = 0f;
			KeyReleased = true;
		}
	}

	private IEnumerator DoZoom()
	{
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = true;
		float Progress = 0f;
		float Duration = 1f;
		float StartZoom = GameManager.GetInstance().CamFollowTarget.targetDistance;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				GameManager.GetInstance().CameraSetTargetZoom(Mathf.Lerp(StartZoom, 8f, Mathf.SmoothStep(0f, 1f, Progress / Duration)));
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator DoHeal()
	{
		if (!loopingSoundInstance.HasValue)
		{
			loopingSoundInstance = AudioManager.Instance.CreateLoop(LoopSound, true);
		}
		KeyReleased = false;
		state.CURRENT_STATE = StateMachine.State.Heal;
		cZoom = StartCoroutine(DoZoom());
		yield return new WaitForSeconds(1f);
		if (!CanHeal())
		{
			StartCoroutine(EndHeal());
			yield break;
		}
		while (CanHeal())
		{
			FaithAmmo.UseAmmo(PlayerSpells.AmmoCost);
			int num = 1;
			num *= TrinketManager.GetHealthAmountMultiplier();
			num = Mathf.RoundToInt(num);
			health.Heal(num);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			playerFarming.growAndFade.Play();
			BiomeConstants.Instance.EmitHeartPickUpVFX(playerFarming.CameraBone.transform.position, 0f, "red", "burst_big");
			BiomeConstants.Instance.EmitBloodImpact(base.transform.position, 0f, "red", "BloodImpact_Large_0");
			AudioManager.Instance.PlayOneShot("event:/player/collect_blue_heart", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject);
			yield return new WaitForSeconds(1f);
		}
		StartCoroutine(EndHeal());
	}

	private bool CanHeal()
	{
		if (FaithAmmo.CanAfford(PlayerSpells.AmmoCost))
		{
			return health.HP + health.SpiritHearts < health.totalHP + health.TotalSpiritHearts;
		}
		return false;
	}

	private IEnumerator EndHeal()
	{
		if (loopingSoundInstance.HasValue)
		{
			AudioManager.Instance.StopLoop(loopingSoundInstance.Value);
			loopingSoundInstance = null;
		}
		if (cZoom != null)
		{
			StopCoroutine(cZoom);
		}
		yield return new WaitForSeconds(0.1f);
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
