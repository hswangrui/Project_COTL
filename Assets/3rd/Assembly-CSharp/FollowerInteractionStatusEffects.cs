using System.Collections;
using UnityEngine;

public class FollowerInteractionStatusEffects : BaseMonoBehaviour
{
	public enum EffectIcon
	{
		Fasting
	}

	private Follower follower;

	public GameObject IconPrefab;

	private RectTransform _rectTransform;

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
		set
		{
			_rectTransform = value;
		}
	}

	public void Init(Follower follower)
	{
		float num = 0.1f;
		this.follower = follower;
		if (FollowerBrainStats.Fasting)
		{
			AddEffect(true, num += 0.1f, EffectIcon.Fasting);
		}
	}

	public void AddEffect(bool Instant, EffectIcon Effect)
	{
		AddEffect(Instant, 0f, Effect);
	}

	public void AddEffect(bool Instant, float Delay, EffectIcon Effect)
	{
		StartCoroutine(AddEffectRoutine(Instant, Delay, Effect));
	}

	private IEnumerator AddEffectRoutine(bool Instant, float Delay, EffectIcon Effect)
	{
		yield return new WaitForSeconds(Delay);
		GameObject gameObject = Object.Instantiate(IconPrefab, base.transform);
		gameObject.SetActive(true);
		FollowerInteractionStatusEffectIcon component = gameObject.GetComponent<FollowerInteractionStatusEffectIcon>();
		if (Effect == EffectIcon.Fasting)
		{
			component.Text.text = "Fasting";
		}
		else
		{
			component.Text.text = "Test Effect!";
			component.RadialProgress.fillAmount = 0.2f;
		}
		StartCoroutine(ScaleIcon(gameObject.transform));
	}

	private IEnumerator ScaleIcon(Transform t)
	{
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				t.localScale = Vector3.Lerp(Vector3.one * 0f, Vector3.one, Mathf.SmoothStep(0f, 1f, Progress / Duration));
				yield return null;
				continue;
			}
			break;
		}
	}
}
