using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class SpineRandomAnimationPickerUI : BaseMonoBehaviour
{
	public List<SpineAnimationsUI> spineAnims = new List<SpineAnimationsUI>();

	public bool randomTimeScale = true;

	public SkeletonGraphic Spine;

	public float minTimeScale = 0.8f;

	public float maxTimeScale = 1.2f;

	private void Start()
	{
		pickRandomAnimation();
	}

	private void OnEnable()
	{
	}

	public void pickRandomAnimation()
	{
		for (int i = 0; i < spineAnims.Count; i++)
		{
			Spine = base.gameObject.GetComponent<SkeletonGraphic>();
			int index = Random.Range(0, spineAnims.Count);
			if (spineAnims[index].TriggeredAnimation != null)
			{
				Spine.AnimationState.SetAnimation(0, spineAnims[index].TriggeredAnimation, true);
			}
			if (randomTimeScale)
			{
				float timeScale = Random.Range(0.8f, 1.2f);
				spineAnims[i].Spine.timeScale = timeScale;
			}
		}
	}
}
