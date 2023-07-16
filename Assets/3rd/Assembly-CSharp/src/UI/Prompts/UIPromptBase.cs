using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using UnityEngine;

namespace src.UI.Prompts
{
	public abstract class UIPromptBase : UIMenuBase
	{
		[SerializeField]
		private RectTransform _containerRectTransform;

		protected override bool _addToActiveMenus
		{
			get
			{
				return false;
			}
		}

		private void Start()
		{
			_canvasGroup.alpha = 0f;
		}

		private void OnEnable()
		{
			LocalizationManager.OnLocalizeEvent += Localize;
		}

		private void OnDisable()
		{
			LocalizationManager.OnLocalizeEvent -= Localize;
		}

		protected override void OnShowStarted()
		{
			_canvasGroup.DOKill();
			_containerRectTransform.DOKill();
			Localize();
		}

		protected override void OnHideStarted()
		{
			_canvasGroup.DOKill();
			_containerRectTransform.DOKill();
		}

		protected abstract void Localize();

		protected override IEnumerator DoShowAnimation()
		{
			AudioManager.Instance.PlayOneShot("event:/ui/open_menu", PlayerFarming.Instance.transform.position);
			Vector3 localPosition = _containerRectTransform.localPosition;
			_containerRectTransform.localPosition = localPosition + Vector3.up * 50f;
			_canvasGroup.alpha = 0f;
			_canvasGroup.DOFade(1f, 0.3f);
			_containerRectTransform.DOLocalMove(localPosition, 0.3f).SetEase(Ease.OutBack);
			yield return new WaitForSeconds(0.3f);
		}

		protected override IEnumerator DoHideAnimation()
		{
			AudioManager.Instance.PlayOneShot("event:/ui/close_menu", PlayerFarming.Instance.transform.position);
			_canvasGroup.DOFade(0f, 0.5f);
			yield return new WaitForSeconds(0.5f);
		}

		protected override void OnHideCompleted()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
