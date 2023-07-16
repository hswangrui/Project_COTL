using System.Collections;
using UnityEngine.Events;

public class GuardianBossIntro : BossIntro
{
	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
		yield return null;
	}
}
