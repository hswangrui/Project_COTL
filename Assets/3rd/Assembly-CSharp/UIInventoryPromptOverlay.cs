using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UIInventoryPromptOverlay : BaseMonoBehaviour
{
	public static bool Showing;

	private float kTransitionTime = 1f;

	[SerializeField]
	private RectTransform _promptRectTransform;

	[SerializeField]
	private CanvasGroup _promptCanvasGroup;

	private void Awake()
	{
		Showing = true;
		_promptRectTransform.localScale = Vector3.zero;
		_promptCanvasGroup.alpha = 0f;
	}

	private IEnumerator ScaleButton()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			_promptRectTransform.transform.DOPunchScale(new Vector3(0.12f, 0.12f), 0.5f).SetUpdate(true);
			yield return null;
		}
	}

	private IEnumerator Start()
	{
		StartCoroutine(ScaleButton());
		_promptRectTransform.DOScale(Vector3.one, kTransitionTime).SetEase(Ease.OutBack).SetUpdate(true);
		_promptCanvasGroup.DOFade(1f, kTransitionTime * 0.5f).SetUpdate(true);
		while (!InputManager.Gameplay.GetMenuButtonDown())
		{
			yield return null;
		}
		StopCoroutine(ScaleButton());
		Object.Destroy(base.gameObject);
	}
}
