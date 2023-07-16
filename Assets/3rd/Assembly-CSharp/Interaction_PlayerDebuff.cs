using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Interaction_PlayerDebuff : Interaction
{
	private ColorGrading colorGrading;

	private TarotCards.TarotCard removedCard;

	protected override void OnEnable()
	{
		base.OnEnable();
		BiomeConstants.Instance.ppv.profile.TryGetSettings<ColorGrading>(out colorGrading);
		StartCoroutine(DebuffIE());
	}

	private IEnumerator DebuffIE()
	{
		yield return new WaitForEndOfFrame();
		while (PlayerFarming.Instance == null || PlayerFarming.Instance.GoToAndStopping || LetterBox.IsPlaying)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		FadeRedIn();
		PlayerFarming.Instance.GoToAndStop(base.transform.position);
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/pentagram_platform/pentagram_platform_start");
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "float-up", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "floating", true, 0f);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/door/cross_disappear", base.gameObject);
		string arg = ScriptLocalization.NAMES_CultLeaders.Dungeon1;
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_2)
		{
			arg = ScriptLocalization.NAMES_CultLeaders.Dungeon2;
		}
		else if (PlayerFarming.Location == FollowerLocation.Dungeon1_3)
		{
			arg = ScriptLocalization.NAMES_CultLeaders.Dungeon3;
		}
		else if (PlayerFarming.Location == FollowerLocation.Dungeon1_4)
		{
			arg = ScriptLocalization.NAMES_CultLeaders.Dungeon4;
		}
		string termToSpeak = string.Format(LocalizationManager.GetTranslation("Interactions/NegativeRoom/Message"), arg);
		int num = GiveDebuff();
		string text = LocalizationManager.GetTranslation("Interactions/NegativeRoom/Debuff" + num);
		if (num == 2)
		{
			text = string.Format(text, TarotCards.LocalisedName(removedCard.CardType, 0));
		}
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, termToSpeak));
		list.Add(new ConversationEntry(base.gameObject, text));
		list[0].Offset = new Vector3(0f, 3f, 0f);
		list[1].Offset = new Vector3(0f, 3f, 0f);
		AudioManager.Instance.PlayOneShot("event:/pentagram_platform/pentagram_platform_curse");
		MMConversation.Play(new ConversationObject(list, null, null), false, false, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "floating-land", false);
		AudioManager.Instance.PlayOneShot("event:/pentagram_platform/pentagram_platform_end");
		FadeRedAway();
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
	}

	private int GiveDebuff()
	{
		float num = Random.Range(0f, 1f);
		HealthPlayer healthPlayer = PlayerFarming.Instance.health as HealthPlayer;
		if (num > 0.9f && healthPlayer.HP > 0f)
		{
			DataManager.Instance.RedHeartsTemporarilyRemoved += Mathf.RoundToInt(healthPlayer.totalHP);
			int num2 = (int)healthPlayer.HP;
			healthPlayer.totalHP = 0f;
			healthPlayer.BlueHearts += num2;
			return 1;
		}
		if (num > 0.7f)
		{
			DataManager.Instance.ProjectileMoveSpeedMultiplier += 0.5f;
			return 3;
		}
		if (num > 0.6f)
		{
			DataManager.Instance.EnemyModifiersChanceMultiplier += 1f;
			return 4;
		}
		if (num > 0.5f)
		{
			DataManager.Instance.EnemyHealthMultiplier += 0.5f;
			return 5;
		}
		if (num > 0.4f && DataManager.Instance.Followers_Demons_IDs.Count > 0)
		{
			DataManager.Instance.Followers_Demons_IDs.Clear();
			DataManager.Instance.Followers_Demons_Types.Clear();
			for (int num3 = Demon_Arrows.Demons.Count - 1; num3 >= 0; num3--)
			{
				Object.Destroy(Demon_Arrows.Demons[num3]);
			}
			return 6;
		}
		if (num > 0.3f)
		{
			Inventory.RemoveDungeonItemsFromInventory();
			return 7;
		}
		if (num > 0.2f)
		{
			DataManager.Instance.DodgeDistanceMultiplier -= 0.5f;
			return 8;
		}
		if (num > 0.1f)
		{
			DataManager.Instance.CurseFeverMultiplier += 1f;
			return 9;
		}
		if (num > 0f && (healthPlayer.BlueHearts > 0f || healthPlayer.SpiritHearts > 0f || healthPlayer.BlackHearts > 0f))
		{
			HealthPlayer.LoseAllSpecialHearts();
			return 10;
		}
		return GiveDebuff();
	}

	public void FadeRedAway()
	{
		DOTween.To(() => colorGrading.colorFilter.value, delegate(Color x)
		{
			colorGrading.colorFilter.value = x;
		}, Color.white, 1f);
	}

	private void FadeRedIn()
	{
		DOTween.To(() => colorGrading.colorFilter.value, delegate(Color x)
		{
			colorGrading.colorFilter.value = x;
		}, new Color(1f, 0.25f, 0.25f, 1f), 2f);
	}
}
