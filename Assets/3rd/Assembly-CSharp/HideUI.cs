using UnityEngine;

public class HideUI : BaseMonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		Canvas[] array = (Canvas[])Object.FindObjectsOfType(typeof(Canvas));
		foreach (Canvas canvas in array)
		{
			if (!(canvas.gameObject.name == "CanvasUnify"))
			{
				CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
				if (canvasGroup == null)
				{
					canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
				}
				if (canvas.gameObject.name != "Canvas - Temple Overlays" && canvas.gameObject.name != "Transition(Clone)")
				{
					canvasGroup.alpha = 0f;
				}
			}
		}
	}

	public void ShowUI()
	{
		Canvas[] array = (Canvas[])Object.FindObjectsOfType(typeof(Canvas));
		for (int i = 0; i < array.Length; i++)
		{
			CanvasGroup component = array[i].GetComponent<CanvasGroup>();
			if ((bool)component)
			{
				component.alpha = 1f;
			}
		}
		Object.Destroy(this);
	}
}
