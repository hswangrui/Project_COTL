using System.Collections;
using UnityEngine;

public class UICrownAbilitySelect : BaseMonoBehaviour
{
	public bool WaitForNoConversation;

	public CanvasGroup canvasGroup;

	private void OnEnable()
	{
		StartCoroutine(DoRoutine());
	}

	private IEnumerator DoRoutine()
	{
		if (WaitForNoConversation)
		{
			GameManager gameManager = GameManager.GetInstance();
			while (gameManager.CamFollowTarget.IN_CONVERSATION)
			{
				yield return null;
			}
		}
		Time.timeScale = 0f;
		float Progress2 = 0f;
		float Duration2 = 0.5f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			canvasGroup.alpha = Progress2 / Duration2;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		while (!InputManager.UI.GetAcceptButtonDown())
		{
			yield return null;
		}
		Progress2 = 0f;
		Duration2 = 0.5f;
		Time.timeScale = 1f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			canvasGroup.alpha = 1f - Progress2 / Duration2;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}
}
