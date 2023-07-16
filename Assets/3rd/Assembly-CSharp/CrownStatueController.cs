using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using MMTools;
using UnityEngine;

public class CrownStatueController : MonoBehaviour
{
	public static CrownStatueController Instance;

	public Interaction EnterEndlessMode;

	public Interaction CrownStatue;

	public GameObject Container;

	public GameObject CameraPosition;

	public GameObject Portal;

	private static readonly int FillAlpha = Shader.PropertyToID("_FillAlpha");

	[SerializeField]
	private GameObject LightingObject;

	[SerializeField]
	private Shader UberShaderRef;

	private EventInstance LoopedSound;

	private void OnEnable()
	{
		Instance = this;
		Container.SetActive(true);
		EnterEndlessMode.enabled = false;
		int num = 0;
		foreach (string killedBoss in DataManager.Instance.KilledBosses)
		{
			if (killedBoss.Contains("_P2"))
			{
				num++;
			}
		}
		if (DataManager.Instance.DeathCatBeaten)
		{
			if (!DataManager.Instance.OnboardedEndlessMode)
			{
				Container.SetActive(true);
				EnterEndlessMode.enabled = false;
			}
			else
			{
				Container.SetActive(false);
				EnterEndlessMode.enabled = true;
				CrownStatue.enabled = false;
			}
		}
		else
		{
			Container.SetActive(true);
			EnterEndlessMode.enabled = false;
		}
	}

	private void OnDisable()
	{
		Instance = null;
	}

	public void EndlessModeOnboarded(Action callback)
	{
		StartCoroutine(EndlessModeOnboardRoutine(callback));
	}

	private IEnumerator EndlessModeOnboardRoutine(Action callback)
	{
		CrownStatue.enabled = false;
		DataManager.Instance.OnboardedEndlessMode = true;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraPosition);
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom");
		LoopedSound = AudioManager.Instance.CreateLoop("event:/material/earthquake", base.gameObject);
		MMVibrate.RumbleContinuous(1f, 5f);
		StartCoroutine(ShakeCameraWithRampUp());
		GameManager.GetInstance().OnConversationNext(CameraPosition, 5f);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.StopLoop(LoopedSound);
		BiomeConstants.Instance.ImpactFrameForDuration();
		MMVibrate.StopRumble();
		SpriteRenderer[] spriteRenderers = base.gameObject.GetComponentsInChildren<SpriteRenderer>();
		Material material = new Material(spriteRenderers[0].material);
		Material oldMat = new Material(spriteRenderers[0].material);
		SpriteRenderer[] array = spriteRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			if (spriteRenderer.material.shader == UberShaderRef)
			{
				spriteRenderer.material = material;
			}
		}
		material.DOColor(StaticColors.OffWhiteColor, 2f);
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
		LightingObject.SetActive(true);
		yield return new WaitForSeconds(1f);
		array = spriteRenderers;
		foreach (SpriteRenderer spriteRenderer2 in array)
		{
			if (spriteRenderer2.material.shader == UberShaderRef)
			{
				spriteRenderer2.material = oldMat;
			}
		}
		LightingObject.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", base.transform.position);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(base.transform.position - Vector3.forward, Vector3.one * 5f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		Container.SetActive(false);
		Portal.gameObject.SetActive(true);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(base.transform.position, new Vector3(5f, 5f, 2f));
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.3f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		EnterEndlessMode.enabled = true;
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator ShakeCameraWithRampUp()
	{
		float t = 0f;
		while (true)
		{
			float num;
			t = (num = t + Time.deltaTime);
			if (!(num < 1f))
			{
				break;
			}
			float t2 = t / 1f;
			CameraManager.instance.ShakeCameraForDuration(Mathf.Lerp(0f, 0.5f, t2), Mathf.Lerp(0f, 1.5f, t2), 3.9f, false);
			yield return null;
		}
		CameraManager.instance.Stopshake();
	}
}
