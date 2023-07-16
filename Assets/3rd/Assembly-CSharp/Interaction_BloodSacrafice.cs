using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_BloodSacrafice : Interaction
{
	private int rewardIndex;

	public SpriteRenderer sprite;

	private void Awake()
	{
		if (PlayerFleeceManager.FleeceNoRedHeartsToUse())
		{
			sprite.color = Color.black;
			Interactable = false;
		}
	}

	public override void GetLabel()
	{
		if (!Interactable)
		{
			base.Label = ScriptLocalization.Interactions.Recharging;
		}
		else
		{
			base.Label = (((bool)PlayerFarming.Instance && PlayerFarming.Instance.health.HP > 0f) ? ScriptLocalization.Interactions.BloodSacrafice : "");
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		StartCoroutine(InteractIE());
	}

	private IEnumerator InteractIE()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
		PlayerFarming.Instance.CustomAnimation("pray", false);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < 10; i++)
		{
			SoulCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, Color.red, null);
			yield return new WaitForSeconds(0.05f);
		}
		PlayerFarming.Instance.health.HP -= 2f;
		yield return new WaitForSeconds(1f);
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "Interactions/BloodSacrafice/Message"));
		list[0].Offset = new Vector3(0f, 3f, 0f);
		MMConversation.Play(new ConversationObject(list, null, null), false, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_negative", PlayerFarming.Instance.transform.position);
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("float-up-spin", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("floating-land-spin", 0, false, 0f);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		StartCoroutine(GiveReward());
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", PlayerFarming.Instance.transform.position);
		yield return new WaitForSeconds(3.2f);
		MMConversation.Play(new ConversationObject(new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, "Interactions/BloodSacrafice/" + rewardIndex)
		}, null, null), false, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		yield return new WaitForEndOfFrame();
		sprite.DOColor(Color.black, 1f);
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		Interactable = false;
		GameManager.GetInstance().OnConversationEnd();
		HealthPlayer healthPlayer = PlayerFarming.Instance.health as HealthPlayer;
		healthPlayer.invincible = false;
		if (healthPlayer.HP + healthPlayer.BlueHearts + healthPlayer.SpiritHearts + healthPlayer.BlackHearts <= 0f)
		{
			PlayerFarming.Instance.health.DealDamage(1f, base.gameObject, base.transform.position, false, Health.AttackTypes.Melee, true);
		}
	}

	private IEnumerator GiveReward()
	{
		float num = Mathf.Clamp01(Random.Range(0f, 1f) * DataManager.Instance.GetLuckMultiplier());
		if (num <= 0.1f)
		{
			rewardIndex = 0;
		}
		if (num <= 0.3f)
		{
			rewardIndex = 1;
			yield return new WaitForSeconds(0.5f);
			DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL += 0.25f;
			BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "strength", "strength");
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
		}
		else if (num <= 0.6f)
		{
			rewardIndex = 2;
			yield return new WaitForSeconds(1f);
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "blue", "burst_big");
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
			yield return new WaitForSeconds(3.5f);
			int num2 = Random.Range(1, 4);
			PlayerFarming.Instance.health.BlueHearts += num2 * 2 * TrinketManager.GetHealthAmountMultiplier();
		}
		else if (num <= 0.9f)
		{
			rewardIndex = 3;
			yield return new WaitForSeconds(1f);
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "black", "burst_big");
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
			yield return new WaitForSeconds(3.5f);
			int num3 = Random.Range(1, 4);
			PlayerFarming.Instance.health.BlackHearts += num3 * 2;
		}
		else if (num <= 0.95f)
		{
			rewardIndex = 4;
			yield return new WaitForSeconds(1f);
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
			yield return new WaitForSeconds(3.5f);
			int num4 = Random.Range(1, 4);
			PlayerFarming.Instance.health.SpiritHearts += num4 * 2;
		}
		else if (num <= 1f)
		{
			rewardIndex = 5;
			yield return new WaitForSeconds(0.5f);
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.TRINKET_CARD, 1, base.transform.position + Vector3.down * 2f, 0f);
		}
	}
}
