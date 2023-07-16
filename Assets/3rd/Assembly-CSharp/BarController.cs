using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
	public Image WhiteBar;

	public Image RedBar;

	public Image Minimum;

	public Image Maximum;

	public GameObject LockImage;

	public Vector3 Shake = new Vector3(0f, 10f);

	private float CurrentSize = 0.5f;

	private const float DoubleSizeIncreaseIconBorder = 0.1f;

	private const float DoubleSizeDecreaseIconBorder = -0.1f;

	private Sequence sequence;

	public List<AnimatedSizeChange> QueuedChanges = new List<AnimatedSizeChange>();

	public List<NotificationData> QueuedNotifications = new List<NotificationData>();

	[SerializeField]
	private TextMeshProUGUI faithIcon;

	[SerializeField]
	private Image faithImage;

	[SerializeField]
	private Sprite faithDoubleUp;

	[SerializeField]
	private Sprite faithUp;

	[SerializeField]
	private Sprite faithDown;

	[SerializeField]
	private Sprite faithDoubleDown;

	[SerializeField]
	private Image warningImage;

	public bool SetColor = true;

	public bool checkSizeDifference;

	public bool useFaithDown = true;

	private float _cacheBarSize;

	private bool playingSizeDiff;

	public bool IsPlaying;

	public bool UseQueuing = true;

	public float MinimumDelta;

	private void OnDisable()
	{
		if (sequence != null)
		{
			sequence.Kill();
		}
		if (WhiteBar != null)
		{
			WhiteBar.DOKill();
			WhiteBar.transform.DOKill();
		}
		if (RedBar != null)
		{
			RedBar.transform.DOKill();
			RedBar.DOKill();
		}
		base.transform.DOKill();
		if (base.transform.GetChild(0) != null)
		{
			base.transform.GetChild(0).DOKill();
		}
		sequence = null;
	}

	public void SetBarSizeForInfo(float WhiteSize, float RedBarSize, bool ShowLock)
	{
		RedBar.fillAmount = RedBarSize;
		if (SetColor)
		{
			RedBar.color = StaticColors.ColorForThreshold(RedBarSize);
		}
		WhiteBar.DOKill();
		WhiteBar.DOFillAmount(WhiteSize, 0.3f).SetEase(Ease.OutSine).SetUpdate(true);
		LockImage.SetActive(ShowLock);
	}

	public void SetBarSize(float Size, bool Animate, bool Force = false, NotificationData NotificationData = null)
	{
		Size = Mathf.Clamp01(Size);
		if (!Animate)
		{
			RedBar.fillAmount = Size;
			WhiteBar.fillAmount = Size;
			if (SetColor)
			{
				RedBar.color = StaticColors.ColorForThreshold(Size);
			}
			CurrentSize = Size;
		}
		if (CurrentSize == Size && CurrentSize != 1f && CurrentSize != 0f && !Force)
		{
			WhiteBar.fillAmount = Size;
			RedBar.fillAmount = Size;
			if (SetColor)
			{
				RedBar.color = StaticColors.ColorForThreshold(Size);
			}
			if (NotificationData != null && NotificationData.Notification != "")
			{
				NotificationCentre.Instance.PlayFaithNotification(NotificationData.Notification, NotificationData.DeltaDisplay, NotificationData.Flair, NotificationData.FollowerID, NotificationData.ExtraText);
			}
			return;
		}
		if (Animate)
		{
			float sizeFromLastChange = GetSizeFromLastChange();
			AnimatedSizeChange item = new AnimatedSizeChange(Size, sizeFromLastChange);
			QueuedChanges.Add(item);
			QueuedNotifications.Add(NotificationData);
		}
		if (warningImage != null)
		{
			warningImage.DOFade(0.75f - Size, 1f).SetEase(Ease.OutQuart).SetUpdate(true);
		}
	}

	public void Update()
	{
		if (QueuedChanges.Count > 0 && (!UseQueuing || (!HUD_Manager.IsTransitioning && !HUD_Manager.Instance.Hidden && !IsPlaying && Time.timeScale > 0f && !LetterBox.IsPlaying)))
		{
			NotificationData notificationData = QueuedNotifications[0];
			Play(QueuedChanges[0], (notificationData != null) ? QueuedNotifications[0].Notification : "");
			QueuedChanges.RemoveAt(0);
			if (notificationData != null && notificationData.Notification != "")
			{
				NotificationCentre.Instance.PlayFaithNotification(notificationData.Notification, notificationData.DeltaDisplay, notificationData.Flair, notificationData.FollowerID, notificationData.ExtraText);
			}
			QueuedNotifications.RemoveAt(0);
		}
	}

	private void OnEnable()
	{
		if (SetColor)
		{
			if (faithImage != null)
			{
				faithImage.color = new Color(1f, 1f, 1f, 0f);
			}
			if (warningImage != null)
			{
				warningImage.color = new Color(1f, 0f, 0f, 0f);
			}
		}
	}

	public void Play(AnimatedSizeChange animatedSizeChange, string Notification = "")
	{
		IsPlaying = true;
		sequence.Kill();
		WhiteBar.transform.DOKill();
		RedBar.transform.DOKill();
		base.transform.DOKill();
		RedBar.DOKill();
		if (faithImage != null && SetColor)
		{
			faithImage.color = new Color(1f, 1f, 1f, 0f);
		}
		if (faithImage != null && Notification != "")
		{
			if (animatedSizeChange.Difference > 0.005f || animatedSizeChange.Difference < -0.005f)
			{
				Debug.Log("Play Shake");
				base.transform.GetChild(0).DOKill();
				base.transform.GetChild(0).transform.localPosition = Vector3.zero;
				base.transform.GetChild(0).DOShakePosition(1f, Shake).SetUpdate(true);
			}
			faithImage.sprite = null;
			if (animatedSizeChange.Difference <= -0.1f)
			{
				faithImage.sprite = faithDoubleDown;
			}
			else if (animatedSizeChange.Difference >= 0.1f)
			{
				faithImage.sprite = faithDoubleUp;
			}
			else if (animatedSizeChange.Difference < 0f - MinimumDelta || (animatedSizeChange.Difference == MinimumDelta && animatedSizeChange.Size == 0f))
			{
				faithImage.sprite = faithDown;
			}
			else if (animatedSizeChange.Difference > MinimumDelta || (animatedSizeChange.Difference == MinimumDelta && animatedSizeChange.Size == 1f))
			{
				faithImage.sprite = faithUp;
			}
			if (SetColor)
			{
				faithIcon.color = Color.white;
				faithImage.color = new Color(1f, 1f, 1f, 0f);
			}
			faithIcon.DOKill();
			faithImage.DOKill();
			if (faithImage.sprite != null)
			{
				faithIcon.DOFade(0f, 0.5f).SetUpdate(true);
				faithImage.DOFade(1f, 0.5f).SetUpdate(true);
			}
		}
		if (animatedSizeChange.Size < CurrentSize)
		{
			RedBar.fillAmount = animatedSizeChange.Size;
			sequence = DOTween.Sequence();
			sequence.AppendInterval(0.5f);
			sequence.SetUpdate(true);
			sequence.Append(WhiteBar.DOFillAmount(animatedSizeChange.Size, 1f).SetEase(Ease.OutQuad));
			sequence.Play().SetUpdate(true).OnComplete(delegate
			{
				IsPlaying = false;
				if (faithImage != null && faithImage.sprite != null)
				{
					if (faithIcon != null)
					{
						faithIcon.DOFade(1f, 0.5f).SetDelay(1f);
					}
					if (faithImage != null)
					{
						faithImage.DOFade(0f, 0.5f).SetDelay(1f);
					}
				}
			});
		}
		else if (animatedSizeChange.Size > CurrentSize)
		{
			WhiteBar.fillAmount = animatedSizeChange.Size;
			sequence = DOTween.Sequence();
			if (WhiteBar.gameObject.activeSelf)
			{
				sequence.AppendInterval(0.5f);
			}
			sequence.Append(RedBar.DOFillAmount(animatedSizeChange.Size, 1f).SetEase(Ease.OutQuad));
			sequence.Play().SetUpdate(true).OnComplete(delegate
			{
				IsPlaying = false;
				if (faithImage != null && faithImage.sprite != null)
				{
					if (faithIcon != null)
					{
						faithIcon.DOFade(1f, 0.5f).SetDelay(1f).SetUpdate(true);
					}
					if (faithImage != null)
					{
						faithImage.DOFade(0f, 0.5f).SetDelay(1f).SetUpdate(true);
					}
				}
			});
		}
		else
		{
			IsPlaying = false;
			sequence = DOTween.Sequence();
			sequence.AppendInterval(0.5f);
			sequence.Append(RedBar.DOFillAmount(animatedSizeChange.Size, 1f).SetEase(Ease.OutQuad));
			sequence.Play().SetUpdate(true).OnComplete(delegate
			{
				if (faithImage != null && faithImage.sprite != null)
				{
					if (faithIcon != null)
					{
						faithIcon.DOFade(1f, 0.5f).SetDelay(1f).SetUpdate(true);
					}
					if (faithImage != null)
					{
						faithImage.DOFade(0f, 0.5f).SetDelay(1f).SetUpdate(true);
					}
				}
			});
		}
		if (SetColor)
		{
			RedBar.color = StaticColors.ColorForThreshold(animatedSizeChange.Size);
		}
		CurrentSize = animatedSizeChange.Size;
	}

	public void ShrinkBarToEmpty(float duration)
	{
		WhiteBar.fillAmount = 0f;
		RedBar.DOFillAmount(0f, duration).SetEase(Ease.InOutQuad).OnComplete(delegate
		{
			base.enabled = true;
		});
	}

	private float GetSizeFromLastChange()
	{
		if (QueuedChanges != null && QueuedChanges.Count > 0)
		{
			return QueuedChanges[QueuedChanges.Count - 1].Size;
		}
		return CurrentSize;
	}
}
