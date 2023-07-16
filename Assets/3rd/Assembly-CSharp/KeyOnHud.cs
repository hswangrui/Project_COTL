using System;
using System.Collections;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.UI;

public class KeyOnHud : BaseMonoBehaviour
{
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Image image;

	private void Start()
	{
		image.enabled = false;
		BiomeGenerator.OnGetKey = (BiomeGenerator.GetKey)Delegate.Combine(BiomeGenerator.OnGetKey, new BiomeGenerator.GetKey(Reveal));
		BiomeGenerator.OnUseKey = (BiomeGenerator.GetKey)Delegate.Combine(BiomeGenerator.OnUseKey, new BiomeGenerator.GetKey(Hide));
	}

	private void OnDisable()
	{
		BiomeGenerator.OnGetKey = (BiomeGenerator.GetKey)Delegate.Remove(BiomeGenerator.OnGetKey, new BiomeGenerator.GetKey(Reveal));
		BiomeGenerator.OnUseKey = (BiomeGenerator.GetKey)Delegate.Remove(BiomeGenerator.OnUseKey, new BiomeGenerator.GetKey(Hide));
	}

	private void Hide()
	{
		StopAllCoroutines();
		image.enabled = false;
	}

	private void Reveal()
	{
		StartCoroutine(RevealRoutine());
	}

	private IEnumerator RevealRoutine()
	{
		yield return new WaitForSeconds(3.5f);
		image.enabled = true;
		float Progress = 0f;
		float Duration = 0.5f;
		float num = 3f;
		float ScaleStart = num;
		float ScaleEnd = 1f;
		while (true)
		{
			float num2;
			Progress = (num2 = Progress + Time.deltaTime);
			if (num2 < Duration)
			{
				num = Mathf.Lerp(ScaleStart, ScaleEnd, curve.Evaluate(Progress / Duration));
				image.rectTransform.localScale = new Vector3(num, num);
				yield return null;
				continue;
			}
			break;
		}
	}
}
