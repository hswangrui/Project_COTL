using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using Rewired;
using RewiredConsts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityUnlock : BaseMonoBehaviour
{
	public enum Ability
	{
		GrappleHook,
		CultLevel1,
		CultLevel2,
		MenticideMushroom,
		FishingRod,
		Arrows,
		LevelUpFollower,
		HeavyAttack,
		PyreResurrect,
		Abilities_Heart_I,
		Heal,
		UpgradeHeal,
		Resurrection,
		Eat,
		TeleportHome
	}

	[Serializable]
	public class AbilityInfo
	{
		public Ability AbilityType;

		[TermsPopup("")]
		public string Title;

		[TermsPopup("")]
		public string Description;

		[TermsPopup("")]
		public string Explanation;

		public Sprite Sprite;

		public bool ShowControlPrompt;

		[ActionIdProperty(typeof(RewiredConsts.Action))]
		public int Action = 9;

		public Vector3 ImageRotation = new Vector3(0f, 0f, -35.26f);

		public bool ShowCrownAbilityUnlockedText = true;

		private void AutoSet()
		{
			Description = Title.Replace("Title", "Description");
			Explanation = Title.Replace("Title", "Explanation");
		}
	}

	public static UIAbilityUnlock Instance;

	public CanvasGroup canvasGroup;

	public List<Image> MainImage;

	public TextMeshProUGUI Title;

	public TextMeshProUGUI Description;

	public TextMeshProUGUI Explanation;

	public TextMeshProUGUI CrownAbilityUnlocked;

	public MMControlPrompt ControlPrompt;

	public List<AbilityInfo> Abilities = new List<AbilityInfo>();

	public static void Play(Ability Ability)
	{
		Instance = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Ability Unlock"), GameObject.FindWithTag("Canvas").transform) as GameObject).GetComponent<UIAbilityUnlock>();
		Instance.Show(Ability);
	}

	public void Show(Ability Ability)
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAction0;
		if (HUD_Manager.Instance != null)
		{
			HUD_Manager.Instance.Hide(false, 0);
		}
		AbilityInfo abilityInfo = GetAbilityInfo(Ability);
		foreach (Image item in MainImage)
		{
			if (abilityInfo.Sprite == null)
			{
				item.enabled = false;
				continue;
			}
			item.enabled = true;
			item.sprite = abilityInfo.Sprite;
			item.rectTransform.eulerAngles = abilityInfo.ImageRotation;
		}
		Title.text = LocalizationManager.Sources[0].GetTranslation(abilityInfo.Title);
		Description.text = LocalizationManager.Sources[0].GetTranslation(abilityInfo.Description);
		Explanation.text = LocalizationManager.Sources[0].GetTranslation(abilityInfo.Explanation);
		CrownAbilityUnlocked.enabled = abilityInfo.ShowCrownAbilityUnlockedText;
		if (abilityInfo.ShowControlPrompt)
		{
			ControlPrompt.gameObject.SetActive(true);
			ControlPrompt.Action = abilityInfo.Action;
			ControlPrompt.ForceUpdate();
		}
		else
		{
			ControlPrompt.gameObject.SetActive(false);
		}
		StartCoroutine(ScreenRoutine());
	}

	private IEnumerator ScreenRoutine()
	{
		Time.timeScale = 0f;
		float Progress = 0f;
		float Duration2 = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (num < Duration2)
			{
				canvasGroup.alpha = Mathf.Lerp(0f, 1f, Progress / Duration2);
				yield return null;
				continue;
			}
			break;
		}
		while (!InputManager.UI.GetAcceptButtonUp() && !InputManager.UI.GetCancelButtonUp())
		{
			yield return null;
		}
		Time.timeScale = 1f;
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
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, Progress / Duration2);
			yield return null;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		HUD_Manager.Instance.Show();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private AbilityInfo GetAbilityInfo(Ability Ability)
	{
		foreach (AbilityInfo ability in Abilities)
		{
			if (ability.AbilityType == Ability)
			{
				return ability;
			}
		}
		return null;
	}
}
