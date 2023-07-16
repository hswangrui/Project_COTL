using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_DonationBox : Interaction
{
	public static Interaction_DonationBox Instance;

	public GameObject Level1;

	public GameObject Level1Full;

	public GameObject Level2;

	public GameObject Level2Full;

	private string sCollectCoins;

	private Vector3 PunchScale = new Vector3(0.1f, 0.1f, 0.1f);

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		UpdateGameObjects();
		Instance = this;
		UpdateLocalisation();
	}

	private void UpdateGameObjects()
	{
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_IV))
		{
			if (DataManager.Instance.TempleDevotionBoxCoinCount > 0)
			{
				Level2.SetActive(false);
				Level2Full.SetActive(true);
			}
			else
			{
				Level2.SetActive(true);
				Level2Full.SetActive(false);
			}
			Level1.SetActive(false);
			Level1Full.SetActive(false);
		}
		else if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_III))
		{
			if (DataManager.Instance.TempleDevotionBoxCoinCount > 0)
			{
				Level1.SetActive(false);
				Level1Full.SetActive(true);
			}
			else
			{
				Level1.SetActive(true);
				Level1Full.SetActive(false);
			}
			Level2.SetActive(false);
			Level2Full.SetActive(false);
		}
		else
		{
			Level1.SetActive(false);
			Level1Full.SetActive(false);
			Level2.SetActive(false);
			Level2Full.SetActive(false);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sCollectCoins = ScriptLocalization.Interactions.TakeItem;
	}

	public override void GetLabel()
	{
		if (!Level1.activeSelf && !Level2.activeSelf && !Level1Full.activeSelf && !Level2Full.activeSelf)
		{
			base.Label = "";
		}
		else if (DataManager.Instance.TempleDevotionBoxCoinCount > 0)
		{
			base.Label = sCollectCoins + " <sprite name=\"icon_blackgold\"> x" + DataManager.Instance.TempleDevotionBoxCoinCount;
		}
		else
		{
			base.Label = "";
		}
	}

	public void DepositCoin()
	{
		DataManager.Instance.TempleDevotionBoxCoinCount++;
		base.gameObject.transform.localScale = Vector3.one;
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOPunchScale(PunchScale, 1f, 5, 0.5f);
		base.HasChanged = true;
		UpdateGameObjects();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StartCoroutine(GiveResourcesRoutine());
	}

	private IEnumerator GiveResourcesRoutine()
	{
		base.gameObject.transform.localScale = Vector3.one;
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOPunchScale(PunchScale, 1f, 5, 0.5f);
		int max = Mathf.Min(DataManager.Instance.TempleDevotionBoxCoinCount, 5);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= max)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			ResourceCustomTarget.Create(state.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSecondsRealtime(0.1f);
		}
		Inventory.AddItem(20, DataManager.Instance.TempleDevotionBoxCoinCount);
		base.gameObject.transform.localScale = Vector3.one;
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOPunchScale(PunchScale, 1f, 5, 0.5f);
		DataManager.Instance.TempleDevotionBoxCoinCount = 0;
		UpdateGameObjects();
		base.gameObject.AddComponent<SpriteRenderer>();
	}
}
