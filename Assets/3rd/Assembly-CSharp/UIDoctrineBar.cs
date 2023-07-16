using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDoctrineBar : BaseMonoBehaviour
{
	private SermonCategory CurrentSermonCategory;

	public Image BarInstant;

	public Image BarLerp;

	public Image BarFlash;

	public TextMeshProUGUI SermonXPText;

	public RectTransform Container;

	public float DoctrineXPTarget
	{
		get
		{
			return DoctrineUpgradeSystem.GetXPTargetBySermon(CurrentSermonCategory);
		}
	}

	public IEnumerator Show(float xp, SermonCategory CurrentSermonCategory)
	{
		this.CurrentSermonCategory = CurrentSermonCategory;
		float Progress = 0f;
		float Duration = 0.5f;
		SermonXPText.text = Mathf.RoundToInt(xp * 10f) + "/" + Mathf.RoundToInt(DoctrineXPTarget * 10f);
		BarInstant.transform.localScale = new Vector3(Mathf.Clamp(xp / DoctrineXPTarget, 0f, 1f), 1f);
		BarLerp.transform.localScale = new Vector3(Mathf.Clamp(xp / DoctrineXPTarget, 0f, 1f), 1f);
		BarFlash.enabled = false;
		if (Container.anchoredPosition.y >= 150f)
		{
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime);
				if (!(num < Duration))
				{
					break;
				}
				Container.anchoredPosition = Vector3.Lerp(new Vector3(0f, -150f), new Vector3(0f, 150f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
				yield return null;
			}
		}
		Container.anchoredPosition = new Vector3(0f, 150f);
		yield return new WaitForSeconds(0.5f);
	}

	public IEnumerator Hide()
	{
		if (Container.anchoredPosition.y == -150f)
		{
			yield break;
		}
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.anchoredPosition = Vector3.Lerp(new Vector3(0f, 150f), new Vector3(0f, -150f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.anchoredPosition = new Vector3(0f, -150f);
		BarInstant.transform.localScale = Vector3.zero;
		BarLerp.transform.localScale = Vector3.zero;
	}

	public IEnumerator UpdateFirstBar(float xp, float duration)
	{
		float Progress = 0f;
		xp = Mathf.Clamp(xp, 0f, DoctrineXPTarget);
		Vector3 Starting = BarInstant.transform.localScale;
		Vector3 Target = new Vector3(Mathf.Min(1f, xp / DoctrineXPTarget), 1f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < duration))
			{
				break;
			}
			BarInstant.transform.localScale = Vector3.Lerp(Starting, Target, Progress / duration);
			SermonXPText.text = Mathf.RoundToInt(xp * 10f) + "/" + Mathf.RoundToInt(DoctrineXPTarget * 10f);
			yield return null;
		}
		BarInstant.transform.localScale = new Vector3(Mathf.Min(1f, xp / DoctrineXPTarget), 1f);
		StartCoroutine(ShakeRoutine());
	}

	public IEnumerator UpdateSecondBar(float xp, float duration)
	{
		float progress = 0f;
		xp = Mathf.Clamp(xp, 0f, DoctrineXPTarget);
		float StartingFloat = BarLerp.transform.localScale.x;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime);
			if (num < duration)
			{
				BarLerp.transform.localScale = new Vector3(Mathf.SmoothStep(StartingFloat, xp / DoctrineXPTarget, progress / duration), 1f);
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
