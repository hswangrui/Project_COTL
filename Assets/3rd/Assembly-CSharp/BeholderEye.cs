using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class BeholderEye : Interaction
{
	public ParticleSystem Particles;

	public SpriteRenderer Image;

	public static BeholderEye Instance;

	private float Delay = 0.5f;

	private string LabelName;

	private Vector3 BookTargetPosition;

	private float Timer;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		LabelName = ScriptLocalization.Inventory.BEHOLDER_EYE;
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
		Particles.Stop();
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
		StartCoroutine(PlayerPickUpBook());
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
		Particles.Play();
		while ((Timer += Time.deltaTime) < 2f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		base.transform.position = BookTargetPosition;
		Inventory.AddItem(101, 1);
		Particles.Stop();
		yield return new WaitForSeconds(0.5f);
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}
}
