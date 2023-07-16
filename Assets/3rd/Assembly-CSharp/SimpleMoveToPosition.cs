using UnityEngine;

public class SimpleMoveToPosition : MonoBehaviour
{
	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private float delay;

	[SerializeField]
	private bool onEnable;

	private bool triggered;

	private void OnEnable()
	{
		if (!triggered)
		{
			MoveToPosition(delay);
		}
	}

	public void MoveToPosition()
	{
		MoveToPosition(0f);
	}

	public void MoveToPosition(float delay)
	{
		GameManager.GetInstance().WaitForSeconds(delay, delegate
		{
			if (base.gameObject.activeInHierarchy)
			{
				triggered = true;
				PlayerFarming.Instance.GoToAndStop(position);
			}
		});
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(position, 0.5f);
	}
}
