using Rewired;
using RewiredConsts;
using Unify.Input;
using UnityEngine.UI;

public class RewiredActivateButton : BaseMonoBehaviour
{
	public Button button;

	[ActionIdProperty(typeof(Action))]
	public int Action;

	private Rewired.Player player;

	private void Start()
	{
		player = RewiredInputManager.MainPlayer;
	}

	private void Update()
	{
		if (player.GetButtonDown(Action))
		{
			button.onClick.Invoke();
			base.enabled = false;
		}
	}
}
