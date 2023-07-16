using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImpactFrame : MonoBehaviour
{
	public Image image;

	private float Duration = 2f;

	private float Delay;

	private bool UseDeltaTime = true;

	private Material clonedMaterial;

	private void OnEnable()
	{
		image.enabled = false;
	}

	public void ShowForDuration(float _Duration = 0.2f, float _Delay = 0f)
	{
		image.enabled = true;
		Duration = _Duration;
		Delay = _Delay;
		StartCoroutine(FadeInRoutine());
	}

	public void Show()
	{
		MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, 0.5f);
		image.enabled = true;
	}

	public void Hide()
	{
		image.enabled = false;
	}

	private IEnumerator FadeInRoutine()
	{
		yield return new WaitForSecondsRealtime(Delay);
		image.enabled = true;
		yield return new WaitForSecondsRealtime(Duration);
		base.gameObject.SetActive(false);
	}
}
