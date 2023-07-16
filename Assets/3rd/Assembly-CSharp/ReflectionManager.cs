using System;
using Spine.Unity;
using UnityEngine;

public class ReflectionManager : BaseMonoBehaviour
{
	public GameObject ReflectionObject;

	private void Awake()
	{
		SkeletonAnimation.OnCreated = (SkeletonAnimation.Created)Delegate.Combine(SkeletonAnimation.OnCreated, new SkeletonAnimation.Created(OnCreated));
	}

	private void OnCreated(SkeletonAnimation skeleton)
	{
		if (skeleton.ShowReflection && !skeleton.HasReflection)
		{
			UnityEngine.Object.Instantiate(ReflectionObject, skeleton.transform.position, Quaternion.identity, skeleton.transform).GetComponent<SpineMirror>().Init(skeleton);
			skeleton.HasReflection = true;
		}
	}
}
