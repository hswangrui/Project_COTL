using System;
using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;

public class UIFollowerInteraction : BaseMonoBehaviour
{
	public TextMeshProUGUI FollowerName;

	public TextMeshProUGUI FollowerRole;

	public TextMeshProUGUI FollowerStats;

	public TextMeshProUGUI FollowerTraits;

	public TextMeshProUGUI FollowerThoughts;

	public RectTransform Container;

	private Canvas canvas;

	public Action CallbackClose;

	public void Show(FollowerBrain Brain, Action CallbackClose)
	{
		Time.timeScale = 0f;
		float num = (Brain.Stats.Satiation + (75f - Brain.Stats.Starvation)) / 175f;
		Debug.Log("SatiationTotal " + num);
		FollowerName.text = "<sprite name=\"img_SwirleyLeft\">" + Brain.Info.Name + ((Brain.Info.XPLevel > 1) ? (ScriptLocalization.Interactions.Level + " " + Brain.Info.XPLevel.ToNumeral()) : "") + "<sprite name=\"img_SwirleyRight\">";
		FollowerStats.text = ((Brain.Stats.Reeducation > 0f) ? ("Disseter <b>" + Brain.Stats.Reeducation + "%</b> \n\n") : "") + "Faith " + ((Brain.Stats.Happiness > 25f) ? "<color=green><b>" : "<color=red><b>") + Brain.Stats.Happiness + "%</color> - Hunger " + ((Brain.Stats.Satiation > 0f) ? "<color=green><b>" : "<color=red><b>") + Mathf.Floor(num * 100f) / 1f + "%</color> - Tierdness " + ((Brain.Stats.Rest > 20f) ? "<color=green><b>" : "<color=red><b>") + Mathf.Floor(Brain.Stats.Rest * 1f) / 1f + "%</color>";
		FollowerTraits.text = "";
		foreach (FollowerTrait.TraitType trait in Brain.Info.Traits)
		{
			TextMeshProUGUI followerTraits = FollowerTraits;
			followerTraits.text = followerTraits.text + "<b>" + FollowerTrait.GetLocalizedTitle(trait) + "</b>- <i>" + FollowerTrait.GetLocalizedDescription(trait) + "</i> \n";
		}
		FollowerRole.text = "<b><color=yellow>" + FollowerRoleInfo.GetLocalizedName(Brain.Info.FollowerRole) + "</color></b>";
		FollowerThoughts.text = Brain.GetThoughtString(18f);
		this.CallbackClose = CallbackClose;
		canvas = GetComponentInParent<Canvas>();
		StopAllCoroutines();
		GameManager.GetInstance().CameraSetOffset(new Vector3(-1.4f, 0f, 0f));
		StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		float Progress = 0f;
		float Duration = 0.3f;
		Vector3 StartingPosition = new Vector3(-700f * canvas.scaleFactor, 0f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.localPosition = Vector3.Lerp(StartingPosition, Vector3.zero, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.localPosition = Vector3.zero;
		if (CallbackClose != null)
		{
			while (InputManager.UI.GetCancelButtonDown() || InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			while (!InputManager.UI.GetCancelButtonDown() && !InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			Action callbackClose = CallbackClose;
			if (callbackClose != null)
			{
				callbackClose();
			}
			Close();
		}
	}

	public void Close()
	{
		Time.timeScale = 1f;
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		StopAllCoroutines();
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		Vector3 StartingPosition = Container.localPosition;
		Vector3 TargetPosition = new Vector3(-700f * canvas.scaleFactor, 0f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.localPosition = Vector3.Lerp(StartingPosition, TargetPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.localPosition = Vector3.zero;
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
