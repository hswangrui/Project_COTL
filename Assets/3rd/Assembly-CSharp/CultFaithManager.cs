using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CultFaithManager : MonoBehaviour
{
	public delegate void ThoughtEvent(Thought thought);

	public static CultFaithManager Instance;

	public GameObject Container;

	public CanvasGroup CanvasGroup;

	public Image WhitePulse;

	[SerializeField]
	private CanvasGroup flash;

	[SerializeField]
	private CanvasGroup faithLock;

	private bool showing;

	public static Action OnPulse;

	private const float STARTING_FAITH = 50f;

	public const float LOW_FAITH = 5f;

	public const float MIN_FAITH = 0f;

	public const float MAX_FAITH = 85f;

	private static Action<bool, NotificationData> OnUpdateFaith;

	public BarController BarController;

	private bool activatedLock;

	private Sequence sequence;

	private Sequence PulseSequence;

	public static List<ThoughtData> TrackedThoughts
	{
		get
		{
			return DataManager.Instance.Thoughts;
		}
		set
		{
			DataManager.Instance.Thoughts = value;
		}
	}

	public static float StaticFaith
	{
		get
		{
			return DataManager.Instance.StaticFaith;
		}
		set
		{
			DataManager.Instance.StaticFaith = value;
		}
	}

	public static float CurrentFaith
	{
		get
		{
			return DataManager.Instance.CultFaith;
		}
		set
		{
			DataManager.Instance.CultFaith = value;
		}
	}

	public static float CultFaithNormalised
	{
		get
		{
			return CurrentFaith / 85f;
		}
	}

	public event ThoughtEvent OnThoughtModified;

	private void Awake()
	{
		Instance = this;
		OnUpdateFaith = (Action<bool, NotificationData>)Delegate.Combine(OnUpdateFaith, new Action<bool, NotificationData>(UpdateBar));
	}

	private void Pulse(float Duration = 1f, float Scale = 1.25f, float Alpha = 0.8f, Ease Ease = Ease.OutSine)
	{
		WhitePulse.transform.localScale = Vector3.one;
		WhitePulse.transform.DOScale(Vector3.one * Scale, Duration).SetEase(Ease);
		WhitePulse.color = new Color(1f, 1f, 1f, Alpha);
		WhitePulse.DOFade(0f, Duration).SetEase(Ease);
	}

	private void Flash()
	{
		flash.gameObject.SetActive(true);
		flash.alpha = 1f;
		flash.GetComponent<Image>().color = Color.white;
		flash.GetComponent<Image>().DOColor(StaticColors.RedColor, 0.33f);
		flash.DOFade(0f, 1f).SetDelay(0.33f);
	}

	private void ReadThoughts()
	{
		Debug.Log("FAITH: " + CurrentFaith);
		foreach (ThoughtData trackedThought in TrackedThoughts)
		{
			Debug.Log(trackedThought.ThoughtType.ToString());
		}
	}

	public static bool HasThought(Thought thought)
	{
		foreach (ThoughtData trackedThought in TrackedThoughts)
		{
			if (trackedThought.ThoughtType == thought)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		faithLock.gameObject.SetActive(false);
		flash.gameObject.SetActive(false);
		activatedLock = false;
		UpdateBar(false, null);
		Container.SetActive(DataManager.Instance.ShowCultFaith);
	}

	public void Reveal()
	{
		GameManager.GetInstance().StartCoroutine(RevealRoutine());
	}

	private IEnumerator RevealRoutine()
	{
		while (HUD_Manager.IsTransitioning || HUD_Manager.Instance.Hidden)
		{
			yield return null;
		}
		while (Time.timeScale < 1f)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/heretics_defeated");
		Vector3 LocalPosition = Container.transform.localPosition;
		Container.transform.parent = HUD_Manager.Instance.transform;
		Container.transform.localPosition = Vector3.zero;
		Container.transform.parent = base.transform;
		DataManager.Instance.ShowCultFaith = true;
		CanvasGroup.alpha = 0f;
		CanvasGroup.DOFade(1f, 1f);
		Container.SetActive(true);
		Container.transform.localScale = Vector3.one * 3f;
		Container.transform.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.OutBack);
		yield return new WaitForSeconds(0.4f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(0.6f);
		Container.transform.DOLocalMove(LocalPosition, 1f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(0.8f);
		AudioManager.Instance.PlayOneShot("event:/ui/level_node_beat_level");
		Container.transform.DOScale(Vector3.one, 1f).SetEase(Ease.InOutBack);
		yield return new WaitForSeconds(0.5f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		showing = true;
	}

	private void OnDestroy()
	{
		base.gameObject.transform.DOKill();
		faithLock.DOKill();
		WhitePulse.DOKill();
		flash.DOKill();
		CanvasGroup.DOKill();
		Container.transform.DOKill();
		if (sequence != null)
		{
			sequence.Kill();
			sequence = null;
		}
		if (PulseSequence != null)
		{
			PulseSequence.Kill();
			PulseSequence = null;
		}
		OnUpdateFaith = (Action<bool, NotificationData>)Delegate.Remove(OnUpdateFaith, new Action<bool, NotificationData>(UpdateBar));
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static void GetFaith(float Delta, float DeltaDisplay, bool Animate, NotificationBase.Flair Flair, string NotificationMessage = "", int FollowerID = -1, params string[] args)
	{
		float num = 0f;
		if (FollowerBrainStats.BrainWashed)
		{
			Delta = 0f;
			DeltaDisplay = 0f;
		}
		else
		{
			foreach (ThoughtData trackedThought in TrackedThoughts)
			{
				int num2 = -1;
				while (++num2 < trackedThought.Quantity)
				{
					num += ((num2 <= 0) ? trackedThought.Modifier : ((float)trackedThought.StackModifier));
				}
			}
		}
		StaticFaith = Mathf.Clamp(StaticFaith + Delta, 0f, 85f);
		CurrentFaith = Mathf.Clamp(StaticFaith + num, 0f, 85f);
		if (CurrentFaith >= 42.5f)
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CrisisOfFaith);
		}
		else if (CurrentFaith <= 5f && DataManager.Instance.TimeSinceFaithHitEmpty == -1f)
		{
			DataManager.Instance.TimeSinceFaithHitEmpty = TimeManager.TotalElapsedGameTime;
		}
		else if (CurrentFaith > 5f && DataManager.Instance.TimeSinceFaithHitEmpty != -1f)
		{
			DataManager.Instance.TimeSinceFaithHitEmpty = -1f;
		}
		if (Instance == null)
		{
			Action<bool, NotificationData> onUpdateFaith = OnUpdateFaith;
			if (onUpdateFaith != null)
			{
				onUpdateFaith(Animate, null);
			}
			if (NotificationMessage != "" && NotificationCentre.Instance != null)
			{
				NotificationCentre.Instance.PlayFaithNotification(NotificationMessage, DeltaDisplay, Flair, FollowerID, args);
			}
		}
		else
		{
			Action<bool, NotificationData> onUpdateFaith2 = OnUpdateFaith;
			if (onUpdateFaith2 != null)
			{
				onUpdateFaith2(Animate, new NotificationData(NotificationMessage, DeltaDisplay, FollowerID, Flair, args));
			}
		}
	}

	public static void AddThought(Thought thought, int FollowerID = -1, float faithMultiplier = 1f, params string[] args)
	{
		if (!DataManager.Instance.ShowCultFaith || DataManager.Instance.Followers.Count == 0 || DungeonSandboxManager.Active)
		{
			return;
		}
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		data.FollowerID = FollowerID;
		data.Modifier *= faithMultiplier;
		if ((bool)Instance)
		{
			ThoughtEvent onThoughtModified = Instance.OnThoughtModified;
			if (onThoughtModified != null)
			{
				onThoughtModified(thought);
			}
		}
		if (data.TrackThought)
		{
			int num = -1;
			while (++num < TrackedThoughts.Count)
			{
				ThoughtData thoughtData = TrackedThoughts[num];
				if (thoughtData.ThoughtGroup == data.ThoughtGroup && thoughtData.Stacking <= 0)
				{
					TrackedThoughts[num] = data;
					GetFaith(0f, thoughtData.Modifier, true, GetNotificationFlair(thought), FollowerThoughts.GetNotificationOnLocalizationKey(thoughtData.ThoughtType), thoughtData.FollowerID, args);
					return;
				}
				if (thoughtData.ThoughtType == thought && thoughtData.Stacking > 0)
				{
					if (thoughtData.Quantity < thoughtData.Stacking)
					{
						thoughtData.Quantity++;
					}
					else
					{
						thoughtData.CoolDowns.RemoveAt(0);
						thoughtData.TimeStarted.RemoveAt(0);
					}
					thoughtData.CoolDowns.Add(data.Duration);
					thoughtData.TimeStarted.Add(TimeManager.TotalElapsedGameTime);
					GetFaith(0f, thoughtData.Modifier, true, GetNotificationFlair(thought), FollowerThoughts.GetNotificationOnLocalizationKey(thoughtData.ThoughtType), thoughtData.FollowerID, args);
					return;
				}
			}
			TrackedThoughts.Add(data);
			GetFaith(0f, data.Modifier, true, GetNotificationFlair(thought), FollowerThoughts.GetNotificationOnLocalizationKey(data.ThoughtType), data.FollowerID, args);
		}
		else
		{
			GetFaith(data.Modifier, data.Modifier, true, GetNotificationFlair(thought), FollowerThoughts.GetNotificationOnLocalizationKey(data.ThoughtType), data.FollowerID, args);
		}
	}

	private static NotificationBase.Flair GetNotificationFlair(Thought thought)
	{
		switch (thought)
		{
		case Thought.Cult_CompleteQuest:
			return NotificationBase.Flair.Positive;
		case Thought.Cult_FollowerDied:
		case Thought.Cult_FollowerDied_Trait:
		case Thought.Cult_FollowerDiedOfOldAge:
		case Thought.DiedFromIllness:
			return NotificationBase.Flair.Negative;
		default:
			return NotificationBase.Flair.None;
		}
	}

	public static void RemoveThought(Thought thought)
	{
		foreach (ThoughtData trackedThought in TrackedThoughts)
		{
			if (trackedThought.ThoughtType != thought)
			{
				continue;
			}
			TrackedThoughts.Remove(trackedThought);
			if ((bool)Instance)
			{
				ThoughtEvent onThoughtModified = Instance.OnThoughtModified;
				if (onThoughtModified != null)
				{
					onThoughtModified(trackedThought.ThoughtType);
				}
			}
			GameManager.GetInstance().StartCoroutine(DelayGetFaith(trackedThought));
			break;
		}
	}

	private static IEnumerator DelayGetFaith(ThoughtData thought)
	{
		yield return null;
		GetFaith(0f, 0f, true, GetNotificationFlair(thought.ThoughtType), FollowerThoughts.GetNotificationOffLocalizationKey(thought.ThoughtType), -1);
	}

	public static void UpdateThoughts(float DeltaGameTime)
	{
		foreach (ThoughtData trackedThought in TrackedThoughts)
		{
			if (trackedThought.Duration == -1f || !(TimeManager.TotalElapsedGameTime - trackedThought.TimeStarted[0] > trackedThought.CoolDowns[0]))
			{
				continue;
			}
			if (trackedThought.Quantity > 1)
			{
				trackedThought.Quantity--;
				trackedThought.CoolDowns.RemoveAt(0);
				trackedThought.TimeStarted.RemoveAt(0);
			}
			else
			{
				TrackedThoughts.Remove(trackedThought);
				if ((bool)Instance)
				{
					ThoughtEvent onThoughtModified = Instance.OnThoughtModified;
					if (onThoughtModified != null)
					{
						onThoughtModified(trackedThought.ThoughtType);
					}
				}
			}
			GetFaith(0f, 0f, true, GetNotificationFlair(trackedThought.ThoughtType), FollowerThoughts.GetNotificationOffLocalizationKey(trackedThought.ThoughtType), -1);
			return;
		}
		if (CheatConsole.FaithType == CheatConsole.FaithTypes.DRIP && !FollowerBrainStats.BrainWashed && (Instance == null || (Instance != null && !Instance.BarController.IsPlaying)))
		{
			if (CurrentFaith > 0.35f)
			{
				StaticFaith -= DeltaGameTime * (0.02f * DifficultyManager.GetDripMultiplier());
			}
			else
			{
				StaticFaith -= DeltaGameTime * (0.01f * DifficultyManager.GetDripMultiplier());
			}
			GetFaith(0f, 0f, false, NotificationBase.Flair.None, "", -1);
		}
	}

	private void Start()
	{
		GetFaith(0f, 0f, false, NotificationBase.Flair.None, "", -1);
		showing = DataManager.Instance.ShowCultFaith;
	}

	private void UpdateBar(bool Animate, NotificationData NotificationData)
	{
		BarController.SetBarSize(CultFaithNormalised, Animate, false, NotificationData);
	}

	public static void UpdateSimulation(float DeltaGameTime)
	{
		if (!DataManager.Instance.ShowCultFaith)
		{
			return;
		}
		UpdateThoughts(DeltaGameTime);
		if ((DataManager.Instance.OnboardedDissenter || PlayerFarming.Location == FollowerLocation.Base) && !TimeManager.IsNight && DataManager.Instance.Followers.Count != 0 && TimeManager.CurrentPhase != DayPhase.Dusk && CurrentFaith / 85f < 0.25f && !(TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToStartDissenting < 3600f))
		{
			FollowerBrain followerBrain = FollowerBrain.RandomAvailableBrainNoCurseState();
			if (followerBrain != null && DataManager.Instance.OnboardedDissenter && followerBrain.Info.Necklace != InventoryItem.ITEM_TYPE.Necklace_Loyalty)
			{
				followerBrain.MakeDissenter();
				DataManager.Instance.LastFollowerToStartDissenting = TimeManager.TotalElapsedGameTime;
			}
		}
	}

	private void TurnFaithLockOn()
	{
		activatedLock = true;
		faithLock.gameObject.SetActive(true);
		Flash();
		Pulse();
		faithLock.alpha = 1f;
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOShakePosition(2f, new Vector3(5f, 0f, 0f)).SetDelay(0.5f);
	}

	private void TurnFaithLockOff()
	{
		activatedLock = false;
		Flash();
		Pulse();
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOShakePosition(2f, new Vector3(5f, 0f, 0f));
		faithLock.DOFade(0f, 2f);
	}

	private void Update()
	{
		if (!DataManager.Instance.ShowCultFaith)
		{
			return;
		}
		if (showing && DataManager.Instance.Followers.Count <= 0)
		{
			showing = false;
			CanvasGroup.DOFade(0f, 1f);
		}
		else if (!showing && DataManager.Instance.Followers.Count > 0)
		{
			showing = true;
			CanvasGroup.DOFade(1f, 1f);
		}
		if (Time.frameCount % 5 != 0)
		{
			return;
		}
		if (FollowerBrainStats.BrainWashed && !activatedLock)
		{
			TurnFaithLockOn();
		}
		else if (!FollowerBrainStats.BrainWashed && activatedLock)
		{
			TurnFaithLockOff();
		}
		if (CurrentFaith / 85f < 0.25f)
		{
			if (sequence != null)
			{
				return;
			}
			sequence = DOTween.Sequence();
			sequence.Append(BarController.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f));
			sequence.AppendInterval(2f);
			sequence.SetLoops(-1);
			sequence.Play();
			PulseSequence = DOTween.Sequence();
			PulseSequence.AppendCallback(delegate
			{
				WhitePulse.transform.localScale = Vector3.one;
				WhitePulse.transform.DOScale(Vector3.one * 1.25f, 1f).SetEase(Ease.OutSine);
				WhitePulse.color = new Color(1f, 1f, 1f, 0.8f);
				WhitePulse.DOFade(0f, 1f).SetEase(Ease.OutSine);
				Action onPulse = OnPulse;
				if (onPulse != null)
				{
					onPulse();
				}
			});
			PulseSequence.AppendInterval(2.5f);
			PulseSequence.SetLoops(-1);
			PulseSequence.Play();
		}
		else if (sequence != null)
		{
			sequence.Kill();
			sequence = null;
			PulseSequence.Kill();
			PulseSequence = null;
		}
	}
}
