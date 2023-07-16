using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationFaith : NotificationBase
{
	[SerializeField]
	private TextMeshProUGUI _faithDeltaText;

	[SerializeField]
	private Image _faithIcon;

	[SerializeField]
	private SkeletonGraphic _spine;

	[Header(" Icons")]
	[SerializeField]
	private Sprite faithDoubleUp;

	[SerializeField]
	private Sprite faithUp;

	[SerializeField]
	private Sprite faithDown;

	[SerializeField]
	private Sprite faithDoubleDown;

	private string _locKey;

	private FollowerInfo _followerInfo;

	private bool _includeName;

	private string[] _extraText;

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

	public void Configure(string locKey, float faithDelta, FollowerInfo followerInfo, bool includeName = true, Flair flair = Flair.None, params string[] args)
	{
		_locKey = locKey;
		_followerInfo = followerInfo;
		_includeName = includeName;
		_extraText = args;
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
		if (_followerInfo != null)
		{
			_spine.ConfigureFollowerSkin(_followerInfo);
			if (faithDelta > 0f)
			{
				_spine.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(NotificationFollower.Animation.Normal), true);
			}
			else
			{
				_spine.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(NotificationFollower.Animation.Sad), true);
			}
		}
		float obj = Mathf.Sign(faithDelta);
		if (!(-1f).Equals(obj))
		{
			if (1f.Equals(obj))
			{
				if (Mathf.Abs(faithDelta) > 14f)
				{
					AudioManager.Instance.PlayOneShot("event:/Stings/global_faith_up_big");
				}
				else
				{
					AudioManager.Instance.PlayOneShot("event:/Stings/global_faith_up");
				}
			}
		}
		else if (Mathf.Abs(faithDelta) > 14f)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/global_faith_down_big");
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/global_faith_down");
		}
		Configure(flair);
	}

	protected override void Localize()
	{
		string text = "";
		if (_extraText.Length != 0)
		{
			string translation = LocalizationManager.GetTranslation(_locKey);
			object[] extraText = _extraText;
			text = string.Format(translation, extraText);
		}
		else
		{
			text = LocalizationManager.GetTranslation(_locKey);
		}
		if (_followerInfo != null)
		{
			if (_includeName)
			{
				_description.text = string.Format(text, _followerInfo.Name);
			}
			else
			{
				_description.text = text;
			}
		}
		else if (text.Contains("{0}"))
		{
			_description.text = string.Format(text, ScriptLocalization.Inventory.FOLLOWERS);
		}
		else
		{
			_description.text = text;
			_spine.gameObject.SetActive(false);
		}
	}
}
