using System.Collections;
using UnityEngine;

public class HUDManager : BaseMonoBehaviour
{
	public static HUDManager Instance;

	private RectTransform rectTransform;

	private Vector3 Offscreen = new Vector3(0f, 350f);

	public static bool isHiding;

	public RectTransform TopLeftUI;

	private void OnEnable()
	{
		Instance = this;
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static void Hide(bool Snap)
	{
		if (Instance != null)
		{
			Instance.hide(Snap);
		}
	}

	public void hide(bool Snap)
	{
		isHiding = true;
		StopAllCoroutines();
		if (Snap)
		{
			rectTransform.localPosition = Offscreen;
		}
		else
		{
			StartCoroutine(DoHide());
		}
	}

	private IEnumerator DoHide()
	{
		Vector3 Speed = new Vector3(0f, -15f);
		while (rectTransform.localPosition.y < Offscreen.y)
		{
			Speed += new Vector3(0f, 2f);
			rectTransform.localPosition += Speed;
			yield return null;
		}
		rectTransform.localPosition = Offscreen;
	}

	public static void Show(bool Snap)
	{
		if (Instance != null)
		{
			Instance.show(Snap);
		}
	}

	public void show(bool Snap)
	{
		StopAllCoroutines();
		if (Snap)
		{
			isHiding = false;
			rectTransform.localPosition = Vector3.zero;
		}
		else
		{
			StartCoroutine(DoShow());
		}
	}

	private IEnumerator DoShow()
	{
		new Vector3(0f, 15f);
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.unscaledDeltaTime);
			if (!(num < 1f))
			{
				break;
			}
			rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, Vector3.zero, 15f * Time.unscaledDeltaTime);
			yield return null;
		}
		rectTransform.localPosition = Vector3.zero;
		isHiding = false;
	}

	public void TopLeftUISetOffset(Vector3 Offset, float Duration)
	{
		StartCoroutine(TopLeftUIAddOffsetRoutine(Offset, Duration));
	}

	private IEnumerator TopLeftUIAddOffsetRoutine(Vector3 Offset, float Duration)
	{
		float Progress = 0f;
		Vector3 StartPosition = TopLeftUI.localPosition;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (num < Duration)
			{
				TopLeftUI.localPosition = Vector3.Lerp(StartPosition, Offset, Mathf.SmoothStep(0f, 1f, Progress / Duration));
				yield return null;
				continue;
			}
			break;
		}
	}
}
