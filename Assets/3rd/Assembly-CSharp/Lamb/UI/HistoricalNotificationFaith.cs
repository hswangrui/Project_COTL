using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class HistoricalNotificationFaith : HistoricalNotificationBase<FinalizedFaithNotification>
	{
		[SerializeField]
		private Image _faithIcon;

		[SerializeField]
		private TextMeshProUGUI _faithDeltaText;

		[SerializeField]
		private GameObject _followerSpineContainer;

		[SerializeField]
		private SkeletonGraphic _followerSpine;

		[Header("Icons")]
		[SerializeField]
		private Sprite faithDoubleUp;

		[SerializeField]
		private Sprite faithUp;

		[SerializeField]
		private Sprite faithDown;

		[SerializeField]
		private Sprite faithDoubleDown;

		protected override void ConfigureImpl(FinalizedFaithNotification finalizedNotification)
		{
			float faithDelta = finalizedNotification.FaithDelta;
			if (faithDelta <= -10f)
			{
				_faithIcon.sprite = faithDoubleDown;
				_faithDeltaText.text = faithDelta.ToString().Bold().Colour(StaticColors.RedColor);
			}
			else if (faithDelta < 0f)
			{
				_faithIcon.sprite = faithDown;
				_faithDeltaText.text = faithDelta.ToString().Bold().Colour(StaticColors.RedColor);
			}
			else if (faithDelta >= 10f)
			{
				_faithIcon.sprite = faithDoubleUp;
				_faithDeltaText.text = faithDelta.ToString().Bold().Colour(StaticColors.GreenColor);
			}
			else if (faithDelta > 0f)
			{
				_faithIcon.sprite = faithUp;
				_faithDeltaText.text = faithDelta.ToString().Bold().Colour(StaticColors.GreenColor);
			}
			else
			{
				_faithIcon.gameObject.SetActive(false);
				_faithDeltaText.gameObject.SetActive(false);
			}
			_followerSpineContainer.SetActive(finalizedNotification.followerInfoSnapshot != null);
			if (_followerSpineContainer.activeSelf)
			{
				if (finalizedNotification.followerInfoSnapshot != null)
				{
					_followerSpine.ConfigureFollowerSkin(finalizedNotification.followerInfoSnapshot);
				}
				if (faithDelta > 0f)
				{
					_followerSpine.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(NotificationFollower.Animation.Normal), true);
				}
				else
				{
					_followerSpine.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(NotificationFollower.Animation.Sad), true);
				}
			}
		}

		protected override string GetLocalizedDescription(FinalizedFaithNotification finalizedNotification)
		{
			if (finalizedNotification.followerInfoSnapshot != null)
			{
				return string.Format(LocalizationManager.GetTranslation(finalizedNotification.LocKey), finalizedNotification.followerInfoSnapshot.Name);
			}
			return base.GetLocalizedDescription(finalizedNotification);
		}
	}
}
