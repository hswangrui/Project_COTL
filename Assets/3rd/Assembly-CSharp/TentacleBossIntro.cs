using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class TentacleBossIntro : BossIntro
{
	public SkeletonAnimation SpineHuman;

	public SpriteRenderer Shadow;

	public StateMachine State;

	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		yield return new WaitForEndOfFrame();
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}
}
