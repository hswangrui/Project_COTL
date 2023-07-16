using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGodTearBar : BaseMonoBehaviour
{
	public Image BarInstant;

	public Image BarLerp;

	public Image BarFlash;

	public TextMeshProUGUI SermonXPText;

	public RectTransform Container;

	private bool Shown;

	public float XPTarget
	{
		get
		{
			return DataManager.Instance.CurrentChallengeModeTargetXP;
		}
	}

	public void Show(float xp)
	{
		if (!Shown)
		{
			StopAllCoroutines();
			StartCoroutine(ShowRoutine(xp));
		}
	}

	public void Hide()
	{
		if (Shown)
		{
			StopAllCoroutines();
			StartCoroutine(HideRoutine());
		}
	}

	public IEnumerator ShowRoutine(float xp)
	{
		Shown = true;
		float Progress = 0f;
		float Duration = 0.5f;
		SermonXPText.text = Mathf.RoundToInt(xp) + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT) + "/" + Mathf.RoundToInt(XPTarget);
		BarInstant.transform.localScale = new Vector3(Mathf.Clamp(xp / XPTarget, 0f, 1f), 1f);
		BarLerp.transform.localScale = new Vector3(Mathf.Clamp(xp / XPTarget, 0f, 1f), 1f);
		BarFlash.enabled = false;
		Vector2 Position = Container.anchoredPosition;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.anchoredPosition = Vector3.Lerp(Position, new Vector3(0f, 150f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.anchoredPosition = new Vector3(0f, 150f);
	}

	public IEnumerator HideRoutine()
	{
		if (Container.anchoredPosition.y == -150f)
		{
			yield break;
		}
		Shown = false;
		float Progress = 0f;
		float Duration = 0.5f;
		Vector2 Position = Container.anchoredPosition;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.anchoredPosition = Vector3.Lerp(Position, new Vector3(0f, -150f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.anchoredPosition = new Vector3(0f, -150f);
		BarInstant.transform.localScale = Vector3.zero;
		BarLerp.transform.localScale = Vector3.zero;
	}

	public IEnumerator UpdateFirstBar(float xp, float duration)
	{
		float Progress = 0f;
		xp = Mathf.Clamp(xp, 0f, XPTarget);
		Vector3 Starting = BarInstant.transform.localScale;
		Vector3 Target = new Vector3(Mathf.Min(1f, xp / XPTarget), 1f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < duration))
			{
				break;
			}
			BarInstant.transform.localScale = Vector3.Lerp(Starting, Target, Progress / duration);
			SermonXPText.text = Mathf.RoundToInt(xp * 10f) + "/" + Mathf.RoundToInt(XPTarget * 10f);
			yield return null;
		}
		BarInstant.transform.localScale = new Vector3(Mathf.Min(1f, xp / XPTarget), 1f);
		StartCoroutine(ShakeRoutine());
	}

	public IEnumerator UpdateSecondBar(float xp, float duration)
	{
		float progress = 0f;
		xp = Mathf.Clamp(xp, 0f, XPTarget);
		float StartingFloat = BarLerp.transform.localScale.x;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime);
			if (num < duration)
			{
				BarLerp.transform.localScale = new Vector3(Mathf.SmoothStep(StartingFloat, xp / XPTarget, progress / duration), 1f);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator ShakeRoutine()
	{
		yield return new WaitForEndOfFrame();
		Container.DOKill();
		Vector3 strength = new Vector3(20f, 0f, 0f);
		Container.anchoredPosition = new Vector3(0f, 150f);
		Container.DOShakePosition(0.25f, strength);
	}

	public IEnumerator FlashBarRoutine(float Delay, float Duration)
	{
		BarFlash.enabled = true;
		BarFlash.color = Color.white;
		Color FadeColor = new Color(1f, 1f, 1f, 0f);
		AudioManager.Instance.PlayOneShot("event:/sermon/select_upgrade", base.gameObject);
		yield return new WaitForSeconds(Delay);
		CameraManager.shakeCamera(1f);
		RumbleManager.Instance.Rumble();
		BarFlash.color = Color.white;
		yield return new WaitForSeconds(0.5f);
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			BarFlash.color = Color.Lerp(Color.white, FadeColor, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		BarFlash.enabled = false;
	}
}
