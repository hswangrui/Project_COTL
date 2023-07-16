using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IllnessBar : MonoBehaviour
{
	public BarController BarController;

	public static IllnessBar Instance;

	public GameObject Container;

	public CanvasGroup CanvasGroup;

	public Image WhitePulse;

	private bool showing;

	private Sequence sequence;

	private Sequence PulseSequence;

	public static float Max
	{
		get
		{
			return Mathf.Max(10f, DynamicMax);
		}
	}

	public static float DynamicMax
	{
		get
		{
			return DataManager.Instance.IllnessBarDynamicMax;
		}
		set
		{
			DataManager.Instance.IllnessBarDynamicMax = value;
		}
	}

	public static float Count
	{
		get
		{
			return DataManager.Instance.IllnessBarCount;
		}
		set
		{
			DataManager.Instance.IllnessBarCount = value;
		}
	}

	public static float IllnessNormalized
	{
		get
		{
			return 1f - Count / Max;
		}
	}

	private void Pulse(float Duration = 1f, float Scale = 1.5f, float Alpha = 0.8f, Ease Ease = Ease.OutSine)
	{
		WhitePulse.transform.localScale = Vector3.one;
		WhitePulse.transform.DOScale(Vector3.one * Scale, Duration).SetEase(Ease);
		WhitePulse.color = new Color(1f, 1f, 1f, Alpha);
		WhitePulse.DOFade(0f, Duration).SetEase(Ease);
	}

	private void Start()
	{
		BarController.SetBarSize(1f - Count / Max, false);
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(DecreaseDynamicMax));
		showing = DataManager.Instance.ShowCultIllness;
	}

	private void OnDestroy()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(DecreaseDynamicMax));
	}

	private void OnEnable()
	{
		Instance = this;
		Container.SetActive(DataManager.Instance.ShowCultIllness);
		StartCoroutine(UpdateRoutine());
	}

	private void Update()
	{
		if (DataManager.Instance.ShowCultIllness)
		{
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
		}
	}

	private void DecreaseDynamicMax()
	{
		if (Count < DynamicMax)
		{
			DynamicMax = Mathf.Clamp(DynamicMax - 1f, 10f, DynamicMax);
		}
	}

	public void Reveal()
	{
		StartCoroutine(RevealRoutine());
	}

	private IEnumerator RevealRoutine()
	{
		while (HUD_Manager.IsTransitioning || HUD_Manager.Instance.Hidden)
		{
			yield return null;
		}
		if (!DataManager.Instance.ShowCultIllness)
		{
			AudioManager.Instance.PlayOneShot("event:/ui/heretics_defeated");
			Vector3 LocalPosition = Container.transform.localPosition;
			Container.transform.parent = HUD_Manager.Instance.transform;
			Container.transform.localPosition = Vector3.zero;
			Container.transform.parent = base.transform;
			DataManager.Instance.ShowCultIllness = true;
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
		}
	}

	private void OnDisable()
	{
		if (sequence != null)
		{
			sequence.Kill();
		}
		if (PulseSequence != null)
		{
			PulseSequence.Kill();
		}
		Container.transform.DOKill();
		CanvasGroup.DOKill();
		WhitePulse.transform.DOKill();
		WhitePulse.DOKill();
		sequence = null;
		PulseSequence = null;
		if (Instance == this)
		{
			Instance = null;
		}
		StopAllCoroutines();
	}

	public static void UpdateSimulation(float DeltaGameTime)
	{
		if (!DataManager.Instance.ShowCultIllness)
		{
			return;
		}
		UpdateCount(DeltaGameTime);
		if ((DataManager.Instance.OnboardedSickFollower || PlayerFarming.Location == FollowerLocation.Base) && !TimeManager.IsNight && DataManager.Instance.Followers.Count != 0 && !(TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToBecomeIll < 600f / DifficultyManager.GetTimeBetweenIllnessMultiplier()) && 1f - Count / Max < 0.25f)
		{
			FollowerBrain followerBrain = FollowerBrain.RandomAvailableBrainNoCurseState();
			if (followerBrain != null && DataManager.Instance.OnboardedSickFollower)
			{
				DataManager.Instance.LastFollowerToBecomeIll = TimeManager.TotalElapsedGameTime;
				followerBrain.MakeSick();
			}
		}
	}

	private static float UpdateCount(float deltaGameTime)
	{
		if (DataManager.Instance == null || BaseLocationManager.Instance == null)
		{
			return Max;
		}
		if (!DataManager.Instance.ShowCultIllness || !BaseLocationManager.Instance.StructuresPlaced)
		{
			return Max;
		}
		float count = Count;
		Count = 0f;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID) && allBrain.Info.CursedState == Thought.Ill && (allBrain.CurrentTask == null || !(allBrain.CurrentTask is FollowerTask_SleepBedRest)))
			{
				Count += 1f;
			}
		}
		Count = StructureManager.GetWasteCount();
		if (Count > DynamicMax)
		{
			DynamicMax = Count;
		}
		return Count;
	}

	private IEnumerator UpdateRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (!DataManager.Instance.ShowCultIllness)
			{
				yield return null;
				continue;
			}
			BarController.SetBarSize(1f - Count / Max, true);
			if (1f - Count / Max < 0.25f)
			{
				if (sequence == null)
				{
					sequence = DOTween.Sequence();
					sequence.Append(BarController.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f));
					sequence.AppendInterval(2f);
					sequence.SetLoops(-1);
					sequence.Play();
					PulseSequence = DOTween.Sequence();
					PulseSequence.AppendCallback(delegate
					{
						WhitePulse.transform.localScale = Vector3.one;
						WhitePulse.transform.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.OutSine);
						WhitePulse.color = new Color(1f, 1f, 1f, 0.8f);
						WhitePulse.DOFade(0f, 1f).SetEase(Ease.OutSine);
					});
					PulseSequence.AppendInterval(2.5f);
					PulseSequence.SetLoops(-1);
					PulseSequence.Play();
				}
			}
			else if (sequence != null)
			{
				sequence.Kill();
				sequence = null;
				PulseSequence.Kill();
				PulseSequence = null;
			}
			yield return new WaitForSeconds(0.835f);
		}
	}
}
