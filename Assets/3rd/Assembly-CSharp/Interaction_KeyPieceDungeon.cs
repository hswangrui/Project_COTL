using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using MMBiomeGeneration;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class Interaction_KeyPieceDungeon : Interaction
{
	public GameObject PlayerPosition;

	public ParticleSystem Particles;

	public SpriteRenderer Image;

	public SpriteRenderer Shadow;

	public List<Sprite> KeyImages = new List<Sprite>();

	public ParticleSystem GodRays;

	private bool Activated;

	private string sLabelName;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 2f;
		Image.sprite = KeyImages[DataManager.Instance.CurrentKeyPieces % KeyImages.Count];
		Particles.Stop();
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabelName = ScriptLocalization.Interactions.KeyPiece;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sLabelName);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Activated = true;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		PlayerFarming.Instance.GoToAndStop(PlayerPosition, base.gameObject, false, false, delegate
		{
			StartCoroutine(PlayerPickUpBook());
		});
	}

	private IEnumerator PlayerPickUpBook()
	{
		yield return new WaitForSeconds(0.2f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		Vector3 BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		Shadow.enabled = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		Particles.Play();
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir", base.gameObject);
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 1.5f))
			{
				break;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		base.transform.position = BookTargetPosition;
		Inventory.KeyPieces++;
		UIKeyScreenOverlayController uIKeyScreenOverlayController = MonoSingleton<UIManager>.Instance.ShowKeyScreen();
		uIKeyScreenOverlayController.OnHidden = (Action)Delegate.Combine(uIKeyScreenOverlayController.OnHidden, (Action)delegate
		{
			if (!DataManager.Instance.HadFirstTempleKey && Inventory.TempleKeys > 0 && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Fleeces))
			{
				UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Fleeces);
				uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
				{
					DataManager.Instance.HadFirstTempleKey = true;
				});
			}
		});
		DataManager.SaveKeyPieceFromLocation(BiomeGenerator.Instance.DungeonLocation, GameManager.CurrentDungeonLayer);
		Particles.Stop();
		yield return new WaitForSeconds(0.5f);
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		GodRays.Stop();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
