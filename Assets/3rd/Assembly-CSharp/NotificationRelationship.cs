using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class NotificationRelationship : NotificationBase
{
	[SerializeField]
	private SkeletonGraphic _spineA;

	[SerializeField]
	private SkeletonGraphic _spineB;

	private NotificationCentre.NotificationType _type;

	private FollowerInfo _followerInfoA;

	private FollowerInfo _followerInfoB;

	protected override float _onScreenDuration
	{
		get
		{
			return 3f;
		}
	}

	protected override float _showHideDuration
	{
		get
		{
			return 0.4f;
		}
	}

	public void Configure(NotificationCentre.NotificationType type, FollowerInfo followerInfoA, FollowerInfo followerInfoB, NotificationFollower.Animation followerAnimationA, NotificationFollower.Animation followerAnimationB, Flair flair = Flair.None)
	{
		_type = type;
		_followerInfoA = followerInfoA;
		_followerInfoB = followerInfoB;
		_spineA.ConfigureFollowerSkin(_followerInfoA);
		_spineA.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(followerAnimationA), true);
		_spineB.ConfigureFollowerSkin(_followerInfoB);
		_spineB.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(followerAnimationB), true);
		Configure(flair);
	}

	protected override void Localize()
	{
		_description.text = string.Format(LocalizationManager.GetTranslation(string.Format("Notifications/Relationship/{0}", _type)), _followerInfoA.Name, _followerInfoB.Name);
	}
}
