using UnityEngine;

[ExecuteInEditMode]
public class CustomVerticalLayout : MonoBehaviour
{
	[SerializeField]
	private Transform[] trackedObjects;

	[SerializeField]
	private float updateInterval = 0.5f;

	[SerializeField]
	private Vector2 positionChangeDelta = Vector2.zero;

	[SerializeField]
	private Vector2 initialPosition;

	private Vector2 changedPosition = Vector2.zero;

	private float updateTimer;

	private bool shouldUpdate = true;

	private void Awake()
	{
		changedPosition = initialPosition + positionChangeDelta;
		shouldUpdate = true;
	}

	private void Update()
	{
		if (trackedObjects == null || trackedObjects.Length == 0)
		{
			return;
		}
		updateTimer += Time.deltaTime;
		if (!(updateTimer >= updateInterval))
		{
			return;
		}
		updateTimer = 0f;
		for (int i = 0; i < trackedObjects.Length; i++)
		{
			if (trackedObjects[i] != null && trackedObjects[i].gameObject.activeInHierarchy)
			{
				if (shouldUpdate)
				{
					((RectTransform)base.transform).anchoredPosition = changedPosition;
					shouldUpdate = false;
				}
				return;
			}
		}
		shouldUpdate = true;
		((RectTransform)base.transform).anchoredPosition = initialPosition;
	}

	public void SetInitialPosition()
	{
		initialPosition = ((RectTransform)base.transform).anchoredPosition;
		changedPosition = initialPosition + positionChangeDelta;
	}
}
