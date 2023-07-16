using UnityEngine;

public class PlayAnimation : BaseMonoBehaviour
{
	public Animator animator;

	private void Start()
	{
	}

	public void Play()
	{
		animator.Play("KeyArt");
	}

	private void Update()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		Shader.SetGlobalFloat("_GlobalTimeUnscaled", fixedDeltaTime);
		Shader.SetGlobalFloat("_GlobalTimeUnscaled1", fixedDeltaTime);
	}
}
