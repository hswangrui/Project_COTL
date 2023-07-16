using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class DeathCatBossIntro : BossIntro
{
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	protected string introAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	protected string idleAnimation;

	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraTarget, 10f);
		yield return new WaitForSeconds(0.5f);
		BossSpine.AnimationState.SetAnimation(0, introAnimation, false);
		yield return new WaitForSeconds(1.8f);
		CameraManager.instance.ShakeCameraForDuration(0.2f, 0.3f, 1.5f);
		float ShakeDuration = 2.3f;
		while (true)
		{
			float num;
			ShakeDuration = (num = ShakeDuration - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			CameraManager.shakeCamera((1f - ShakeDuration / 2.3f) * 0.2f, Random.Range(0, 360));
			yield return null;
		}
		BossSpine.AnimationState.SetAnimation(0, idleAnimation, true);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}
}
