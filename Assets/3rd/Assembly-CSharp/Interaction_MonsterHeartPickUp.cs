using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_MonsterHeartPickUp : Interaction
{
	private string LabelName;

	public float Delay = 0.5f;

	public SpriteRenderer Shadow;

	public SpriteRenderer Image;

	private Vector3 BookTargetPosition;

	private float Timer;

	private void Start()
	{
		UpdateLocalisation();
		StartCoroutine(HeartBeart());
	}

	private IEnumerator HeartBeart()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_beat", base.gameObject);
			base.gameObject.transform.DOPunchScale(new Vector3(0.3f, -0.3f), 0.5f);
			yield return null;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		StopCoroutine(HeartBeart());
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		LabelName = ScriptLocalization.Interactions.MonsterHeart;
	}

	protected override void Update()
	{
		Delay -= Time.deltaTime;
		base.Update();
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

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		base.gameObject.GetComponent<PickUp>().enabled = false;
		StartCoroutine(PlayerPickUpBook());
	}

	private IEnumerator PlayerPickUpBook()
	{
		AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_sequence", base.gameObject);
		Timer = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		Shadow.enabled = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		while ((Timer += Time.deltaTime) < 0.5f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		base.transform.position = BookTargetPosition;
		yield return new WaitForSeconds(1f);
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		Inventory.AddItem(22, 1);
		Object.Destroy(base.gameObject);
	}
}
