using UnityEngine;

public class BugInputField : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	public void ShowSelected()
	{
		_animator.ResetAllTriggers();
		_animator.SetTrigger("Selected");
	}

	public void ShowNormal()
	{
		_animator.ResetAllTriggers();
		_animator.SetTrigger("Normal");
	}
}
