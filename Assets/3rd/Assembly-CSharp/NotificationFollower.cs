using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class NotificationFollower : NotificationBase
{
	public enum Animation
	{
		Angry,
		Happy,
		Normal,
		Sad,
		Sick,
		Tired,
		Unhappy,
		VeryAngry,
		Dead,
		Dissenting
	}

	[SerializeField]
	private SkeletonGraphic _spine;

	private NotificationCentre.NotificationType _type;

	private FollowerInfo _followerInfo;

	protected override float _onScreenDuration
	{
		get
		{
			return 6f;
		}
	}

	protected override float _showHideDuration
	{
		get
		{
			return 0.4f;
		}
	}

	public void Configure(NotificationCentre.NotificationType type, FollowerInfo followerInfo, Animation followerAnimation, Flair flair = Flair.None)
	{
		_type = type;
		_followerInfo = followerInfo;
		Configure(flair);
		if (_followerInfo != null)
		{
			_spine.ConfigureFollowerSkin(_followerInfo);
			_spine.AnimationState.SetAnimation(0, GetAnimation(followerAnimation), true);
		}
	}

	protected override void Localize()
	{
		if ((bool)_description && _followerInfo != null)
		{
			_description.text = string.Format(LocalizationManager.GetTranslation(NotificationCentre.GetLocKey(_type)), _followerInfo.Name);
		}
	}

	public static string GetAnimation(Animation followerAnimation)
	{
		switch (followerAnimation)
		{
		case Animation.Angry:
			return "Avatars/avatar-angry";
		case Animation.Happy:
			return "Avatars/avatar-happy";
		case Animation.Normal:
			return "Avatars/avatar-normal";
		case Animation.Sad:
			return "Avatars/avatar-sad";
		case Animation.Sick:
			return "Avatars/avatar-sick";
		case Animation.Tired:
			return "Avatars/avatar-tired";
		case Animation.Unhappy:
			return "Avatars/avatar-unhappy";
		case Animation.VeryAngry:
			return "Avatars/avatar-veryangry";
		case Animation.Dead:
			return "Avatars/avatar-dead";
		case Animation.Dissenting:
			return "Avatars/avatar-dissenter2";
		default:
			return "";
		}
	}
}
