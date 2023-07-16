using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using UnityEngine;

public class Interaction_DivineCrystal : Interaction
{
	public SpriteRenderer Image;

	[SerializeField]
	public ParticleSystem ps_PickUp;

	public static Interaction_DivineCrystal Instance;

	private float Delay = 0.5f;

	private string LabelName;

	private Vector3 BookTargetPosition;

	private float Timer;

	private EventInstance loopedSound;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		LabelName = ScriptLocalization.Interactions.PickUp + " " + ScriptLocalization.Inventory.GOD_TEAR;
	}

	public override void GetLabel()
	{
		if (Delay < 0f)
		{
			base.Label = LabelName;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Instance = this;
		StartCoroutine(DelayDoTween());
	}

	private IEnumerator DelayDoTween()
	{
		yield return new WaitForSeconds(0.5f);
		base.gameObject.GetComponent<PickUp>().enabled = false;
		Image.gameObject.transform.DOLocalMoveZ(-0.33f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetRelative(true);
	}

	protected override void Update()
	{
		base.Update();
		Delay -= Time.deltaTime;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		base.gameObject.GetComponent<PickUp>().enabled = false;
		Image.gameObject.transform.DOKill();
		ps_PickUp.Play();
		StartCoroutine(PlayerPickUpBook());
		if (!DungeonSandboxManager.Active)
		{
			IncrementGodTears();
		}
	}

	private void IncrementGodTears()
	{
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_1)
		{
			DataManager.Instance.Dungeon1GodTears++;
		}
		else if (PlayerFarming.Location == FollowerLocation.Dungeon1_2)
		{
			DataManager.Instance.Dungeon2GodTears++;
		}
		else if (PlayerFarming.Location == FollowerLocation.Dungeon1_3)
		{
			DataManager.Instance.Dungeon3GodTears++;
		}
		else if (PlayerFarming.Location == FollowerLocation.Dungeon1_4)
		{
			DataManager.Instance.Dungeon4GodTears++;
		}
	}

	private IEnumerator PlayerPickUpBook()
	{
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.transform.position);
		Timer = 0f;
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 5f);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		Image.transform.DOShakeScale(2.5f, 0.2f);
		while ((Timer += Time.deltaTime) < 2f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		base.transform.position = BookTargetPosition;
		Inventory.AddItem(119, 1);
		yield return new WaitForSeconds(0.5f);
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}
}
