using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_CoinGamble : Interaction
{
	[SerializeField]
	private GameObject cameraBone;

	[SerializeField]
	private Health[] goldSacks;

	private const int maxRolls = 3;

	private int rolls = 1;

	private float costMultiplier = 1f;

	private string resultText;

	public int goldSacksCacheLength;

	private int cost
	{
		get
		{
			return Mathf.RoundToInt(20f * costMultiplier);
		}
	}

	private void Start()
	{
		goldSacksCacheLength = goldSacks.Length;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		StartCoroutine(CheckGoldSacks());
	}

	private IEnumerator CheckGoldSacks()
	{
		while (true)
		{
			int num = 0;
			Health[] array = goldSacks;
			foreach (Health health in array)
			{
				if (health == null || health.HP <= 0f)
				{
					num++;
				}
			}
			if (goldSacksCacheLength != num)
			{
				Debug.Log("Shaking");
				base.transform.DOKill();
				if (num != goldSacks.Length)
				{
					base.transform.DOShakePosition(0.5f, Vector3.left * 0.1f);
				}
				else
				{
					base.transform.DOShakePosition(1f, Vector3.left * 0.5f);
				}
				goldSacksCacheLength = num;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(20) < cost)
		{
			return "<color=red>";
		}
		return "";
	}

	public override void GetLabel()
	{
		base.Label = (Interactable ? (ScriptLocalization.Interactions.MakeOffering + " " + CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, cost)) : ScriptLocalization.Interactions.Recharging);
	}

	public override void OnInteract(StateMachine state)
	{
		if (Inventory.GetItemQuantity(20) >= cost)
		{
			base.OnInteract(state);
			Interactable = false;
			StartCoroutine(GambleIE());
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator GambleIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(cameraBone, 8f);
		yield return new WaitForSeconds(0.5f);
		if (!DataManager.Instance.EncounteredGambleRoom)
		{
			DataManager.Instance.EncounteredGambleRoom = true;
			List<ConversationEntry> list = new List<ConversationEntry>();
			list.Add(new ConversationEntry(cameraBone, "Interactions/CoinGamble/Message/0"));
			list[0].Offset = new Vector3(0f, 5f, 0f);
			MMConversation.Play(new ConversationObject(list, null, null), false, false, false);
			MMConversation.mmConversation.SpeechBubble.ScreenOffset = 400f;
			yield return new WaitForEndOfFrame();
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
		}
		float increment = 1f / (float)cost;
		for (int i = 0; i < cost; i++)
		{
			Inventory.GetItemByType(20);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.gameObject.transform.position);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.gameObject.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			Inventory.ChangeItemQuantity(20, -1);
			yield return new WaitForSeconds(increment);
		}
		yield return StartCoroutine(Shake());
		yield return StartCoroutine(GiveReward());
		yield return new WaitForSeconds(1f);
		List<ConversationEntry> list2 = new List<ConversationEntry>();
		list2.Add(new ConversationEntry(cameraBone, resultText));
		list2[0].Offset = new Vector3(0f, 5f, 0f);
		MMConversation.Play(new ConversationObject(list2, null, null), false, false, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 400f;
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		if (rolls >= 3)
		{
			Interactable = false;
		}
		yield return new WaitForEndOfFrame();
		costMultiplier += 0.5f;
		rolls++;
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator GiveReward()
	{
		float num = Mathf.Clamp01(Random.Range(0f, 1f) * DataManager.Instance.GetLuckMultiplier());
		bool interactable = true;
		int num2 = 0;
		Health[] array = goldSacks;
		foreach (Health health in array)
		{
			if (health == null || health.HP <= 0f)
			{
				num2++;
			}
		}
		if ((float)num2 / (float)goldSacks.Length > 0.6f)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/gamble_lose", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Failure, false, false, GameManager.GetInstance());
			resultText = LocalizationManager.GetTranslation("Interactions/CoinGamble/0");
			interactable = false;
		}
		else if (num <= 0.4f)
		{
			yield return new WaitForSeconds(0.5f);
			int amount2 = Random.Range(5, 50);
			if (amount2 > 20)
			{
				AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", PlayerFarming.Instance.transform.position);
				MMVibrate.Haptic(MMVibrate.HapticTypes.Success, false, false, GameManager.GetInstance());
				BiomeConstants.Instance.EmitConfettiVFX(base.transform.position);
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/Stings/gamble_lose", PlayerFarming.Instance.transform.position);
				MMVibrate.Haptic(MMVibrate.HapticTypes.Failure, false, false, GameManager.GetInstance());
			}
			float increment = 1f / (float)amount2;
			string translation = LocalizationManager.GetTranslation("Interactions/CoinGamble/1");
			resultText = string.Format(translation, "<color=#FFD201>x" + amount2 + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.BLACK_GOLD));
			for (int k = 0; k < amount2; k++)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
				yield return new WaitForSeconds(increment);
			}
		}
		else if (num <= 0.6f)
		{
			int amount2 = Random.Range(7, 12);
			int k = Random.Range(7, 12);
			string translation2 = LocalizationManager.GetTranslation("Interactions/CoinGamble/1");
			resultText = string.Format(translation2, "<color=#FFD201>" + amount2 + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.LOG));
			resultText = resultText + " <color=#FFD201>" + k + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.STONE);
			yield return new WaitForSeconds(0.5f);
			AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success, false, false, GameManager.GetInstance());
			BiomeConstants.Instance.EmitConfettiVFX(base.transform.position);
			for (int m = 0; m < amount2; m++)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.LOG, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
			for (int n = 0; n < k; n++)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.STONE, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
		}
		else if ((double)num <= 0.9)
		{
			resultText = LocalizationManager.GetTranslation("Interactions/CoinGamble/2");
			yield return new WaitForSeconds(0.5f);
			AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success, false, false, GameManager.GetInstance());
			BiomeConstants.Instance.EmitConfettiVFX(base.transform.position);
			for (int num3 = 0; num3 < 15; num3++)
			{
				InventoryItem.ITEM_TYPE[] array2 = new InventoryItem.ITEM_TYPE[4]
				{
					InventoryItem.ITEM_TYPE.BERRY,
					InventoryItem.ITEM_TYPE.FISH,
					InventoryItem.ITEM_TYPE.MEAT,
					InventoryItem.ITEM_TYPE.PUMPKIN
				};
				InventoryItem.Spawn(array2[Random.Range(0, array2.Length)], 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
		}
		else if (num <= 0.95f && DataManager.CheckIfThereAreSkinsAvailable())
		{
			resultText = LocalizationManager.GetTranslation("Interactions/CoinGamble/4");
			yield return new WaitForSeconds(0.5f);
			AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success, false, false, GameManager.GetInstance());
			BiomeConstants.Instance.EmitConfettiVFX(base.transform.position);
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(3f, 270f);
		}
		else if (num <= 1f)
		{
			resultText = LocalizationManager.GetTranslation("Interactions/CoinGamble/5");
			yield return new WaitForSeconds(0.5f);
			AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success, false, false, GameManager.GetInstance());
			BiomeConstants.Instance.EmitConfettiVFX(base.transform.position);
			for (int k = 0; k < Random.Range(5, 7); k++)
			{
				InventoryItem.Spawn((Random.Range(0, 2) == 0) ? InventoryItem.ITEM_TYPE.BLUE_HEART : InventoryItem.ITEM_TYPE.HALF_BLUE_HEART, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
				yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
			}
		}
		Interactable = interactable;
	}

	private IEnumerator Shake()
	{
		AudioManager.Instance.PlayOneShot("event:/locations/light_house/fireplace_shake", base.transform.position);
		Vector3 pos = base.transform.position;
		float progress = 0f;
		float duration = 2f;
		while (progress < duration)
		{
			base.transform.position = pos + (Vector3)Random.insideUnitCircle * progress * 0.05f;
			progress = Mathf.Clamp(progress + Time.unscaledDeltaTime, 0f, duration);
			yield return null;
		}
		while (progress > 0f)
		{
			base.transform.position = pos + (Vector3)Random.insideUnitCircle * progress * 0.05f;
			progress = Mathf.Clamp(progress - Time.unscaledDeltaTime * 3f, 0f, duration);
			yield return null;
		}
		base.transform.position = pos;
	}
}
