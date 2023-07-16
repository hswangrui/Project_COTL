using System;
using System.Collections;
using System.Collections.Generic;
using MMTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_BlackSoul : BaseMonoBehaviour
{
	public Image ProgressBar;

	public Image ProgressBarInstant;

	public ParticleSystem particleSystem;

	public GameObject FollowerTokensObject;

	public TextMeshProUGUI TokenText;

	public NotificationCentreScreen HUD_BlackSoulNotification;

	public static List<int> UpgradeTargets = new List<int>
	{
		70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
		150, 200
	};

	public List<ConversationEntry> ConversationEntryTutorial;

	public GameObject TutorialObject;

	private RectTransform TutorialRectTransform;

	public RectTransform RingsObject;

	private CanvasGroup canvasGroup;

	public int ParseTokens;

	private void Start()
	{
		if (DataManager.Instance.BlackSoulTarget == 0)
		{
			DataManager.Instance.BlackSoulTarget = UpgradeTargets[Mathf.Min(DataManager.Instance.Followers.Count + DataManager.Instance.FollowerTokens, UpgradeTargets.Count - 1)];
		}
		particleSystem.Stop();
		Image progressBar = ProgressBar;
		float fillAmount = (ProgressBarInstant.fillAmount = (float)Inventory.BlackSouls / GetUpgradeTarget());
		progressBar.fillAmount = fillAmount;
		if (!DataManager.Instance.BlackSoulsEnabled)
		{
			RingsObject.gameObject.SetActive(false);
		}
	}

	public void DoTutorial()
	{
		MMConversation.Play(new ConversationObject(ConversationEntry.CloneList(ConversationEntryTutorial), null, delegate
		{
			StartCoroutine(StartTutorialRoutine());
			StartCoroutine(ScaleProgressBarRoutine());
			DataManager.Instance.BlackSoulsEnabled = true;
		}));
	}

	private IEnumerator StartTutorialRoutine()
	{
		if (Interaction_DeathNPC.Instance != null)
		{
			Interaction_DeathNPC.Instance.enabled = false;
		}
		TutorialRectTransform = UnityEngine.Object.Instantiate(TutorialObject, base.transform.parent).GetComponent<RectTransform>();
		TutorialRectTransform.SetSiblingIndex(base.transform.GetSiblingIndex());
		Time.timeScale = 0f;
		canvasGroup = TutorialRectTransform.GetComponent<CanvasGroup>();
		float Progress = 0f;
		float Duration = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (num < Duration)
			{
				canvasGroup.alpha = Progress / Duration;
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator ScaleProgressBarRoutine()
	{
		yield return new WaitForSecondsRealtime(1f);
		RingsObject.gameObject.SetActive(true);
		CameraManager.shakeCamera(0.3f);
		RingsObject.position = TutorialRectTransform.position;
		ProgressBar.fillAmount = 0f;
		ProgressBarInstant.fillAmount = 0f;
		float Progress = 0f;
		float Duration2 = 0.3f;
		float StartingScale = RingsObject.localScale.x;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(2f, StartingScale, Progress / Duration2);
			RingsObject.localScale = Vector3.one * num2;
			yield return null;
		}
		yield return new WaitForSecondsRealtime(0.5f);
		CameraManager.shakeCamera(0.3f);
		ProgressBarInstant.fillAmount = 0.5f;
		yield return new WaitForSecondsRealtime(0.5f);
		Progress = 0f;
		Duration2 = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			ProgressBar.fillAmount = Mathf.SmoothStep(0f, 0.5f, Progress / Duration2);
			yield return null;
		}
		yield return new WaitForSecondsRealtime(0.5f);
		CameraManager.shakeCamera(0.3f);
		ProgressBarInstant.fillAmount = 1f;
		yield return new WaitForSecondsRealtime(0.5f);
		Progress = 0f;
		Duration2 = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			ProgressBar.fillAmount = Mathf.SmoothStep(0.5f, 1f, Progress / Duration2);
			yield return null;
		}
		yield return new WaitForSecondsRealtime(0.1f);
		ParseTokens = Inventory.FollowerTokens;
		Inventory.FollowerTokens = 1;
		CameraManager.shakeCamera(0.3f);
		StartCoroutine(ExitTutorialRoutine());
	}

	private IEnumerator ExitTutorialRoutine()
	{
		while (!InputManager.UI.GetAcceptButtonUp() && !InputManager.UI.GetCancelButtonUp())
		{
			yield return null;
		}
		EndTutorial();
	}

	private void EndTutorial()
	{
		StopAllCoroutines();
		StartCoroutine(EndTutorialRoutine());
	}

	private IEnumerator EndTutorialRoutine()
	{
		Time.timeScale = 1f;
		float Progress = 0f;
		float Duration = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			canvasGroup.alpha = 1f - Progress / Duration;
			yield return null;
		}
		UnityEngine.Object.Destroy(TutorialRectTransform.gameObject);
		yield return new WaitForSecondsRealtime(0.1f);
		Progress = 0f;
		Duration = 0.5f;
		Vector3 StartPosition = RingsObject.localPosition;
		float Scale = 1f;
		float ScaleSpeed4 = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ScaleSpeed4 += (1.1f - Scale) * 0.4f;
			float num2 = Scale;
			ScaleSpeed4 = (num = ScaleSpeed4 * 0.6f);
			Scale = num2 + num * (Time.unscaledDeltaTime * 60f);
			RingsObject.localScale = Vector3.one * Scale;
			yield return null;
		}
		Progress = 0f;
		Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			RingsObject.localPosition = Vector3.Lerp(StartPosition, Vector3.zero, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		yield return new WaitForSecondsRealtime(0.2f);
		ScaleSpeed4 = 0f;
		Progress = 0f;
		Duration = 0.5f;
		CameraManager.shakeCamera(0.4f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ScaleSpeed4 += (1f - Scale) * 0.2f;
			float num3 = Scale;
			ScaleSpeed4 = (num = ScaleSpeed4 * 0.6f);
			Scale = num3 + num * (Time.unscaledDeltaTime * 60f);
			RingsObject.localScale = Vector3.one * Scale;
			yield return null;
		}
		RingsObject.localScale = Vector3.one;
		yield return new WaitForSecondsRealtime(0.5f);
		ProgressBarInstant.fillAmount = (float)Inventory.BlackSouls / GetUpgradeTarget();
		Progress = 0f;
		Duration = 0.5f;
		float StartFill = ProgressBar.fillAmount;
		float EndFill = (float)Inventory.BlackSouls / GetUpgradeTarget();
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ProgressBar.fillAmount = Mathf.SmoothStep(StartFill, EndFill, Progress / Duration);
			yield return null;
		}
		Inventory.FollowerTokens = ParseTokens;
		yield return new WaitForSecondsRealtime(0.5f);
		if (Interaction_DeathNPC.Instance != null)
		{
			Interaction_DeathNPC.Instance.enabled = true;
		}
	}

	public float GetUpgradeTarget()
	{
		return DataManager.Instance.BlackSoulTarget;
	}

	private void OnEnable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(OnGetFollowerToken));
		PlayerFarming.OnGetBlackSoul += OnGetBlackSoul;
		Inventory.OnGetFollowerToken = (Inventory.GetFollowerToken)Delegate.Combine(Inventory.OnGetFollowerToken, new Inventory.GetFollowerToken(OnGetFollowerToken));
		Inventory.FollowerTokens = DataManager.Instance.FollowerTokens;
		OnGetFollowerToken();
	}

	private void OnDisable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnGetFollowerToken));
		PlayerFarming.OnGetBlackSoul -= OnGetBlackSoul;
		Inventory.OnGetFollowerToken = (Inventory.GetFollowerToken)Delegate.Remove(Inventory.OnGetFollowerToken, new Inventory.GetFollowerToken(OnGetFollowerToken));
	}

	private void OnGetFollowerToken()
	{
		DataManager.Instance.BlackSoulTarget = UpgradeTargets[Mathf.Min(DataManager.Instance.Followers.Count + DataManager.Instance.Followers_Recruit.Count + DataManager.Instance.FollowerTokens, UpgradeTargets.Count - 1)];
		TokenText.text = DataManager.Instance.FollowerTokens.ToString();
		if (Inventory.FollowerTokens > 0)
		{
			FollowerTokensObject.SetActive(true);
		}
		else
		{
			FollowerTokensObject.SetActive(false);
		}
	}

	private void OnGetBlackSoul(int DeltaValue)
	{
		particleSystem.Play();
		if ((float)Inventory.BlackSouls / GetUpgradeTarget() >= 1f)
		{
			Inventory.FollowerTokens++;
			Inventory.BlackSouls = 0;
			Image progressBar = ProgressBar;
			float fillAmount = (ProgressBarInstant.fillAmount = (float)Inventory.BlackSouls / GetUpgradeTarget());
			progressBar.fillAmount = fillAmount;
		}
		else
		{
			UpdateProgressBar();
		}
	}

	private void UpdateProgressBar()
	{
		ProgressBarInstant.fillAmount = (float)Inventory.BlackSouls / GetUpgradeTarget();
		if (ProgressBar.fillAmount > ProgressBarInstant.fillAmount)
		{
			ProgressBar.fillAmount = ProgressBarInstant.fillAmount;
			return;
		}
		StopAllCoroutines();
		StartCoroutine(SmoothStepRoutine());
	}

	private IEnumerator SmoothStepRoutine()
	{
		yield return new WaitForSeconds(1f);
		float Progress = 0f;
		float Duration = 0.5f;
		float StartPosition = ProgressBar.fillAmount;
		float TargetPosition = ProgressBarInstant.fillAmount;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ProgressBar.fillAmount = Mathf.SmoothStep(StartPosition, TargetPosition, Progress / Duration);
			yield return null;
		}
		ProgressBar.fillAmount = ProgressBarInstant.fillAmount;
	}
}
