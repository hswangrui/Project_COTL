using System;
using System.Collections;
using I2.Loc;
using MMBiomeGeneration;
using UnityEngine;

public class Interaction_EntranceShrine : Interaction
{
	public GameObject ParentContainer;

	public DevotionCounterOverlay devotionCounterOverlay;

	public GameObject ReceiveSoulPosition;

	public Health health;

	public SpriteXPBar XPBar;

	private string sString;

	private int SoulCount = 20;

	private int SoulMax = 20;

	public GameObject[] Dummys;

	private GameObject Player;

	private bool Activating;

	private float Delay;

	private float Distance;

	public float DistanceToTriggerDeposits = 5f;

	private void Start()
	{
		if (DungeonSandboxManager.Active)
		{
			GameManager.GetInstance().StartCoroutine(TimedDelay(0.5f, delegate
			{
				RoomLockController.RoomCompleted();
			}));
			UnityEngine.Object.Destroy(XPBar.gameObject);
			UnityEngine.Object.Destroy(this);
		}
		UpdateLocalisation();
		ContinuouslyHold = true;
		BiomeGenerator.OnBiomeChangeRoom += OnChangeRoom;
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			SoulMax = (GameManager.Layer2 ? 40 : 7);
			break;
		case FollowerLocation.Dungeon1_2:
			SoulMax = (GameManager.Layer2 ? 45 : 14);
			break;
		case FollowerLocation.Dungeon1_3:
			SoulMax = (GameManager.Layer2 ? 50 : 20);
			break;
		case FollowerLocation.Dungeon1_4:
			SoulMax = (GameManager.Layer2 ? 55 : 30);
			break;
		}
		SoulCount = SoulMax;
		if ((GameManager.CurrentDungeonFloor == 1 && GameManager.InitialDungeonEnter) || !DataManager.Instance.HasBuiltShrine1)
		{
			ParentContainer.gameObject.SetActive(false);
		}
		else
		{
			HideDummys();
		}
	}

	public void HideDummys()
	{
		GameObject[] dummys = Dummys;
		foreach (GameObject gameObject in dummys)
		{
			if ((bool)gameObject)
			{
				gameObject.gameObject.SetActive(false);
			}
		}
	}

	public void Die()
	{
		for (int i = 0; (float)i < (float)SoulCount * 1.25f; i++)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
		}
		FaithAmmo.Reload();
		SoulCount = 0;
		UpdateBar();
	}

	public override void OnEnableInteraction()
	{
		ActivateDistance = 3f;
		base.OnEnableInteraction();
		UpdateBar();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		BiomeGenerator.OnBiomeChangeRoom -= OnChangeRoom;
	}

	private void OnChangeRoom()
	{
		BiomeGenerator.OnBiomeChangeRoom -= OnChangeRoom;
		if (GameManager.CurrentDungeonFloor > 1 || !GameManager.InitialDungeonEnter)
		{
			RoomLockController.RoomCompleted();
		}
		else
		{
			HUD_Manager.Instance.ShowTopRight();
		}
	}

	public override void GetLabel()
	{
		int num;
		object obj;
		if (SoulCount > 0)
		{
			if (!GameManager.HasUnlockAvailable())
			{
				num = (DataManager.Instance.DeathCatBeaten ? 1 : 0);
				if (num == 0)
				{
					obj = "<sprite name=\"icon_blackgold\">";
					goto IL_002f;
				}
			}
			else
			{
				num = 1;
			}
			obj = "<sprite name=\"icon_spirits\">";
			goto IL_002f;
		}
		base.Label = "";
		return;
		IL_002f:
		string text = (string)obj;
		if (num == 0)
		{
			sString = ScriptLocalization.Interactions.Collect;
		}
		base.Label = sString + " " + text + " x" + SoulCount + "/" + SoulMax;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			Activating = true;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.StealDevotion;
	}

	private void UpdateBar()
	{
		if (!(XPBar == null))
		{
			float value = Mathf.Clamp((float)SoulCount / (float)SoulMax, 0f, 1f);
			XPBar.UpdateBar(value);
		}
	}

	private new void Update()
	{
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		GetLabel();
		Distance = Vector3.Distance(base.transform.position, Player.transform.position);
		if (Activating && (SoulCount <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Distance > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			SoulCount--;
			Delay = 0.1f;
			UpdateBar();
		}
	}

	private IEnumerator TimedDelay(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	private void GivePlayerSoul()
	{
		PlayerFarming.Instance.GetSoul(1);
	}
}
