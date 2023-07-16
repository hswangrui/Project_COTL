using System.Collections;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_ChestBossRoom : Interaction
{
	public enum State
	{
		Hidden,
		Closed,
		Open
	}

	public bool StartRevealed;

	public InventoryItem.ITEM_TYPE ItemType = InventoryItem.ITEM_TYPE.LOG;

	public InventoryItemDisplay Item;

	public GameObject PlayerPosition;

	public SkeletonAnimation Spine;

	private string sLabel;

	private float Timer;

	private State MyState;

	public int MaxToGive = 12;

	public int MinToGive = 8;

	public static Interaction_Chest.ChestEvent OnChestRevealed;

	private InventoryItem.ITEM_TYPE Reward;

	private HealthPlayer healthPlayer;

	public static int RewardCount = -1;

	public static int RevealCount = -1;

	private bool Loop;

	private new void Update()
	{
		if (MyState == State.Closed)
		{
			Timer += Time.deltaTime;
		}
	}

	private void Start()
	{
		Item.gameObject.SetActive(false);
		UpdateLocalisation();
		if (MyState == State.Hidden)
		{
			Spine.gameObject.SetActive(false);
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, "closed", true);
		}
		if (StartRevealed)
		{
			Reveal();
		}
	}

	public void Reveal()
	{
		Spine.gameObject.SetActive(true);
		Spine.AnimationState.SetAnimation(0, "reveal", true);
		Spine.AnimationState.AddAnimation(0, "closed", true, 0f);
		MyState = State.Closed;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.OpenChest;
	}

	public override void GetLabel()
	{
		base.Label = ((MyState == State.Closed && Timer > 0.5f) ? sLabel : "");
	}

	public override void OnInteract(StateMachine state)
	{
		if (MyState == State.Closed)
		{
			base.OnInteract(state);
			StartCoroutine(InteractionRoutine());
		}
	}

	private IEnumerator InteractionRoutine()
	{
		Loop = false;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		PlayerFarming.Instance.GoToAndStop(PlayerPosition, base.gameObject);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		Spine.AnimationState.SetAnimation(0, "open", false);
		Spine.AnimationState.AddAnimation(0, "opened", true, 0f);
		MyState = State.Open;
		for (int i = 0; i < Random.Range(MinToGive, MaxToGive); i++)
		{
			InventoryItem.Spawn(ItemType, 1, base.transform.position);
			yield return new WaitForSeconds(0.1f);
		}
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position);
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		Interaction_Chest.ChestEvent onChestRevealed = OnChestRevealed;
		if (onChestRevealed != null)
		{
			onChestRevealed();
		}
	}
}
