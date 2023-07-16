using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollBarController : BaseMonoBehaviour
{
	public Scrollbar scrollbar;

	public CanvasGroup canvasGroup;

	public float SelectionDelay;

	private bool released;

	public string HorizontalNavAxisName = "Horizontal";

	public string VerticalNavAxisName = "Vertical";

	private float ButtonDownDelay;

	public float ScrollBarSpeedMultiplier = 1f;

	private void Start()
	{
		EventSystem.current.SetSelectedGameObject(scrollbar.gameObject);
	}

	private void Update()
	{
		if (!canvasGroup.interactable || canvasGroup.alpha == 0f || EventSystem.current.currentSelectedGameObject != scrollbar.gameObject)
		{
			return;
		}
		SelectionDelay -= Time.unscaledDeltaTime;
		if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) <= 0.2f && Mathf.Abs(InputManager.UI.GetVerticalAxis()) <= 0.2f)
		{
			SelectionDelay = 0f;
		}
		if (SelectionDelay < 0f)
		{
			if (InputManager.UI.GetVerticalAxis() < -0.35f && scrollbar.value > 0f)
			{
				scrollbar.value -= 0.033f * ScrollBarSpeedMultiplier;
			}
			if (InputManager.UI.GetVerticalAxis() > 0.35f && scrollbar.value < 1f)
			{
				scrollbar.value += 0.033f * ScrollBarSpeedMultiplier;
			}
		}
	}
}
