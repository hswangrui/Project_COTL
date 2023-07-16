using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples
{
	public class SpineManualUpdater : BaseMonoBehaviour
	{
		[Range(0f, 0.125f)]
		[Tooltip("To specify a framerate, type 1/framerate in the inspector text box.")]
		public float timeBetweenFrames = 1f / 24f;

		public List<SkeletonAnimation> components;

		private float timeOfLastUpdate;

		private float accumulator;

		private bool doLateUpdate;

		private void OnValidate()
		{
			if (timeBetweenFrames < 0f)
			{
				timeBetweenFrames = 0f;
			}
		}

		private void OnEnable()
		{
			timeOfLastUpdate = Time.time;
		}

		private void Start()
		{
			int i = 0;
			for (int count = components.Count; i < count; i++)
			{
				SkeletonAnimation skeletonAnimation = components[i];
				skeletonAnimation.Initialize(false);
				skeletonAnimation.clearStateOnDisable = false;
				skeletonAnimation.enabled = false;
				skeletonAnimation.Update(0f);
				skeletonAnimation.LateUpdate();
			}
		}

		private void Update()
		{
			if (timeBetweenFrames <= 0f)
			{
				return;
			}
			accumulator += Time.deltaTime;
			bool flag = false;
			if (accumulator > timeBetweenFrames)
			{
				accumulator %= timeBetweenFrames;
				flag = true;
			}
			if (flag)
			{
				float time = Time.time;
				float deltaTime = time - timeOfLastUpdate;
				int i = 0;
				for (int count = components.Count; i < count; i++)
				{
					components[i].Update(deltaTime);
				}
				doLateUpdate = true;
				timeOfLastUpdate = time;
			}
		}

		private void LateUpdate()
		{
			if (doLateUpdate)
			{
				int i = 0;
				for (int count = components.Count; i < count; i++)
				{
					components[i].LateUpdate();
				}
				doLateUpdate = false;
			}
		}
	}
}
