using UnityEngine;
using UnityEngine.Events;

public class TriggerCallback : BaseMonoBehaviour
{
	public UnityEvent Callback;

	private bool Activated;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Activated && collision.gameObject.CompareTag("Player"))
		{
			Activated = true;
			UnityEvent callback = Callback;
			if (callback != null)
			{
				callback.Invoke();
			}
		}
	}

	public void DisableTrigger()
	{
		base.gameObject.GetComponent<Collider2D>().enabled = false;
	}
}
