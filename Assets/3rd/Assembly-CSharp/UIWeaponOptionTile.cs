using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIWeaponOptionTile : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public RectTransform Container;

	public Image Icon;

	public Image BG;

	public Image SelectedIcon;

	public WeaponUpgradeSystem.WeaponType WeaponType;

	private bool _Paused;

	public Selectable Selectable { get; private set; }

	public bool Paused
	{
		get
		{
			return _Paused;
		}
		set
		{
			Debug.Log("CHANGE VALUE! " + value);
			_Paused = value;
		}
	}

	private void Awake()
	{
		Selectable = GetComponent<Selectable>();
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
		while (Paused)
		{
			yield return null;
		}
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
		while (Paused)
		{
			yield return null;
		}
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
		Container.localScale = Vector3.one;
	}
}
