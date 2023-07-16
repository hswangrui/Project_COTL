using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_CrownShrine : Interaction
{
	[SerializeField]
	private int cultLeaderStatue = -1;

	[SerializeField]
	private Material crownMaterial;

	[SerializeField]
	private Material defaultMaterial;

	private SpriteRenderer _spriteRenderer;

	private bool active;

	private void Start()
	{
		Interactable = false;
		_spriteRenderer = GetComponent<SpriteRenderer>();
		if (active)
		{
			_spriteRenderer.material = crownMaterial;
		}
		else
		{
			_spriteRenderer.material = defaultMaterial;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StartCoroutine(PrayRoutine());
	}

	private IEnumerator PrayRoutine()
	{
		active = !active;
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
		PlayerFarming.Instance.CustomAnimation("pray", false);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 10f);
		string termToSpeak = string.Format(active ? LocalizationManager.GetTranslation("Conversation_NPC/CrownShrine_Convo_0") : LocalizationManager.GetTranslation("Conversation_NPC/CrownShrine_Convo_1"), GetCultLeaderName());
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, termToSpeak));
		list[0].Offset = new Vector3(0f, 5f, 0f);
		MMConversation.Play(new ConversationObject(list, null, null), false, false, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 400f;
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		_spriteRenderer.material = (active ? crownMaterial : defaultMaterial);
		_spriteRenderer.gameObject.transform.DOShakePosition(1f, new Vector3(0.1f, 0f, 0f));
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
		AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", base.transform.position);
		GameManager.GetInstance().OnConversationEnd();
	}

	public override void GetLabel()
	{
		if (Interactable)
		{
			base.Label = ScriptLocalization.Interactions.Pray;
		}
	}

	private string GetCultLeaderName()
	{
		string text = "";
		switch (cultLeaderStatue)
		{
		case 1:
			text = ScriptLocalization.NAMES_CultLeaders.Dungeon1;
			break;
		case 2:
			text = ScriptLocalization.NAMES_CultLeaders.Dungeon2;
			break;
		case 3:
			text = ScriptLocalization.NAMES_CultLeaders.Dungeon3;
			break;
		case 4:
			text = ScriptLocalization.NAMES_CultLeaders.Dungeon4;
			break;
		}
		return "<color=#FFD201>" + text + "</color>";
	}
}
