using UnityEngine;

public class AnimatorStartOnRandomFrame : BaseMonoBehaviour
{
	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		animator.Play(currentAnimationName(), 0, Random.value);
		animator.cullingMode = AnimatorCullingMode.CullCompletely;
	}

	private string currentAnimationName()
	{
		string result = "";
		AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationClip.name))
			{
				result = animationClip.name.ToString();
			}
		}
		return result;
	}
}
