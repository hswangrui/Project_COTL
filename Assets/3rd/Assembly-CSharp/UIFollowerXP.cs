using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowerXP : BaseMonoBehaviour
{
	public CanvasGroup canvasGroup;

	public GameObject IconPrefab;

	public Transform ContainerTransform;

	private List<UIFollowerXPIcon> Icons = new List<UIFollowerXPIcon>();

	private bool ReleaseButton;

	public void Play()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			UIFollowerXPIcon component = Object.Instantiate(IconPrefab, ContainerTransform).GetComponent<UIFollowerXPIcon>();
			Icons.Add(component);
			component.Play(allBrain);
		}
		StartCoroutine(AwaitClose());
	}

	private IEnumerator AwaitClose()
	{
		Time.timeScale = 0f;
		float Progress = 0f;
		float Duration2 = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			canvasGroup.alpha = Progress / Duration2;
			yield return null;
		}
		float num2 = -0.1f;
		foreach (UIFollowerXPIcon icon in Icons)
		{
			icon.UpdateXP(num2 += 0.1f);
		}
		while (!InputManager.UI.GetAcceptButtonHeld() && !InputManager.UI.GetCancelButtonHeld())
		{
			yield return null;
		}
		Progress = 0f;
		Duration2 = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			canvasGroup.alpha = 1f - Progress / Duration2;
			yield return null;
		}
		Time.timeScale = 1f;
		Object.Destroy(base.gameObject);
	}
}
