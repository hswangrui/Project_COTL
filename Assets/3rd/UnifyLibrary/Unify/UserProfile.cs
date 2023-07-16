using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unify
{
	public class UserProfile : MonoBehaviour
	{
		public GameObject overaly;

		public GameObject profileName;

		public GameObject profileScore;

		public GameObject profileImage;

		private Text nameText;

		private Text scoreText;

		public void Start()
		{
			nameText = profileName.GetComponent<Text>();
			scoreText = profileScore.GetComponent<Text>();
			UserHelper.OnPlayerUserChanged = (UserHelper.PlayerUserChangedDelegate)Delegate.Combine(UserHelper.OnPlayerUserChanged, new UserHelper.PlayerUserChangedDelegate(OnPlayerUserChanged));
		}

		public void OnDestroy()
		{
			UserHelper.OnPlayerUserChanged = (UserHelper.PlayerUserChangedDelegate)Delegate.Remove(UserHelper.OnPlayerUserChanged, new UserHelper.PlayerUserChangedDelegate(OnPlayerUserChanged));
		}

		public void OnPlayerUserChanged(int playerNo, User was, User user)
		{
			if (playerNo != 0)
			{
				return;
			}
			if (user != null)
			{
				nameText.text = user.nickName;
				scoreText.text = user.score.ToString();
				overaly.SetActive(value: true);
				UserHelper.Instance.GetUserPicture(user, 512, delegate(Texture2D texture)
				{
					if (texture != null)
					{
						Logger.Log("USERPROFILE: profile texture: " + texture.width + "x" + texture.height);
						profileImage.GetComponent<RawImage>().texture = texture;
					}
					else
					{
						Logger.Log("USERPROFILE: on profile picture available");
					}
				});
			}
			else
			{
				overaly.SetActive(value: false);
			}
		}
	}
}
