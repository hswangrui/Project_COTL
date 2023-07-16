using Spine.Unity;
using TMPro;
using UnityEngine;

public class HUD_ProblemUnlock : BaseMonoBehaviour
{
	public SkeletonGraphic Avatar;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI SubtitleText;

	public HUD_ProblemUnlockItem[] Items;

	public void Init(UnlockManager.UnlockType type, UnlockManager.UnlockNotificationData[] unlockTypes)
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		Avatar.AnimationState.SetAnimation(0, UnlockManager.GetUnlockAnimationName(type), true);
		TitleText.text = UnlockManager.GetUnlockTitle(type);
		SubtitleText.text = UnlockManager.GetUnlockSubtitle(type);
		int i;
		for (i = 0; i < unlockTypes.Length; i++)
		{
			HUD_ProblemUnlockItem obj = Items[i];
			obj.gameObject.SetActive(true);
			obj.Init(unlockTypes[i]);
		}
		for (; i < Items.Length; i++)
		{
			Items[i].gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (InputManager.UI.GetAcceptButtonDown())
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			Object.Destroy(base.gameObject);
		}
	}
}
