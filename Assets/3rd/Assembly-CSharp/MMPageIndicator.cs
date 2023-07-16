using UnityEngine;

public class MMPageIndicator : MonoBehaviour
{
	[SerializeField]
	private GameObject _activeState;

	[SerializeField]
	private GameObject _inactiveState;

	public void Activate()
	{
		_activeState.SetActive(true);
		_inactiveState.SetActive(false);
	}

	public void Deactivate()
	{
		_activeState.SetActive(false);
		_inactiveState.SetActive(true);
	}
}
