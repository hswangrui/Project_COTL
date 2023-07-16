using UnityEngine;

public class FollowerFootsteps : MonoBehaviour
{
	public void PlayFootstep()
	{
		AudioManager.Instance.PlayFootstep(base.transform.position);
	}
}
