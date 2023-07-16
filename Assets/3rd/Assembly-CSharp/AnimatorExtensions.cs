using System.Collections;
using UnityEngine;

public static class AnimatorExtensions
{
	public static IEnumerator YieldForAnimation(this Animator animator, string stateName)
	{
		animator.Play(stateName);
		yield return null;
		float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
		while (true)
		{
			float num;
			animationDuration = (num = animationDuration - Time.unscaledDeltaTime);
			if (num > 0f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public static void ResetAllTriggers(this Animator animator)
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Trigger)
			{
				animator.ResetTrigger(animatorControllerParameter.name);
			}
		}
	}
}
