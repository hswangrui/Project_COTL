using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KBBackground : MonoBehaviour
{
	private const string kRectBRotate = "_RectBRotate";

	private const float kRectBRotateStartValue = 0f;

	private const float kRectBRotateEndValue = 0.5f;

	private const string kRectMaskCutoff = "_RectMaskCutoff";

	private const float kRectMaskCutoffStartValue = 0.541f;

	private const float kRectMaskCutoffEndValue = 0.7f;

	private const string kNoiseAInf = "_NoiseAInf";

	private const float kNoiseAInfStartValue = 0.893f;

	private const float kNoiseAInfEndValue = 0.2f;

	[SerializeField]
	private Image _background;

	private Material _backgroundMaterialInstance;

	private void Awake()
	{
		_backgroundMaterialInstance = new Material(_background.material);
		_background.material = _backgroundMaterialInstance;
	}

	private void OnDestroy()
	{
		if (_backgroundMaterialInstance != null)
		{
			Object.Destroy(_backgroundMaterialInstance);
			_backgroundMaterialInstance = null;
		}
	}

	public IEnumerator TransitionToEndValues()
	{
		float progress = 0f;
		float duration = 1f;
		while (progress < duration)
		{
			progress = Mathf.Clamp(progress + Time.unscaledDeltaTime, 0f, duration);
			float t = progress / duration;
			_backgroundMaterialInstance.SetFloat("_NoiseAInf", Mathf.SmoothStep(0.893f, 0.2f, t));
			_backgroundMaterialInstance.SetFloat("_RectMaskCutoff", Mathf.SmoothStep(0.541f, 0.7f, t));
			_backgroundMaterialInstance.SetFloat("_RectBRotate", Mathf.SmoothStep(0f, 0.5f, t));
			yield return null;
		}
	}

	public IEnumerator TransitionToStartValues()
	{
		float progress = 0f;
		float duration = 1f;
		while (progress < duration)
		{
			progress = Mathf.Clamp(progress + Time.unscaledDeltaTime, 0f, duration);
			float t = progress / duration;
			_backgroundMaterialInstance.SetFloat("_NoiseAInf", Mathf.SmoothStep(0.2f, 0.893f, t));
			_backgroundMaterialInstance.SetFloat("_RectMaskCutoff", Mathf.SmoothStep(0.7f, 0.541f, t));
			_backgroundMaterialInstance.SetFloat("_RectBRotate", Mathf.SmoothStep(0.5f, 0f, t));
			yield return null;
		}
	}
}
