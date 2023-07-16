using System;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class PlayerDoctrineStone : MonoBehaviour
{
	public SkeletonGraphic Spine;

	public CanvasGroup CanvasGroup;

	private const int TotalCount = 3;

	public static Action OnIncreaseCount;

	public static Action OnDecreaseCount;

	public static Action OnCachePosition;

	[SerializeField]
	private bool IsPlaying;

	private bool GivenAnswer;

	private bool firstChoice;

	public static PlayerDoctrineStone Instance;

	private TrackEntry t;

	private Sequence FadeOutSequence;

	[SerializeField]
	public int CompletedDoctrineStones
	{
		get
		{
			return DataManager.Instance.CompletedDoctrineStones;
		}
		set
		{
			int completedDoctrineStones = DataManager.Instance.CompletedDoctrineStones;
			DataManager.Instance.CompletedDoctrineStones = value;
			if (value > completedDoctrineStones)
			{
				Action onIncreaseCount = OnIncreaseCount;
				if (onIncreaseCount != null)
				{
					onIncreaseCount();
				}
			}
			else
			{
				Action onDecreaseCount = OnDecreaseCount;
				if (onDecreaseCount != null)
				{
					onDecreaseCount();
				}
			}
		}
	}

	[SerializeField]
	private int CurrentCount
	{
		get
		{
			return DataManager.Instance.DoctrineCurrentCount;
		}
		set
		{
			DataManager.Instance.DoctrineCurrentCount = value;
		}
	}

	[SerializeField]
	private int TargetCount
	{
		get
		{
			return DataManager.Instance.DoctrineTargetCount;
		}
		set
		{
			DataManager.Instance.DoctrineTargetCount = value;
		}
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void HandleEvent(TrackEntry trackentry, global::Spine.Event e)
	{
		Debug.Log(e.Data.Name);
		switch (e.Data.Name)
		{
		case "CameraShake":
			CameraManager.instance.ShakeCameraForDuration(0.7f, 1f, 0.2f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
			break;
		case "Break":
			AudioManager.Instance.PlayOneShot("event:/temple_key/become_whole", PlayerFarming.Instance.gameObject);
			AudioManager.Instance.PlayOneShot("event:/material/stone_break", PlayerFarming.Instance.gameObject);
			break;
		case "Shake":
			AudioManager.Instance.PlayOneShot("event:/doctrine_stone/doctrine_shake", PlayerFarming.Instance.gameObject);
			break;
		case "Close":
			AudioManager.Instance.PlayOneShot("event:/ui/close_menu", PlayerFarming.Instance.gameObject);
			break;
		}
	}

	private void OnDisable()
	{
		if (FadeOutSequence != null)
		{
			FadeOutSequence.Kill();
			FadeOutSequence = null;
		}
		if (this == Instance)
		{
			Instance = null;
		}
	}

	private void Start()
	{
		Spine.AnimationState.Event += HandleEvent;
		Spine.AnimationState.SetAnimation(0, CurrentCount.ToString(), false);
		CanvasGroup.alpha = 0f;
		Spine.enabled = false;
	}

	private void OnDestroy()
	{
		Spine.AnimationState.Event -= HandleEvent;
		if (t != null)
		{
			t.Complete -= CompleteAnimation;
		}
	}

	public static void Play(int Delta)
	{
		if (!(Instance == null))
		{
			Instance.GivePiece(Delta);
		}
	}

	public void GivePiece(int Delta)
	{
		TargetCount += Delta;
		if (TargetCount >= 3)
		{
			if (PlayerCanvas.Instance != null)
			{
				PlayerCanvas.Instance.enabled = false;
			}
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.FindDoctrineStone);
			Action onCachePosition = OnCachePosition;
			if (onCachePosition != null)
			{
				onCachePosition();
			}
			if (DataManager.Instance.ShowFirstDoctrineStone)
			{
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CrownBone.gameObject, 7f);
			}
		}
	}

	private void Update()
	{
		if (IsPlaying || CurrentCount >= TargetCount)
		{
			return;
		}
		if (FadeOutSequence != null)
		{
			FadeOutSequence.Kill();
		}
		CanvasGroup.DOKill();
		CanvasGroup.alpha = 1f;
		Spine.enabled = true;
		IsPlaying = true;
		t = Spine.AnimationState.SetAnimation(0, CurrentCount + 1 + "-activate", false);
		t.Complete += CompleteAnimation;
		Spine.AnimationState.AddAnimation(0, (CurrentCount + 1).ToString(), false, 0f);
		if (CurrentCount + 1 >= 3)
		{
			AudioManager.Instance.PlayOneShot("event:/doctrine_stone/doctrine_last_piece");
			if (DataManager.Instance.ShowFirstDoctrineStone)
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.FoundItem;
			}
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/doctrine_stone/doctrine_piece");
		}
	}

	private void CompleteAnimation(TrackEntry trackentry)
	{
		t.Complete -= CompleteAnimation;
		int currentCount = CurrentCount + 1;
		CurrentCount = currentCount;
		if (CurrentCount >= 3)
		{
			EndDeclareDoctrine();
		}
		else
		{
			EndPlayingAndFadeOut(false);
		}
	}

	private void EndDeclareDoctrine()
	{
		if (PlayerCanvas.Instance != null)
		{
			PlayerCanvas.Instance.enabled = true;
		}
		if (DataManager.Instance.ShowFirstDoctrineStone)
		{
			GameManager.GetInstance().OnConversationEnd();
		}
		DataManager.Instance.ShowFirstDoctrineStone = false;
		CurrentCount = 0;
		TargetCount -= 3;
		EndPlayingAndFadeOut(true);
	}

	private void EndPlayingAndFadeOut(bool IncrementCount)
	{
		FadeOutSequence = DOTween.Sequence();
		if (IncrementCount)
		{
			FadeOutSequence.AppendInterval(0.25f);
			FadeOutSequence.AppendCallback(delegate
			{
				CanvasGroup.alpha = 0f;
				Spine.enabled = false;
				int completedDoctrineStones = CompletedDoctrineStones + 1;
				CompletedDoctrineStones = completedDoctrineStones;
			});
			FadeOutSequence.AppendInterval(1f);
			FadeOutSequence.AppendCallback(delegate
			{
				IsPlaying = false;
			});
		}
		else
		{
			FadeOutSequence.AppendInterval(0.5f).OnComplete(delegate
			{
				IsPlaying = false;
			});
		}
		FadeOutSequence.Play();
	}
}
