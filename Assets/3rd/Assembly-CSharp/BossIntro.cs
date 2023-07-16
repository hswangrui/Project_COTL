using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class BossIntro : BaseMonoBehaviour
{
	public SkeletonAnimation BossSpine;

	public UnityEvent Callback;

	public GameObject CameraTarget;

	private void Start()
	{
		if (BossSpine != null)
		{
			//BossSpine.UpdateInterval = 1;
		}
	}

	public virtual IEnumerator PlayRoutine(bool skipped = false)
	{
		yield return null;
	}
}
