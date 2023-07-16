using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class WormBossIntro : BossIntro
{
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	protected string introAnimation;

	[SerializeField]
	private EnemyWormBoss wormBoss;

	private void Start()
	{
	}

	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		BossSpine.AnimationState.SetAnimation(0, introAnimation, false);
		LetterBox.Instance.HideSkipPrompt();
		if (!LetterBox.IsPlaying)
		{
			GameManager.GetInstance().OnConversationNew();
		}
		GameManager.GetInstance().OnConversationNext(CameraTarget, 15f);
		yield return new WaitForSeconds(1.25f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationNext(CameraTarget, 20f);
		CameraManager.instance.ShakeCameraForDuration(2f, 2.5f, 1f);
		yield return new WaitForSeconds(3f);
		GameManager.GetInstance().OnConversationEnd();
		wormBoss.BeginPhase1();
		if (!DataManager.Instance.BossesEncountered.Contains(FollowerLocation.Dungeon1_1))
		{
			DataManager.Instance.BossesEncountered.Add(FollowerLocation.Dungeon1_1);
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}
}
