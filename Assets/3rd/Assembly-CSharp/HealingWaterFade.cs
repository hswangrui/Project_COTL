using System.Collections;
using UnityEngine;

public class HealingWaterFade : BaseMonoBehaviour
{
	public SpriteRenderer BlueWater;

	public SpriteRenderer BlackWater;

	public BaseMonoBehaviour HealingComponent;

	public BaseMonoBehaviour SplashComponent;

	public GameObject HeartGem;

	public GameObject HeartGemBroken;

	public void CrossFade()
	{
		StartCoroutine(CrossFadeRoutine());
	}

	private IEnumerator CrossFadeRoutine()
	{
		HeartGem.SetActive(false);
		HeartGemBroken.SetActive(true);
		CameraManager.shakeCamera(0.5f, Random.Range(0, 360));
		BlueWater.gameObject.SetActive(false);
		BlackWater.gameObject.SetActive(true);
		Color CurrentColor = new Color(1f, 0f, 0f, 0f);
		Color TargetColor = BlackWater.color;
		float Progress = 0f;
		float Duration = 2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			BlackWater.color = Color.Lerp(CurrentColor, TargetColor, Progress / Duration);
			yield return null;
		}
		HealingComponent.enabled = false;
		SplashComponent.enabled = true;
	}
}
