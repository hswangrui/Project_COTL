using UnityEngine;

public class MoonGate : BaseMonoBehaviour
{
	private void Start()
	{
		BaseTimer.Instance.OnBaseTimeComplete += OnBaseTimeComplete;
	}

	private void OnDisable()
	{
		BaseTimer.Instance.OnBaseTimeComplete -= OnBaseTimeComplete;
	}

	private void OnBaseTimeComplete()
	{
		Debug.Log("END OF DAY!");
		Object.Destroy(base.gameObject);
	}
}
