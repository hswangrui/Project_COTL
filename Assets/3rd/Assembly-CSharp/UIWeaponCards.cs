using System;
using System.Collections;
using System.Collections.Generic;
using MMTools.UIInventory;
using TMPro;
using UnityEngine;

public class UIWeaponCards : UIInventoryController
{
	public GameObject ControlPromptsObject;

	public TextMeshProUGUI RedealTokenCount;

	private GameObject Player;

	private Canvas canvas;

	private Action PlayerCallBack;

	private Action ResetCallBack;

	public RectTransform CardStartingPosition;

	public List<UIWeaponCard> WeaponCards = new List<UIWeaponCard>();

	public float Offset = 100f;

	public static void Play(Action CallBack, Action PlayerCallBack, Action ResetCallBack, GameObject Player)
	{
		UIWeaponCards component = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("MMUIInventory/UI Weapon Cards"), GlobalCanvasReference.Instance).GetComponent<UIWeaponCards>();
		component.Callback = CallBack;
		component.PlayerCallBack = PlayerCallBack;
		component.ResetCallBack = ResetCallBack;
		component.Player = Player;
		component.PauseTimeSpeed = 1f;
		component.canvas = GlobalCanvasReference.CanvasInstance;
	}

	public override void StartUIInventoryController()
	{
		ControlPromptsObject.SetActive(false);
		StartCoroutine(DealCards());
	}

	private IEnumerator DealCards()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(0.5f);
		Action playerCallBack = PlayerCallBack;
		if (playerCallBack != null)
		{
			playerCallBack();
		}
		yield return new WaitForSeconds(1.3f);
		bool Redeal = false;
		if (DataManager.Instance.PLAYER_REDEAL_TOKEN > 0)
		{
			ControlPromptsObject.SetActive(true);
			RedealTokenCount.text = "x" + DataManager.Instance.PLAYER_REDEAL_TOKEN;
		}
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 0.5f)
			{
				if (InputManager.UI.GetAcceptButtonDown())
				{
					Redeal = true;
					Timer = 1f;
				}
				yield return null;
				continue;
			}
			break;
		}
		while (!Redeal && !InputManager.UI.GetAcceptButtonDown() && !InputManager.UI.GetCancelButtonDown() && Mathf.Abs(InputManager.UI.GetHorizontalAxis()) < 0.3f && Mathf.Abs(InputManager.UI.GetVerticalAxis()) < 0.3f)
		{
			yield return null;
		}
		Close();
		if (DataManager.Instance.PLAYER_REDEAL_TOKEN > 0 && (InputManager.UI.GetAcceptButtonDown() || Redeal))
		{
			Action resetCallBack = ResetCallBack;
			if (resetCallBack != null)
			{
				resetCallBack();
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void UpdateUIInventoryController()
	{
	}

	public override void Close()
	{
		base.Close();
	}
}
