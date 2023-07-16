using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Snail_Interaction : Interaction
{
	public SpriteRenderer Snail;

	public Sprite SnailUnlit;

	public Sprite SnailLit;

	public GameObject Lighting;

	public int ShrineNumber = -1;

	private void Start()
	{
		CheckSprites();
	}

	private void CheckSprites()
	{
		Snail.sprite = SnailUnlit;
		Lighting.SetActive(false);
		switch (ShrineNumber)
		{
		case 0:
			if (DataManager.Instance.ShellsGifted_0)
			{
				Snail.sprite = SnailLit;
				Lighting.SetActive(true);
			}
			break;
		case 1:
			if (DataManager.Instance.ShellsGifted_1)
			{
				Snail.sprite = SnailLit;
				Lighting.SetActive(true);
			}
			break;
		case 2:
			if (DataManager.Instance.ShellsGifted_2)
			{
				Snail.sprite = SnailLit;
				Lighting.SetActive(true);
			}
			break;
		case 3:
			if (DataManager.Instance.ShellsGifted_3)
			{
				Snail.sprite = SnailLit;
				Lighting.SetActive(true);
			}
			break;
		case 4:
			if (DataManager.Instance.ShellsGifted_4)
			{
				Snail.sprite = SnailLit;
				Lighting.SetActive(true);
			}
			break;
		}
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.SHELL) >= 1)
		{
			Interactable = true;
			return "<color=#f4ecd3>";
		}
		Interactable = false;
		return "<color=red>";
	}

	public override void GetLabel()
	{
		base.GetLabel();
		if (DataManager.GetFollowerSkinUnlocked("Snail"))
		{
			base.Label = "";
			Interactable = false;
			return;
		}
		switch (ShrineNumber)
		{
		case 0:
			if (DataManager.Instance.ShellsGifted_0)
			{
				Interactable = false;
				base.Label = "";
				return;
			}
			break;
		case 1:
			if (DataManager.Instance.ShellsGifted_1)
			{
				Interactable = false;
				base.Label = "";
				return;
			}
			break;
		case 2:
			if (DataManager.Instance.ShellsGifted_2)
			{
				Interactable = false;
				base.Label = "";
				return;
			}
			break;
		case 3:
			if (DataManager.Instance.ShellsGifted_3)
			{
				Interactable = false;
				base.Label = "";
				return;
			}
			break;
		case 4:
			if (DataManager.Instance.ShellsGifted_4)
			{
				Interactable = false;
				base.Label = "";
				return;
			}
			break;
		}
		base.Label = ScriptLocalization.Interactions.MakeOffering + " | " + GetAffordColor() + "<sprite name=\"icon_Shell\">" + Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.SHELL) + " / " + 1;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		StartCoroutine(PrayRoutine());
	}

	private IEnumerator PrayRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
		PlayerFarming.Instance.CustomAnimation("pray", false);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(Snail.gameObject, 10f);
		ResourceCustomTarget.Create(Snail.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.SHELL, delegate
		{
			AudioManager.Instance.PlayOneShot("event:/material/stone_impact", Snail.transform.position);
			Inventory.ChangeItemQuantity(117, -1);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
			Snail.transform.DOShakePosition(1f, new Vector3(0.3f, 0f, 0f));
			Snail.sprite = SnailLit;
			Lighting.SetActive(true);
		});
		yield return new WaitForSeconds(1f);
		switch (ShrineNumber)
		{
		case 0:
			DataManager.Instance.ShellsGifted_0 = true;
			break;
		case 1:
			DataManager.Instance.ShellsGifted_1 = true;
			break;
		case 2:
			DataManager.Instance.ShellsGifted_2 = true;
			break;
		case 3:
			DataManager.Instance.ShellsGifted_3 = true;
			break;
		case 4:
			DataManager.Instance.ShellsGifted_4 = true;
			break;
		}
		if (DataManager.Instance.ShellsGifted_0 && DataManager.Instance.ShellsGifted_1 && DataManager.Instance.ShellsGifted_2 && DataManager.Instance.ShellsGifted_3 && DataManager.Instance.ShellsGifted_4)
		{
			yield return new WaitForSeconds(0.5f);
			Inventory.SetItemQuantity(117, 0);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
			AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", base.transform.position);
			yield return new WaitForSeconds(1f);
			FollowerSkinCustomTarget.Create(Snail.transform.position, PlayerFarming.Instance.transform.position, 1f, "Snail", PickedUp);
		}
		else
		{
			PickedUp();
		}
	}

	private void PickedUp()
	{
		GameManager.GetInstance().OnConversationEnd();
		base.enabled = false;
	}
}
