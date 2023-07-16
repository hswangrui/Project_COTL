using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

public abstract class EnemyRoundsBase : BaseMonoBehaviour
{
	public delegate void RoundEvent(int currentRound, int maxRounds);

	public delegate void EnemyEvent(UnitObject enemy);

	public static EnemyRoundsBase Instance;

	protected Action actionCallback;

	protected bool combatBegan;

	[CompilerGenerated]
	private readonly int _003CTotalRounds_003Ek__BackingField;

	[CompilerGenerated]
	private readonly int _003CCurrentRound_003Ek__BackingField;

	protected bool showRoundsUI = true;

	[SerializeField]
	private SpriteRenderer[] roundIndicators;

	public virtual int TotalRounds
	{
		[CompilerGenerated]
		get
		{
			return _003CTotalRounds_003Ek__BackingField;
		}
	}

	public virtual int CurrentRound
	{
		[CompilerGenerated]
		get
		{
			return _003CCurrentRound_003Ek__BackingField;
		}
	}

	public bool Completed { get; protected set; }

	public float SpawnDelay { get; set; }

	public static event RoundEvent OnRoundStart;

	public event EnemyEvent OnEnemySpawned;

	protected virtual void Awake()
	{
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void BeginCombat()
	{
		BeginCombat(false, null);
	}

	public virtual void BeginCombat(bool showRoundsUI, Action ActionCallback)
	{
		this.showRoundsUI = showRoundsUI;
		combatBegan = true;
		actionCallback = (Action)Delegate.Combine(actionCallback, ActionCallback);
		actionCallback = (Action)Delegate.Combine(actionCallback, new Action(CompletedRounds));
		AudioManager.Instance.SetMusicCombatState();
		if (Interaction_Chest.Instance != null)
		{
			GameManager.GetInstance().AddToCamera(Interaction_Chest.Instance.gameObject);
			Interaction_Chest.Instance.Delay = 3f;
		}
	}

	public void RoundStarted(int round, int totalRounds)
	{
		RoundEvent onRoundStart = EnemyRoundsBase.OnRoundStart;
		if (onRoundStart != null)
		{
			onRoundStart(round, totalRounds);
		}
		for (int i = 0; i < roundIndicators.Length; i++)
		{
			if (!roundIndicators[i].gameObject.activeSelf)
			{
				roundIndicators[i].color = new Color(roundIndicators[i].color.r, roundIndicators[i].color.g, roundIndicators[i].color.b, 0f);
				roundIndicators[i].DOFade(1f, 0.5f);
			}
			roundIndicators[i].gameObject.SetActive(i <= round - 1);
		}
	}

	private void CompletedRounds()
	{
		for (int i = 0; i < roundIndicators.Length; i++)
		{
			if (!roundIndicators[i].gameObject.activeSelf)
			{
				roundIndicators[i].color = new Color(roundIndicators[i].color.r, roundIndicators[i].color.g, roundIndicators[i].color.b, 0f);
				roundIndicators[i].DOFade(1f, 0.5f);
			}
			roundIndicators[i].gameObject.SetActive(true);
		}
		RoomLockController.RoomCompleted(true, false);
		if (Interaction_Chest.Instance != null)
		{
			Interaction_Chest.Instance.Delay = 0f;
			GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		}
	}

	public virtual void AddEnemyToRound(Health e)
	{
		if ((bool)e.GetComponent<UnitObject>())
		{
			EnemyEvent onEnemySpawned = this.OnEnemySpawned;
			if (onEnemySpawned != null)
			{
				onEnemySpawned(e.GetComponent<UnitObject>());
			}
		}
	}
}
