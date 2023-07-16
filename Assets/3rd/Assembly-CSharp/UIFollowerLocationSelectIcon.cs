using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFollowerLocationSelectIcon : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public FollowerBrain Brain;

	public Image SelectedIcon;

	public RectTransform Container;

	public TextMeshProUGUI Name;

	public void Play(FollowerBrain Brain)
	{
		this.Brain = Brain;
		Name.text = Brain.Info.Name;
	}

	public void OnSelect(BaseEventData eventData)
	{
		SelectedIcon.enabled = true;
		StopAllCoroutines();
		StartCoroutine(Selected(Container.localScale.x, 1.3f));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		SelectedIcon.enabled = false;
		StopAllCoroutines();
		StartCoroutine(DeSelected());
	}

	private IEnumerator Selected(float Starting, float Target)
	{
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(Starting, Target, Progress / Duration);
			Container.localScale = Vector3.one * num2;
			yield return null;
		}
		Container.localScale = Vector3.one * Target;
	}

	private IEnumerator DeSelected()
	{
		float Progress = 0f;
		float Duration = 0.3f;
		float StartingScale = Container.localScale.x;
		float TargetScale = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(StartingScale, TargetScale, Progress / Duration);
			Container.localScale = Vector3.one * num2;
			yield return null;
		}
		Container.localScale = Vector3.one * TargetScale;
	}
}
