using Lamb.UI;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace src.UI.Menus
{
	public class PrisonInfoCard : UIInfoCardBase<FollowerInfo>
	{
		[Header("Prison Info Card")]
		[SerializeField]
		private TextMeshProUGUI _followerNameText;

		[SerializeField]
		private SkeletonGraphic _followerSpine;

		[SerializeField]
		private RectTransform _redOutline;

		private FollowerInfo _followerInfo;

		public SkeletonGraphic FollowerSpine
		{
			get
			{
				return _followerSpine;
			}
		}

		public RectTransform RedOutline
		{
			get
			{
				return _redOutline;
			}
		}

		public override void Configure(FollowerInfo config)
		{
			if (_followerInfo != config)
			{
				_followerInfo = config;
				_followerNameText.text = config.Name;
				_followerSpine.ConfigureFollower(config);
			}
		}
	}
}
