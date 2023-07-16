using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class Interaction_KeyPiece : Interaction
{
	public static Interaction_KeyPiece Instance;

	public float Delay = 0.5f;

	private string LabelName;

	public ParticleSystem Particles;

	public SpriteRenderer Image;

	public SpriteRenderer Shadow;

	public List<Sprite> KeyImages = new List<Sprite>();

	public Action Callback;

	private Vector3 BookTargetPosition;

	private float Timer;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		LabelName = ScriptLocalization.Interactions.KeyPiece;
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

	protected override void Update()
	{
		base.Update();
		Delay -= Time.deltaTime;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Image.sprite = KeyImages[DataManager.Instance.CurrentKeyPieces % KeyImages.Count];
		Particles.Stop();
		Instance = this;
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
		StartCoroutine(PlayerPickUpBook());
	}

	private IEnumerator PlayerPickUpBook()
	{
		Timer = 0f;
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 5f);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		BookTargetPosition = state.transform.position + new Vector3(0f, 0.2f, -1.2f);
		Shadow.enabled = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		Particles.Play();
		AudioManager.Instance.PlayOneShot("event:/temple_key/fragment_pickup", base.gameObject);
		base.transform.DOMove(BookTargetPosition, 0.2f);
		yield return new WaitForSeconds(2.5f);
		Inventory.KeyPieces++;
		UIKeyScreenOverlayController uIKeyScreenOverlayController = MonoSingleton<UIManager>.Instance.ShowKeyScreen();
		uIKeyScreenOverlayController.OnHidden = (Action)Delegate.Combine(uIKeyScreenOverlayController.OnHidden, (Action)delegate
		{
			if (!DataManager.Instance.HadFirstTempleKey && Inventory.TempleKeys > 0 && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Fleeces))
			{
				UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Fleeces);
				uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
				{
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/UnlockFleece", Objectives.CustomQuestTypes.UnlockFleece));
					DataManager.Instance.HadFirstTempleKey = true;
				});
			}
		});
		Particles.Stop();
		yield return new WaitForSeconds(0.5f);
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
