using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FrogBossIntro : BossIntro
{
	[SerializeField]
	private EnemyFrogBoss frogBoss;

	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		BossSpine.AnimationState.SetAnimation(0, "transform", false);
		LetterBox.Instance.HideSkipPrompt();
		GameManager.GetInstance().OnConversationNext(CameraTarget, 12f);
		yield return new WaitForSeconds(2.1f);
		GameManager.GetInstance().OnConversationNext(CameraTarget, 16f);
		CameraManager.instance.ShakeCameraForDuration(2f, 2.5f, 1f);
		BossSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(3f);
		GameManager.GetInstance().OnConversationEnd();
		frogBoss.BeginPhase1();
		if (!DataManager.Instance.BossesEncountered.Contains(FollowerLocation.Dungeon1_2))
		{
			DataManager.Instance.BossesEncountered.Add(FollowerLocation.Dungeon1_2);
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}
}
