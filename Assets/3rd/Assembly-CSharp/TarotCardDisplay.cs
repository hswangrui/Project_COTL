using UnityEngine;

public class TarotCardDisplay : MonoBehaviour
{
	private UITarotDisplay tarotDisplay;

	private Interaction_BuyItem buyItem;

	private void Awake()
	{
		buyItem = GetComponent<Interaction_BuyItem>();
	}

	private void Update()
	{
		if (PlayerFarming.Instance != null && Interactor.CurrentInteraction == buyItem && !PlayerFarming.Instance.GoToAndStopping && !LetterBox.IsPlaying && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.CustomAnimation && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.InActive)
		{
			if (tarotDisplay == null)
			{
				AudioManager.Instance.PlayOneShot("event:/ui/open_menu", PlayerFarming.Instance.transform.position);
				tarotDisplay = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/UI Tarot Pickup"), GameObject.FindWithTag("Canvas").transform).GetComponent<UITarotDisplay>();
				tarotDisplay.Play(buyItem.itemForSale.Card, base.gameObject);
			}
		}
		else if (tarotDisplay != null)
		{
			Object.Destroy(tarotDisplay.gameObject);
			tarotDisplay = null;
		}
	}

	private void OnDestroy()
	{
		if (tarotDisplay != null)
		{
			Object.Destroy(tarotDisplay.gameObject);
			tarotDisplay = null;
		}
	}
}
