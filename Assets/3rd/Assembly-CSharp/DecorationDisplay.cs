using UnityEngine;

public class DecorationDisplay : MonoBehaviour
{
	private UIDecorationDisplay decorationDisplay;

	private Interaction_BuyItem buyItem;

	private void Awake()
	{
		buyItem = GetComponent<Interaction_BuyItem>();
	}

	private void Update()
	{
		if (PlayerFarming.Instance != null && Interactor.CurrentInteraction == buyItem && !PlayerFarming.Instance.GoToAndStopping && !LetterBox.IsPlaying && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.CustomAnimation && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.InActive)
		{
			if (decorationDisplay == null)
			{
				AudioManager.Instance.PlayOneShot("event:/ui/open_menu", PlayerFarming.Instance.transform.position);
				decorationDisplay = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/UI Decoration Pickup"), GameObject.FindWithTag("Canvas").transform).GetComponent<UIDecorationDisplay>();
				decorationDisplay.Play(buyItem.itemForSale.decorationToBuy, base.gameObject);
			}
		}
		else if (decorationDisplay != null)
		{
			Object.Destroy(decorationDisplay.gameObject);
			decorationDisplay = null;
		}
	}

	private void OnDestroy()
	{
		if (decorationDisplay != null)
		{
			Object.Destroy(decorationDisplay.gameObject);
			decorationDisplay = null;
		}
	}
}
