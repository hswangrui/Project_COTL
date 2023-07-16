using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class CriticalTimer : MonoBehaviour
{
	[SerializeField]
	private Image wheel;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private SkeletonGraphic chargingIcon;

	public void UpdateCharging(float normalised)
	{
		if (normalised > 1f)
		{
			if (!chargingIcon.enabled)
			{
				AudioManager.Instance.PlayOneShot("event:/weapon/crit_ready", base.gameObject);
				chargingIcon.enabled = true;
			}
			if (chargingIcon.AnimationState.GetCurrent(0).Animation.Name != "charged")
			{
				chargingIcon.AnimationState.SetAnimation(0, "charged", true);
			}
			if (canvasGroup.alpha == 1f)
			{
				canvasGroup.DOKill();
				canvasGroup.DOFade(0f, 0.25f);
			}
		}
		else
		{
			if (chargingIcon.enabled)
			{
				chargingIcon.enabled = false;
			}
			if (canvasGroup.alpha == 1f)
			{
				canvasGroup.DOKill();
				canvasGroup.DOFade(0f, 0.25f);
			}
		}
	}
}
