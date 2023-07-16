using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class SpineExtensions
{
	public static IEnumerator YieldForAnimation(this SkeletonAnimation skeletonAnimation, string animation)
	{
		skeletonAnimation.AnimationState.SetAnimation(0, animation, false);
		while (!skeletonAnimation.AnimationState.GetCurrent(0).IsComplete)
		{
			yield return null;
		}
	}

	public static IEnumerator YieldForAnimation(this SkeletonGraphic skeletonGraphic, string animation)
	{
		skeletonGraphic.SetAnimation(animation);
		while (!skeletonGraphic.AnimationState.GetCurrent(0).IsComplete)
		{
			yield return null;
		}
	}

	public static void SetAnimation(this SkeletonGraphic skeletonGraphic, string animation)
	{
		skeletonGraphic.SetAnimation(animation, false);
	}

	public static void SetAnimation(this SkeletonGraphic skeletonGraphic, string animation, bool loop)
	{
		skeletonGraphic.AnimationState.SetAnimation(0, animation, loop);
	}

	public static void ConfigureFollower(this SkeletonGraphic skeletonGraphic, FollowerInfo followerInfo)
	{
		skeletonGraphic.ConfigureFollowerSkin(followerInfo);
		skeletonGraphic.ConfigureFollowerOutfit(followerInfo);
		skeletonGraphic.ConfigureEmotion(followerInfo);
	}

	public static void ConfigureFollower(this SkeletonGraphic skeletonGraphic, FollowerInfoSnapshot followerInfoSnapshot)
	{
		skeletonGraphic.ConfigureFollowerSkin(followerInfoSnapshot);
		skeletonGraphic.ConfigureEmotion(followerInfoSnapshot);
	}

	public static void ConfigureFollowerSkin(this SkeletonGraphic skeletonGraphic, FollowerInfo followerInfo)
	{
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(followerInfo.SkinName);
		skeletonGraphic.ConfigureFollowerSkin(colourData, followerInfo.SkinVariation, followerInfo.SkinColour);
	}

	public static void ConfigureFollowerSkin(this SkeletonGraphic skeletonGraphic, FollowerInfoSnapshot followerInfoSnapshot)
	{
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(followerInfoSnapshot.SkinName);
		skeletonGraphic.ConfigureFollowerSkin(colourData, followerInfoSnapshot.SkinVariation, followerInfoSnapshot.SkinColour);
	}

	public static void ConfigureFollowerSkin(this SkeletonGraphic skeletonGraphic, WorshipperData.SkinAndData skinAndData, int variant = 0, int colour = 0)
	{
		skeletonGraphic.Skeleton.SetSkin(skinAndData.Skin[Mathf.Min(variant, skinAndData.Skin.Count - 1)].Skin);
		foreach (WorshipperData.SlotAndColor slotAndColour in skinAndData.SlotAndColours[Mathf.Min(colour, skinAndData.SlotAndColours.Count - 1)].SlotAndColours)
		{
			Slot slot = skeletonGraphic.Skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
	}

	public static void ConfigureFollowerOutfit(this SkeletonGraphic skeletonGraphic, FollowerInfo followerInfo)
	{
		HatType hat = HatType.None;
		if (followerInfo.TaxEnforcer)
		{
			hat = HatType.TaxEnforcer;
		}
		else if (followerInfo.FaithEnforcer)
		{
			hat = HatType.FaithEnforcer;
		}
		new FollowerOutfit(followerInfo).SetOutfit(skeletonGraphic, followerInfo.Outfit, followerInfo.Necklace, false, hat);
	}

	public static void ConfigureEmotion(this SkeletonGraphic skeletonGraphic, FollowerInfo followerInfo)
	{
		skeletonGraphic.ConfigureEmotion(followerInfo.HasThought(Thought.Dissenter), FollowerBrainStats.BrainWashed, followerInfo.Illness, 1200f, CultFaithManager.CurrentFaith);
	}

	public static void ConfigureEmotion(this SkeletonGraphic skeletonGraphic, FollowerInfoSnapshot followerInfoSnapshot)
	{
		skeletonGraphic.ConfigureEmotion(followerInfoSnapshot.Dissenter, followerInfoSnapshot.Brainwashed, followerInfoSnapshot.Illness, followerInfoSnapshot.Rest, followerInfoSnapshot.CultFaith);
	}

	public static void ConfigureEmotion(this SkeletonGraphic skeletonGraphic, bool dissenter, bool brainwashed, float illness, float rest, float faith)
	{
		if (dissenter)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-dissenter", true);
			return;
		}
		if (brainwashed)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-enlightened", true);
			return;
		}
		if (illness > 0f)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-sick", true);
			return;
		}
		if (rest <= 20f)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-tired", true);
			return;
		}
		if (faith >= 0f && faith <= 25f)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-angry", true);
		}
		if (faith > 25f && faith <= 40f)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-unhappy", true);
		}
		if (faith > 40f && faith <= 80f)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-normal", true);
		}
		if (faith > 75f)
		{
			skeletonGraphic.SetFaceAnimation("Emotions/emotion-happy", true);
		}
	}

	public static void SetFaceAnimation(this SkeletonGraphic skeletonGraphic, string animation, bool loop)
	{
		skeletonGraphic.AnimationState.SetAnimation(1, animation, loop);
	}

	public static void ConfigurePrison(this SkeletonGraphic skeletonGraphic, FollowerInfo followerInfo, StructuresData structureData, bool ui = false)
	{
		string text = "Prison/stocks";
		if (followerInfo.IsStarving)
		{
			text = string.Join("-", text, "hungry");
		}
		if (structureData.Type == StructureBrain.TYPES.PRISON && DataManager.Instance.Followers_Dead_IDs.Contains(followerInfo.ID))
		{
			text = ((!structureData.Rotten) ? string.Join("-", text, "dead") : string.Join("-", text, "rotten"));
		}
		if (ui)
		{
			text = string.Join("-", text, "ui");
		}
		skeletonGraphic.SetAnimation(text);
	}
}
