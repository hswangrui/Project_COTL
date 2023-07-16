using UnityEngine;
using UnityEngine.UI;

public class NotificationHelpHinder : NotificationTwitch
{
	//public static NotificationHelpHinder Instance;

	//[SerializeField]
	//private Image bar;

	//protected override float _onScreenDuration
	//{
	//	get
	//	{
	//		return TwitchHelpHinder.VotingPhaseDuration + TwitchHelpHinder.ChoicePhaseDuration + 3f;
	//	}
	//}

	//private void Awake()
	//{
	//	Instance = this;
	//}

	//protected override void OnDestroy()
	//{
	//	base.OnDestroy();
	//	Instance = null;
	//}

	//private void Update()
	//{
	//	if (TwitchHelpHinder.Active)
	//	{
	//		float num = TwitchHelpHinder.Timer / (TwitchHelpHinder.VotingPhaseDuration + TwitchHelpHinder.ChoicePhaseDuration);
	//		UpdateBar(1f - num);
	//		if (bar.fillAmount <= 0f)
	//		{
	//			CloseNotification();
	//		}
	//	}
	//}

	//private void UpdateBar(float norm)
	//{
	//	bar.fillAmount = norm;
	//}

	//public static void CloseNotification()
	//{
	//	if ((bool)Instance)
	//	{
	//		Instance.Hide();
	//		Instance = null;
	//	}
	//}
}
