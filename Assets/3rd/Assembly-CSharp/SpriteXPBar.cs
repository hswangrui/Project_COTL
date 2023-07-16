using DG.Tweening;
using UnityEngine;

public class SpriteXPBar : MonoBehaviour
{
	public SpriteRenderer ProgressBar;

	public SpriteRenderer ProgressBarTmpFill;

	public Transform XPBar;

	public bool HideOnEmpty = true;

	private SpriteRenderer[] SpriteRenderers;

	private Quaternion _startRot;

	[SerializeField]
	private Color fullColor = new Color(0.9921569f, 0.1137255f, 0.01176471f, 1f);

	private float _value;

	private bool hidden;

	private bool lessValue;

	private void Awake()
	{
		_startRot = XPBar.rotation;
		SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		ProgressBarTmpFill.transform.DOScaleX(0f, 0.1f).SetUpdate(true);
		ProgressBar.transform.DOScaleX(0f, 0.1f).SetUpdate(true);
	}

	public void UpdateBar(float value)
	{
		if (value < _value)
		{
			lessValue = true;
		}
		else
		{
			lessValue = false;
		}
		_value = value;
		XPBar.DOComplete();
		XPBar.DOKill();
		ProgressBar.DOKill();
		ProgressBar.transform.DOKill();
		ProgressBarTmpFill.DOKill();
		ProgressBarTmpFill.transform.DOKill();
		if (value <= 0f)
		{
			ProgressBarTmpFill.transform.DOScaleX(0f, 0.1f);
			ProgressBar.transform.DOScaleX(0f, 0.1f);
			if (HideOnEmpty)
			{
				Hide();
			}
			hidden = true;
			return;
		}
		if (hidden || (SpriteRenderers.Length != 0 && SpriteRenderers[0].color.a == 0f))
		{
			Show();
			hidden = false;
		}
		XPBar.DOPunchRotation(Vector3.back * 10f, 0.5f).SetEase(Ease.OutBounce);
		if (_value >= 1f)
		{
			ProgressBar.transform.DOScaleX(value, 0.1f).SetEase(Ease.OutQuart);
			ProgressBar.DOColor(fullColor, 1f);
		}
		else if (!lessValue)
		{
			ProgressBarTmpFill.transform.DOScaleX(value, 0.1f).SetEase(Ease.OutQuart);
			ProgressBar.transform.DOScaleX(value, 1f).SetEase(Ease.OutQuart).SetDelay(1f);
		}
		else
		{
			ProgressBarTmpFill.transform.DOScaleX(value, 0.1f).SetEase(Ease.OutQuart);
			ProgressBar.transform.DOScaleX(value, 0.1f).SetEase(Ease.OutQuart);
		}
	}

	public void Hide(bool instant = false)
	{
		if (SpriteRenderers != null)
		{
			SpriteRenderer[] spriteRenderers = SpriteRenderers;
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				spriteRenderers[i].DOFade(0f, instant ? 0f : 1f);
			}
		}
		hidden = true;
	}

	public void Show(bool instant = false)
	{
		if (SpriteRenderers != null)
		{
			SpriteRenderer[] spriteRenderers = SpriteRenderers;
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				spriteRenderers[i].DOFade(1f, instant ? 0f : 1f);
			}
		}
		hidden = false;
	}
}
