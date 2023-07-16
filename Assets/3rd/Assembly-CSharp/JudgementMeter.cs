using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class JudgementMeter : MonoBehaviour
{
	public static bool JudgementEnabled;

	private static JudgementMeter instance;

	private const int Max = 4;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Image needle;

	[SerializeField]
	private GameObject topGlow;

	[SerializeField]
	private GameObject bottomGlow;

	private int Judgement
	{
		get
		{
			return DataManager.Instance.JudgementAmount;
		}
		set
		{
			DataManager.Instance.JudgementAmount = value;
		}
	}

	public static void Show()
	{
		if (JudgementEnabled)
		{
			if (instance == null)
			{
				instance = Object.Instantiate(Resources.Load<JudgementMeter>("Prefabs/UI/UI Judgement Meter"), GameObject.FindGameObjectWithTag("Canvas").transform);
			}
			instance.gameObject.SetActive(true);
			instance.canvasGroup.alpha = 0f;
			instance.canvasGroup.DOFade(1f, 0.25f);
			instance.needle.transform.localPosition = new Vector3(instance.needle.transform.localPosition.x, 48 * instance.Judgement, instance.needle.transform.localPosition.z);
			instance.topGlow.SetActive(instance.Judgement >= 4);
			instance.bottomGlow.SetActive(instance.Judgement <= -4);
		}
	}

	private void Hide()
	{
		canvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			base.gameObject.SetActive(false);
		});
	}

	public static void ShowModify(int increment)
	{
		if (!JudgementEnabled)
		{
			DataManager.Instance.JudgementAmount += increment;
			return;
		}
		Show();
		instance.StartCoroutine(instance.ModifyIE(increment));
	}

	private IEnumerator ModifyIE(int increment)
	{
		yield return new WaitForSeconds(1f);
		int previous = Judgement;
		Judgement = Mathf.Clamp(Judgement + increment, -4, 4);
		if (previous != Judgement)
		{
			ShortcutExtensions.DOLocalMove(endValue: new Vector3(instance.needle.transform.localPosition.x, 48 * instance.Judgement, instance.needle.transform.localPosition.z), target: needle.transform, duration: 0.25f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				if (instance.Judgement >= 4)
				{
					Vector3 localScale = instance.topGlow.transform.localScale;
					instance.topGlow.SetActive(true);
					instance.topGlow.transform.localScale = Vector3.zero;
					instance.topGlow.transform.DOScale(localScale, 0.25f);
				}
				else if (instance.Judgement <= -4)
				{
					Vector3 localScale2 = instance.bottomGlow.transform.localScale;
					instance.bottomGlow.SetActive(true);
					instance.bottomGlow.transform.localScale = Vector3.zero;
					instance.bottomGlow.transform.DOScale(localScale2, 0.25f);
				}
				else if (previous >= 4)
				{
					instance.topGlow.transform.DOScale(0f, 0.25f);
				}
				else if (previous <= -4)
				{
					instance.bottomGlow.transform.DOScale(0f, 0.25f);
				}
			});
		}
		else
		{
			Vector3 p = needle.transform.localPosition;
			needle.transform.DOShakePosition(0.5f, 10f).OnComplete(delegate
			{
				needle.transform.localPosition = p;
			});
		}
		yield return new WaitForSeconds(2f);
		Hide();
	}
}
