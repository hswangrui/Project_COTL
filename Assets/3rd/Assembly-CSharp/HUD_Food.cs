using UnityEngine;
using UnityEngine.UI;

public class HUD_Food : BaseMonoBehaviour
{
	public RectTransform Container;

	public Image ProgressRing;

	public float LerpSpeed = 1f;

	public Image ProgressRingHappiness;

	private bool _offscreen;

	private void Start()
	{
		int count = DataManager.Instance.Followers.Count;
		_offscreen = count <= 0;
		Container.anchoredPosition = (_offscreen ? new Vector3(0f, 300f) : Vector3.zero);
	}

	private void Update()
	{
		if (_offscreen)
		{
			Container.anchoredPosition = Vector3.Lerp(Container.anchoredPosition, new Vector3(0f, 300f), Time.deltaTime * 5f);
			return;
		}
		Container.anchoredPosition = Vector3.Lerp(Container.anchoredPosition, new Vector3(0f, 0f), Time.deltaTime * 5f);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			num += allBrain.Stats.Happiness;
			num2 += allBrain.Stats.Satiation + (75f - allBrain.Stats.Starvation);
			num3 += 1f;
		}
		ProgressRing.fillAmount = ((num3 <= 0f) ? 0f : (num2 / (175f * num3)));
		ProgressRingHappiness.fillAmount = ((num3 <= 0f) ? 0f : (num / (100f * num3)));
	}
}
