using UnityEngine;

public class CanvasConstants : BaseMonoBehaviour
{
	public CanvasGroup _canvasGroup;

	public GameObject BuildSiteProgressUIPrefab;

	public static CanvasConstants instance;

	private void OnEnable()
	{
		instance = this;
	}

	private void OnDisable()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	public CanvasConstants()
	{
		instance = this;
	}

	public void Hide()
	{
		_canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		_canvasGroup.alpha = 1f;
	}
}
