using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRitualPerformed : BaseMonoBehaviour
{
	public RectTransform Container;

	public Image WhiteFlash;

	public Image BackFlash;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI DescriptionText;

	public void Play(UpgradeSystem.Type RitualType)
	{
		TitleText.text = UpgradeSystem.GetLocalizedName(RitualType);
		DescriptionText.text = UpgradeSystem.GetLocalizedActivated(RitualType);
		StartCoroutine(PlayRoutine());
	}

	private IEnumerator PlayRoutine()
	{
		BackFlash.enabled = false;
		Container.localScale = Vector3.one * 2f;
		Container.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		WhiteFlash.color = new Color(1f, 1f, 1f, 0.6f);
		WhiteFlash.DOColor(new Color(0f, 0f, 0f, 1f), 0.3f);
		yield return new WaitForSeconds(0.4f);
		CameraManager.instance.ShakeCameraForDuration(1.2f, 1.5f, 0.3f);
		GameManager.GetInstance().HitStop();
		BackFlash.enabled = true;
		BackFlash.color = Color.white;
		BackFlash.DOColor(new Color(1f, 1f, 1f, 0f), 0.3f);
		BackFlash.rectTransform.localScale = Vector3.one;
		BackFlash.rectTransform.DOScale(new Vector3(1.5f, 1.5f), 0.3f);
		WhiteFlash.color = new Color(1f, 1f, 1f, 0.6f);
		WhiteFlash.DOColor(new Color(0f, 0f, 0f, 1f), 0.3f);
		Container.DOShakePosition(0.5f, 10f);
		while (!InputManager.UI.GetAcceptButtonDown())
		{
			yield return null;
		}
		Container.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
		{
			Object.Destroy(base.gameObject);
		});
	}
}
