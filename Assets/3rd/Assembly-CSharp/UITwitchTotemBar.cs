using UnityEngine;

public class UITwitchTotemBar : MonoBehaviour
{
	//[SerializeField]
	//private SpriteXPBar bar;

	//private int currentContributions;

	//private void OnEnable()
	//{
	//	if (TwitchAuthentication.IsAuthenticated && !TwitchTotem.Deactivated)
	//	{
	//		bar.Show(true);
	//	}
	//	else
	//	{
	//		base.gameObject.SetActive(false);
	//	}
	//}

	//private void Awake()
	//{
	//	TwitchTotem.TotemUpdated += TwitchTotem_TotemUpdated;
	//	TwitchAuthentication.OnAuthenticated += TwitchAuthentication_OnAuthenticated;
	//	TwitchAuthentication.OnLoggedOut += TwitchAuthentication_OnLoggedOut;
	//	TwitchTotem_TotemUpdated(TwitchTotem.CurrentContributions);
	//}

	//private void OnDestroy()
	//{
	//	TwitchTotem.TotemUpdated -= TwitchTotem_TotemUpdated;
	//	TwitchAuthentication.OnAuthenticated -= TwitchAuthentication_OnAuthenticated;
	//	TwitchAuthentication.OnLoggedOut -= TwitchAuthentication_OnLoggedOut;
	//}

	//private void TwitchAuthentication_OnAuthenticated()
	//{
	//	if (TwitchTotem.Deactivated)
	//	{
	//		base.gameObject.SetActive(false);
	//		return;
	//	}
	//	base.gameObject.SetActive(true);
	//	bar.Show(true);
	//	TwitchTotem_TotemUpdated(TwitchTotem.CurrentContributions);
	//}

	//private void TwitchAuthentication_OnLoggedOut()
	//{
	//	base.gameObject.SetActive(false);
	//	bar.Hide(true);
	//}

	//private void TwitchTotem_TotemUpdated(int contributions)
	//{
	//	contributions -= 10 * TwitchTotem.TwitchTotemsCompleted;
	//	if (contributions != currentContributions)
	//	{
	//		float value = (float)contributions / 10f;
	//		bar.UpdateBar(Mathf.Clamp01(value));
	//	}
	//	currentContributions = contributions;
	//}
}
