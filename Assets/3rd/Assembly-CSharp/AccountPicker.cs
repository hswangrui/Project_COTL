using Unify;
using Unify.Input;
using UnityEngine;

public class AccountPicker : MonoBehaviour
{
	private void Start()
	{
		if (UnifyManager.platform == UnifyManager.Platform.GameCore)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (InputManager.UI.GetAccountPickerButtonDown())
		{
			User sessionOwner = SessionManager.GetSessionOwner();
			if (sessionOwner != null && sessionOwner.gamePadId != GamePad.None)
			{
				UserHelper.DisengageAllPlayers();
				UserHelper.Instance.ShowAccountPicker(sessionOwner.gamePadId);
			}
		}
	}
}
