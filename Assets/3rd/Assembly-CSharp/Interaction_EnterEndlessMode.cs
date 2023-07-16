using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using MMTools;
using src.Extensions;
using UnityEngine;

public class Interaction_EnterEndlessMode : Interaction
{
	private string sLabel;

	[SerializeField]
	private List<GameObject> _objectsToEnable = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _objectsToDisable = new List<GameObject>();

	[SerializeField]
	private GameObject _portalVFX;

	[SerializeField]
	private GameObject _portalVFX_Recharge;

	[SerializeField]
	private GameObject _portalVFXLighting;

	[SerializeField]
	private GameObject crownStatue;

	[SerializeField]
	private GameObject cameraInclude;

	private Tween currentTween;

	private bool wasRecharging;

	public GameObject distortionObject;

	private void Start()
	{
		UpdateLocalisation();
	}

	protected override void OnEnable()
	{
		_portalVFX_Recharge.SetActive(false);
		_portalVFX.SetActive(false);
		base.OnEnable();
		distortionObject.gameObject.SetActive(false);
		CheckState();
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(CheckStateDelayed));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(CheckStateDelayed));
	}

	private void CheckStateDelayed()
	{
		StartCoroutine(CheckStateDelayedRoutine());
	}

	private IEnumerator CheckStateDelayedRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		CheckState(true);
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		Interactable = !DataManager.Instance.EndlessModeOnCooldown;
	}

	private void CheckState(bool fromDayChange = false)
	{
		if (!DataManager.Instance.OnboardedEndlessMode)
		{
			return;
		}
		foreach (GameObject item in _objectsToEnable)
		{
			item.SetActive(true);
		}
		foreach (GameObject item2 in _objectsToDisable)
		{
			item2.SetActive(false);
		}
		if (DataManager.Instance.EndlessModeOnCooldown)
		{
			wasRecharging = true;
			_portalVFX_Recharge.SetActive(true);
			_portalVFX.SetActive(false);
			return;
		}
		if (wasRecharging && fromDayChange)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", base.gameObject);
		}
		wasRecharging = false;
		_portalVFX_Recharge.SetActive(false);
		_portalVFX.SetActive(true);
		base.HasChanged = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.EnterPurgatory;
	}

	public override void GetLabel()
	{
		if (string.IsNullOrEmpty(sLabel))
		{
			UpdateLocalisation();
		}
		if (DataManager.Instance.EndlessModeOnCooldown)
		{
			base.Label = ScriptLocalization.Interactions.Recharging;
		}
		else
		{
			base.Label = sLabel;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!DataManager.Instance.EndlessModeOnCooldown)
		{
			Interactable = false;
			StartCoroutine(EnterEndlessModeIE());
		}
	}

	public void PulseDisplacementObject()
	{
		if (distortionObject.gameObject.activeSelf)
		{
			distortionObject.transform.localScale = Vector3.zero;
			distortionObject.transform.DORestart();
			distortionObject.transform.DOPlay();
			return;
		}
		distortionObject.SetActive(true);
		distortionObject.transform.localScale = Vector3.zero;
		distortionObject.transform.DOScale(9f, 0.9f).SetEase(Ease.Linear).OnComplete(delegate
		{
			distortionObject.SetActive(false);
		});
	}

	private IEnumerator EnterEndlessModeIE()
	{
		_portalVFXLighting.gameObject.SetActive(true);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "warp-out-down", false);
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		PlayerFarming.Instance.circleCollider2D.enabled = false;
		PlayerFarming.Instance.transform.DOMove(base.transform.position - Vector3.up, 1f);
		currentTween.Kill();
		BiomeConstants.Instance.GoopFadeIn(1f, 1.4f);
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		yield return new WaitForSeconds(1.15f);
		cameraInclude.gameObject.SetActive(true);
		PulseDisplacementObject();
		yield return new WaitForSeconds(1f);
		BiomeConstants.Instance.ChromaticAbberationTween(0.1f, 0.6f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		SimulationManager.Pause();
		bool enteredEndlessMode = false;
		cameraInclude.gameObject.SetActive(false);
		_portalVFXLighting.gameObject.SetActive(false);
		UISandboxMenuController uISandboxMenuController = MonoSingleton<UIManager>.Instance.SandboxMenuTemplate.Instantiate();
		uISandboxMenuController.Show();
		uISandboxMenuController.OnScenarioChosen = (Action<ScenarioData>)Delegate.Combine(uISandboxMenuController.OnScenarioChosen, (Action<ScenarioData>)delegate(ScenarioData scenario)
		{
			DungeonSandboxManager.CurrentScenario = scenario;
			DungeonSandboxManager.CurrentFleece = scenario.FleeceType;
			GameManager.NewRun("", false);
			GameManager.DungeonUseAllLayers = true;
			DataManager.Instance.EndlessModeOnCooldown = true;
			enteredEndlessMode = true;
			UIManager.PlayAudio("event:/ui/heretics_defeated");
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Dungeon Sandbox", 1f, "", delegate
			{
				SaveAndLoad.Save();
			});
		});
		uISandboxMenuController.OnHide = (Action)Delegate.Combine(uISandboxMenuController.OnHide, (Action)delegate
		{
			Interactable = true;
			if (!enteredEndlessMode)
			{
				StartCoroutine(ExitEndlessModeIE());
			}
		});
	}

	public IEnumerator ExitEndlessModeIE()
	{
		Interactable = false;
		_portalVFXLighting.gameObject.SetActive(true);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.state.facingAngle = (PlayerFarming.Instance.state.LookAngle = 0f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "warp-in-up", false);
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		PlayerFarming.Instance.circleCollider2D.enabled = false;
		PlayerFarming.Instance.transform.DOMove(base.transform.position - Vector3.up * 3f, 1f);
		BiomeConstants.Instance.GoopFadeOut(1f);
		currentTween.Kill();
		yield return new WaitForSeconds(0.2f);
		PulseDisplacementObject();
		yield return new WaitForSeconds(2.6f);
		_portalVFXLighting.gameObject.SetActive(false);
		SimulationManager.UnPause();
		PlayerFarming.Instance.circleCollider2D.enabled = true;
		GameManager.GetInstance().OnConversationEnd();
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
	}
}
