using System;
using Lamb.UI;
using UnityEngine;
using UnityEngine.Events;

public class UnlockMapLocation : BaseMonoBehaviour
{
	public FollowerLocation Location;

	public bool ReReveal;

	public UnityEvent Callback;

	public void Play()
	{
		if (!ReReveal && !DataManager.Instance.DiscoverLocation(Location))
		{
			return;
		}
		UIWorldMapMenuController uIWorldMapMenuController = MonoSingleton<UIManager>.Instance.ShowWorldMap();
		uIWorldMapMenuController.Show(Location, ReReveal);
		uIWorldMapMenuController.OnHidden = (Action)Delegate.Combine(uIWorldMapMenuController.OnHidden, (Action)delegate
		{
			Debug.Log("AAAA " + Callback.GetPersistentEventCount());
			UnityEvent callback = Callback;
			if (callback != null)
			{
				callback.Invoke();
			}
		});
	}
}
