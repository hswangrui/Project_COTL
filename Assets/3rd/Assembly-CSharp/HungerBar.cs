using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
	public BarController BarController;

	public static HungerBar Instance;

	public CanvasGroup CanvasGroup;

	public GameObject Container;

	public Image WhitePulse;

	[SerializeField]
	private CanvasGroup flash;

	[SerializeField]
	private CanvasGroup Lock;

	private bool showing;

	private bool activatedLock;

	private static float reservedSatiation = 0f;

	private bool Revealing;

	private static float LastCount = 0f;

	private static float CountInterval = 15f;

	private static float ElapsedGameTime;

	private Sequence sequence;

	private Sequence PulseSequence;

	public static float MIN_HUNGER
	{
		get
		{
			return 0f;
		}
	}

	public static float MAX_HUNGER
	{
		get
		{
			return 100f;
		}
	}

	public static float HungerNormalized
	{
		get
		{
			return Count / MAX_HUNGER;
		}
	}

	public static float Count
	{
		get
		{
			return DataManager.Instance.HungerBarCount;
		}
		set
		{
			DataManager.Instance.HungerBarCount = value;
		}
	}

	public static float ReservedSatiation
	{
		get
		{
			return reservedSatiation;
		}
		set
		{
			reservedSatiation = Mathf.Clamp(value, MIN_HUNGER, MAX_HUNGER);
		}
	}

	private void Pulse(float Duration = 1f, float Scale = 1.5f, float Alpha = 0.8f, Ease Ease = Ease.OutSine)
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

	private void TurnLockOn()
	{
		activatedLock = true;
		Lock.gameObject.SetActive(true);
		Flash();
		Pulse();
		Lock.alpha = 1f;
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOShakePosition(2f, new Vector3(5f, 0f, 0f)).SetDelay(0.5f);
	}

	private void TurnLockOff()
	{
		activatedLock = false;
		Flash();
		Pulse();
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOShakePosition(2f, new Vector3(5f, 0f, 0f));
		Lock.DOFade(0f, 2f);
	}

	private void Start()
	{
		BarController.SetBarSize(HungerNormalized, false);
		showing = DataManager.Instance.ShowCultHunger;
	}

	private void OnEnable()
	{
		Lock.gameObject.SetActive(false);
		flash.gameObject.SetActive(false);
		activatedLock = false;
		Instance = this;
		Container.SetActive(DataManager.Instance.ShowCultHunger);
		StartCoroutine(UpdateRoutine());
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
		Count = 0f;
		int num = 0;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID))
			{
				num++;
				Count += allBrain.Stats.Satiation;
				Count -= allBrain.Stats.Starvation;
			}
		}
		Count /= num;
		if (Instance != null)
		{
			Instance.BarController.SetBarSize(HungerNormalized, false);
		}
		yield return new WaitForSeconds(0.5f);
		Revealing = true;
		AudioManager.Instance.PlayOneShot("event:/ui/heretics_defeated");
		Vector3 LocalPosition = Container.transform.localPosition;
		Container.transform.parent = HUD_Manager.Instance.transform;
		Container.transform.localPosition = Vector3.zero;
		Container.transform.parent = base.transform;
		DataManager.Instance.ShowCultHunger = true;
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
		Revealing = false;
		showing = true;
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

	private void Update()
	{
		if (DataManager.Instance.ShowCultHunger)
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

	public static void UpdateSimulation(float DeltaGameTime)
	{
		if (!DataManager.Instance.ShowCultHunger || (Instance != null && Instance.Revealing))
		{
			return;
		}
		GetCount(DeltaGameTime);
		if (DataManager.Instance.Followers.Count > 0 && HungerNormalized < 0.25f && !(TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToStartStarving < 600f))
		{
			FollowerBrain hungriestFollowerBrainNotStarving = FollowerManager.GetHungriestFollowerBrainNotStarving();
			if (hungriestFollowerBrainNotStarving != null)
			{
				hungriestFollowerBrainNotStarving.MakeStarve();
				DataManager.Instance.LastFollowerToStartStarving = TimeManager.TotalElapsedGameTime;
			}
		}
	}

	private static void GetCount(float DeltaGameTime)
	{
		ElapsedGameTime += DeltaGameTime;
		if (!(ElapsedGameTime > CountInterval))
		{
			return;
		}
		ElapsedGameTime = 0f;
		float count = Count;
		Count = 0f;
		int num = 0;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID, true))
			{
				num++;
				Count += allBrain.Stats.Satiation;
				Count -= allBrain.Stats.Starvation;
			}
		}
		Count /= num;
		if (Instance != null)
		{
			Instance.BarController.SetBarSize(HungerNormalized, true);
		}
	}

	private IEnumerator UpdateRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (!DataManager.Instance.ShowCultHunger)
			{
				yield return null;
				continue;
			}
			if (Time.frameCount % 5 == 0)
			{
				if (FollowerBrainStats.Fasting && !activatedLock)
				{
					TurnLockOn();
				}
				else if (!FollowerBrainStats.Fasting && activatedLock)
				{
					TurnLockOff();
				}
			}
			if (HungerNormalized < 0.25f)
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
