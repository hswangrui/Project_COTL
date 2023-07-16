using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_CrownShrineSherpa : Interaction
{
	[SerializeField]
	private GameObject ShrineNormal;

	[SerializeField]
	private GameObject ShrineHighlight;

	[SerializeField]
	private GameObject LightingOverride;

	[SerializeField]
	private GameObject FloorMarkings;

	[SerializeField]
	private GameObject TargetPosition;

	public override void GetLabel()
	{
		if (Interactable)
		{
			base.Label = ScriptLocalization.Interactions.Pray;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		PlayerFarming.Instance.GoToAndStop(TargetPosition.transform.position, base.gameObject, false, false, delegate
		{
			StartCoroutine(InteractIE());
		});
	}

	private void Start()
	{
		ShrineNormal.SetActive(true);
		ShrineHighlight.SetActive(false);
		FloorMarkings.SetActive(false);
		LightingOverride.SetActive(false);
		ActivateDistance = 2f;
	}

	private IEnumerator InteractIE()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
		PlayerFarming.Instance.CustomAnimation("pray", false);
		yield return new WaitForSeconds(0.5f);
		LightingOverride.SetActive(true);
		yield return new WaitForSeconds(1f);
		ShrineNormal.SetActive(false);
		ShrineHighlight.SetActive(true);
		FloorMarkings.SetActive(true);
		base.gameObject.transform.DOShakePosition(1f, new Vector3(0.25f, 0f, 0f));
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 1f);
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "Interactions/CrownShrine/Convo"));
		list[0].Offset = new Vector3(0f, 3f, 0f);
		MMConversation.Play(new ConversationObject(list, null, null), false, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", PlayerFarming.Instance.transform.position);
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		LightingOverride.SetActive(false);
		DataManager.Instance.PrayedAtCrownShrine = true;
		Interactable = false;
		GameManager.GetInstance().OnConversationEnd();
	}
}
