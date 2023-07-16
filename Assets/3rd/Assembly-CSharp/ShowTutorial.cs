using System;
using Lamb.UI;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;
using UnityEngine.Events;

public class ShowTutorial : BaseMonoBehaviour
{
	public TutorialTopic TutorialTopic;

	public bool ForcePlay;

	public UnityEvent Callback;

	public void Play()
	{
		Debug.Log("PLAY!");
		Debug.Log("DataManager.Instance.TryRevealTutorialTopic(TutorialTopic): " + DataManager.Instance.TryRevealTutorialTopic(TutorialTopic));
		if (!DataManager.Instance.TryRevealTutorialTopic(TutorialTopic) && !ForcePlay)
		{
			return;
		}
		UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic);
		uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
		{
			UnityEvent callback = Callback;
			if (callback != null)
			{
				callback.Invoke();
			}
		});
	}
}
