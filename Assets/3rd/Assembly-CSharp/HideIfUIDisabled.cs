using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideIfUIDisabled : BaseMonoBehaviour
{
	private Renderer[] SpriteRenderers;

	private List<Renderer> HiddenRenderers = new List<Renderer>();

	private Coroutine cHideAllRoutine;

	private void OnEnable()
	{
		if (CheatConsole.HidingUI)
		{
			HideUI();
		}
		CheatConsole.OnHideUI = (Action)Delegate.Combine(CheatConsole.OnHideUI, new Action(HideUI));
		CheatConsole.OnShowUI = (Action)Delegate.Combine(CheatConsole.OnShowUI, new Action(ShowUI));
		PhotoModeManager.OnPhotoModeEnabled += HideUI;
		PhotoModeManager.OnPhotoModeDisabled += ShowUI;
	}

	private void OnDisable()
	{
		CheatConsole.OnHideUI = (Action)Delegate.Remove(CheatConsole.OnHideUI, new Action(HideUI));
		CheatConsole.OnShowUI = (Action)Delegate.Remove(CheatConsole.OnShowUI, new Action(ShowUI));
		PhotoModeManager.OnPhotoModeEnabled -= HideUI;
		PhotoModeManager.OnPhotoModeDisabled -= ShowUI;
	}

	private void HideUI()
	{
		if (cHideAllRoutine != null)
		{
			StopCoroutine(cHideAllRoutine);
		}
		cHideAllRoutine = StartCoroutine(HideAllRoutine());
	}

	private IEnumerator HideAllRoutine()
	{
		while (true)
		{
			SpriteRenderers = GetComponentsInChildren<Renderer>();
			Renderer[] spriteRenderers = SpriteRenderers;
			foreach (Renderer renderer in spriteRenderers)
			{
				if (renderer.enabled)
				{
					renderer.enabled = false;
					if (!HiddenRenderers.Contains(renderer))
					{
						HiddenRenderers.Add(renderer);
					}
				}
			}
			yield return null;
		}
	}

	private void ShowUI()
	{
		if (cHideAllRoutine != null)
		{
			StopCoroutine(cHideAllRoutine);
		}
		foreach (Renderer hiddenRenderer in HiddenRenderers)
		{
			hiddenRenderer.enabled = true;
		}
		HiddenRenderers.Clear();
	}
}
