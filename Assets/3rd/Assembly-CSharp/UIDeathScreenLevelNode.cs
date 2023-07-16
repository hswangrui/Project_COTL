using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIDeathScreenLevelNode : BaseMonoBehaviour
{
	public enum ResultTypes
	{
		Completed,
		Killed,
		Unreached
	}

	public enum LevelNodeSkins
	{
		normal,
		boss,
		other
	}

	public SkeletonGraphic Spine;

	public Image icon;

	public void Play(float Delay, ResultTypes ResultType, LevelNodeSkins LevelNodeSkin, int index, bool endNode)
	{
		StartCoroutine(PlayRoutine(Delay, ResultType, LevelNodeSkin, index, endNode));
	}

	private IEnumerator PlayRoutine(float Delay, ResultTypes ResultType, LevelNodeSkins LevelNodeSkin, int index, bool endNode)
	{
		Spine.AnimationState.Event += HandleEvent;
		icon.DOFade(0.01f, 0f).SetUpdate(true);
		UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
		if (index >= 0 && index <= DataManager.Instance.FollowersRecruitedInNodes.Count - 1 && DataManager.Instance.FollowersRecruitedInNodes[index] > 0)
		{
			Spine.Skeleton.SetSkin(string.Concat(LevelNodeSkin, "-follower"));
		}
		else if (LevelNodeSkin != LevelNodeSkins.boss)
		{
			Spine.Skeleton.SetSkin(string.Concat(LevelNodeSkin, endNode ? "-end" : string.Empty));
		}
		else
		{
			Spine.Skeleton.SetSkin(LevelNodeSkin.ToString());
		}
		Spine.AnimationState.SetAnimation(0, "appear", false);
		switch (ResultType)
		{
		case ResultTypes.Completed:
			yield return new WaitForSecondsRealtime(Delay);
			if (LevelNodeSkin == LevelNodeSkins.boss)
			{
				Spine.AnimationState.SetAnimation(0, "fill-boss", false);
				UIManager.PlayAudio("event:/ui/level_node_beat_boss");
				MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact);
			}
			else
			{
				UIManager.PlayAudio("event:/ui/level_node_beat_level");
				Spine.AnimationState.SetAnimation(0, "fill", false);
				Spine.AnimationState.AddAnimation(0, "full", false, 0f);
				MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
			}
			break;
		case ResultTypes.Killed:
			yield return new WaitForSecondsRealtime(Delay);
			Spine.AnimationState.SetAnimation(0, "fail", false);
			Spine.AnimationState.AddAnimation(0, "failed", false, 0f);
			UIManager.PlayAudio("event:/ui/level_node_die");
			MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact);
			break;
		}
	}

	private void HandleEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "reveal")
		{
			icon.DOFade(1f, 0.33f).SetUpdate(true);
		}
	}

	private void OnDisable()
	{
		Spine.AnimationState.Event -= HandleEvent;
	}
}
