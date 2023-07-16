using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD_DiscipleXP : MonoBehaviour
{
	public Image InstantBar;

	public Image LerpBar;

	public Image FlashBar;

	private Coroutine cLerpBarRoutine;

	private Coroutine cFlashBarRoutine;

	private float XP
	{
		get
		{
			return DataManager.Instance.DiscipleXP;
		}
	}

	private float TargetXP
	{
		get
		{
			return DataManager.TargetDiscipleXP[Mathf.Min(DataManager.Instance.DiscipleLevel, DataManager.TargetDiscipleXP.Count - 1)];
		}
	}

	private void OnEnable()
	{
		PlayerFarming.OnGetDiscipleXP = (Action)Delegate.Combine(PlayerFarming.OnGetDiscipleXP, new Action(OnGetXP));
		RectTransform rectTransform = InstantBar.rectTransform;
		Vector3 localScale = (LerpBar.rectTransform.localScale = new Vector3(XP / TargetXP, 1f));
		rectTransform.localScale = localScale;
		FlashBar.enabled = false;
	}

	private void OnDisable()
	{
		PlayerFarming.OnGetDiscipleXP = (Action)Delegate.Remove(PlayerFarming.OnGetDiscipleXP, new Action(OnGetXP));
	}

	private void OnGetXP()
	{
		if (XP >= TargetXP)
		{
			RectTransform rectTransform = LerpBar.rectTransform;
			Vector3 localScale = (InstantBar.rectTransform.localScale = Vector3.zero);
			rectTransform.localScale = localScale;
			if (cFlashBarRoutine != null)
			{
				StopCoroutine(cFlashBarRoutine);
			}
			StartCoroutine(FlashBarRoutine());
		}
		else
		{
			InstantBar.rectTransform.localScale = new Vector3(XP / TargetXP, 1f);
			if (cLerpBarRoutine != null)
			{
				StopCoroutine(cLerpBarRoutine);
			}
			if (InstantBar.rectTransform.localScale.x > LerpBar.rectTransform.localScale.x)
			{
				cLerpBarRoutine = StartCoroutine(LerpBarRoutine());
			}
			else
			{
				LerpBar.rectTransform.localScale = InstantBar.rectTransform.localScale;
			}
		}
	}

	private IEnumerator LerpBarRoutine()
	{
		yield return new WaitForSecondsRealtime(0.2f);
		Vector3 StartPosition = LerpBar.rectTransform.localScale;
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
			LerpBar.rectTransform.localScale = Vector3.Lerp(StartPosition, InstantBar.rectTransform.localScale, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		LerpBar.rectTransform.localScale = InstantBar.rectTransform.localScale;
	}

	private IEnumerator FlashBarRoutine()
	{
		FlashBar.enabled = true;
		FlashBar.color = Color.white;
		Color FadeColor = new Color(1f, 1f, 1f, 0f);
		yield return new WaitForSecondsRealtime(0.5f);
		float Progress = 0f;
		float Duration = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			FlashBar.color = Color.Lerp(Color.white, FadeColor, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		FlashBar.enabled = false;
	}
}
