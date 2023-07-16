using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoopFade : BaseMonoBehaviour
{
	public Image image;

	private float Duration = 2f;

	private float Delay;

	public float value;

	private bool UseDeltaTime = true;

	private Material clonedMaterial;

	private void Start()
	{
		clonedMaterial = new Material(image.materialForRendering);
		image.material = clonedMaterial;
	}

	public void FadeIn(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true)
	{
		image.enabled = true;
		this.UseDeltaTime = UseDeltaTime;
		Duration = _Duration;
		Delay = _Delay;
		StartCoroutine(FadeInRoutine());
	}

	public void FadeOut(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true)
	{
		image.enabled = true;
		this.UseDeltaTime = UseDeltaTime;
		Duration = _Duration;
		Delay = _Delay;
		StartCoroutine(FadeOutRoutine());
	}

	private IEnumerator FadeInRoutine()
	{
		if (UseDeltaTime)
		{
			yield return new WaitForSeconds(Delay);
		}
		else
		{
			yield return new WaitForSecondsRealtime(Delay);
		}
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (num < Duration)
			{
				image.materialForRendering.SetFloat("_RectMaskCutoff", Mathf.SmoothStep(0.85f, 0.3f, Progress / Duration));
				value = Mathf.SmoothStep(1f, 0f, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator FadeOutRoutine()
	{
		if (UseDeltaTime)
		{
			yield return new WaitForSeconds(Delay);
		}
		else
		{
			yield return new WaitForSecondsRealtime(Delay);
		}
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			image.materialForRendering.SetFloat("_RectMaskCutoff", Mathf.SmoothStep(0.3f, 0.85f, Progress / Duration));
			value = Mathf.SmoothStep(0f, 1f, Progress / Duration);
			yield return null;
		}
		base.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		image.enabled = false;
		image.materialForRendering.SetFloat("_RectMaskCutoff", 1f);
	}
}
