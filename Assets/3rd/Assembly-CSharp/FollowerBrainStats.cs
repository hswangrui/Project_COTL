using System;
using System.Collections.Generic;
using UnityEngine;

public class FollowerBrainStats
{
	public delegate void StatChangedEvent(int followerID, float newValue, float oldValue, float change);

	public delegate void StatStateChangedEvent(int followerId, FollowerStatState newState, FollowerStatState oldState);

	public delegate void StatusChangedEvent(int followerID);

	private int _followerID;

	private FollowerInfo _info;

	private FollowerBrain followerBrain;

	public static StatChangedEvent OnLevelUp;

	public static StatChangedEvent OnMaxHPChanged;

	public static StatChangedEvent OnHPChanged;

	public static StatChangedEvent OnDevotionChanged;

	public static StatChangedEvent OnDevotionGivenChanged;

	public static StatStateChangedEvent OnHappinessStateChanged;

	public static StatChangedEvent OnFearLoveChanged;

	public static StatChangedEvent OnSatiationChanged;

	public static StatStateChangedEvent OnSatiationStateChanged;

	public static StatChangedEvent OnStarvationChanged;

	public static StatStateChangedEvent OnStarvationStateChanged;

	public static StatChangedEvent OnBathroomChanged;

	public static StatStateChangedEvent OnBathroomStateChanged;

	public static StatChangedEvent OnRestChanged;

	public static StatStateChangedEvent OnRestStateChanged;

	public static StatChangedEvent OnIllnessChanged;

	public static StatStateChangedEvent OnIllnessStateChanged;

	public static StatChangedEvent OnExhaustionChanged;

	public static StatStateChangedEvent OnExhaustionStateChanged;

	public static StatChangedEvent OnReeducationChanged;

	public static StatStateChangedEvent OnReeducationStateChanged;

	public Action OnReeducationComplete;

	public static StatusChangedEvent OnMotivatedChanged;

	public bool WorkerBeenGivenOrders
	{
		get
		{
			return _info.WorkerBeenGivenOrders;
		}
		set
		{
			if (_info.WorkerBeenGivenOrders != value && value)
			{
				followerBrain.CheckChangeTask();
			}
			_info.WorkerBeenGivenOrders = value;
		}
	}

	public float MaxHP
	{
		get
		{
			return _info.MaxHP;
		}
		set
		{
			float maxHP = MaxHP;
			_info.MaxHP = value;
			if (maxHP != MaxHP)
			{
				StatChangedEvent onMaxHPChanged = OnMaxHPChanged;
				if (onMaxHPChanged != null)
				{
					onMaxHPChanged(_followerID, MaxHP, maxHP, MaxHP - maxHP);
				}
			}
		}
	}

	public float HP
	{
		get
		{
			return _info.HP;
		}
		set
		{
			float hP = HP;
			_info.HP = value;
			if (hP != HP)
			{
				StatChangedEvent onHPChanged = OnHPChanged;
				if (onHPChanged != null)
				{
					onHPChanged(_followerID, HP, hP, HP - hP);
				}
			}
		}
	}

	public int DevotionGiven
	{
		get
		{
			return _info.DevotionGiven;
		}
		set
		{
			int devotionGiven = DevotionGiven;
			if (devotionGiven != DevotionGiven)
			{
				StatChangedEvent onDevotionGivenChanged = OnDevotionGivenChanged;
				if (onDevotionGivenChanged != null)
				{
					onDevotionGivenChanged(_followerID, DevotionGiven, devotionGiven, DevotionGiven - devotionGiven);
				}
			}
		}
	}

	public bool HasLevelledUp
	{
		get
		{
			return _info.Adoration >= MAX_ADORATION;
		}
	}

	public float Adoration
	{
		get
		{
			return _info.Adoration;
		}
		set
		{
			_info.Adoration = value;
		}
	}

	public float MAX_ADORATION
	{
		get
		{
			return 100f;
		}
	}

	public int LastSermon
	{
		get
		{
			return _info.LastSermon;
		}
		set
		{
			_info.LastSermon = value;
		}
	}

	public bool HadSermonYesterday
	{
		get
		{
			return _info.LastSermon == DataManager.Instance.CurrentDayIndex - 1;
		}
	}

	public bool PaidTithes
	{
		get
		{
			return _info.PaidTithes;
		}
		set
		{
			_info.PaidTithes = value;
		}
	}

	public bool ReceivedBlessing
	{
		get
		{
			return _info.ReceivedBlessing;
		}
		set
		{
			_info.ReceivedBlessing = value;
		}
	}

	public bool KissedAction
	{
		get
		{
			return _info.KissedAction;
		}
		set
		{
			_info.KissedAction = value;
		}
	}

	public bool ReeducatedAction
	{
		get
		{
			return _info.ReeducatedAction;
		}
		set
		{
			_info.ReeducatedAction = value;
		}
	}

	public bool Inspired
	{
		get
		{
			return _info.Inspired;
		}
		set
		{
			_info.Inspired = value;
		}
	}

	public bool PetDog
	{
		get
		{
			return _info.PetDog;
		}
		set
		{
			_info.PetDog = value;
		}
	}

	public bool Intimidated
	{
		get
		{
			return _info.Intimidated;
		}
		set
		{
			_info.Intimidated = value;
		}
	}

	public bool Bribed
	{
		get
		{
			return _info.Bribed;
		}
		set
		{
			_info.Bribed = value;
		}
	}

	public List<ThoughtData> Thoughts
	{
		get
		{
			return _info.Thoughts;
		}
		set
		{
			_info.Thoughts = value;
		}
	}

	public float Happiness
	{
		get
		{
			return 50f;
		}
	}

	public float FearLove
	{
		get
		{
			return _info.FearLove;
		}
		set
		{
			float fearLove = FearLove;
			_info.FearLove = value;
			if (fearLove != FearLove)
			{
				StatChangedEvent onFearLoveChanged = OnFearLoveChanged;
				if (onFearLoveChanged != null)
				{
					onFearLoveChanged(_followerID, FearLove, fearLove, FearLove - fearLove);
				}
			}
		}
	}

	public float Satiation
	{
		get
		{
			return _info.Satiation;
		}
		set
		{
			if (Fasting && value < Satiation)
			{
				return;
			}
			if (value <= 0f && DataManager.Instance.CookedFirstFood)
			{
				if (_info.IsStarving && !TimeManager.PauseGameTime)
				{
					Starvation += Mathf.Abs(value - 0f) * ((PlayerFarming.Location == followerBrain.Location) ? 1f : 0.5f);
				}
			}
			else if (value > Satiation)
			{
				Starvation = 0f;
				IsStarving = false;
			}
			float satiation = Satiation;
			_info.Satiation = value;
			if (satiation == Satiation)
			{
				return;
			}
			StatChangedEvent onSatiationChanged = OnSatiationChanged;
			if (onSatiationChanged != null)
			{
				onSatiationChanged(_followerID, Satiation, satiation, Satiation - satiation);
			}
			if (satiation <= 30f && Satiation > 30f)
			{
				StatStateChangedEvent onSatiationStateChanged = OnSatiationStateChanged;
				if (onSatiationStateChanged != null)
				{
					onSatiationStateChanged(_followerID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			else if (satiation > 30f && Satiation <= 30f)
			{
				StatStateChangedEvent onSatiationStateChanged2 = OnSatiationStateChanged;
				if (onSatiationStateChanged2 != null)
				{
					onSatiationStateChanged2(_followerID, FollowerStatState.On, FollowerStatState.Off);
				}
			}
		}
	}

	public bool IsStarving
	{
		get
		{
			return _info.IsStarving;
		}
		set
		{
			_info.IsStarving = value;
		}
	}

	public float Starvation
	{
		get
		{
			return _info.Starvation;
		}
		set
		{
			float starvation = Starvation;
			_info.Starvation = value;
			if (starvation == Starvation)
			{
				return;
			}
			StatChangedEvent onStarvationChanged = OnStarvationChanged;
			if (onStarvationChanged != null)
			{
				onStarvationChanged(_followerID, Starvation, starvation, Starvation - starvation);
			}
			if (starvation < 75f && Starvation >= 75f)
			{
				Debug.Log("A ");
				if (_info.CursedState != Thought.Zombie)
				{
					Debug.Log("B ");
					if (!_info.DiedOfStarvation)
					{
						DataManager.Instance.LastFollowerToStarveToDeath = TimeManager.TotalElapsedGameTime;
						_info.DiedOfStarvation = true;
					}
				}
			}
			else if (starvation > 0f && Starvation <= 0f)
			{
				Debug.Log("C ");
				IsStarving = false;
				StatStateChangedEvent onStarvationStateChanged = OnStarvationStateChanged;
				if (onStarvationStateChanged != null)
				{
					onStarvationStateChanged(_followerID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
		}
	}

	public float Bathroom
	{
		get
		{
			return _info.Bathroom;
		}
		set
		{
			float bathroom = Bathroom;
			_info.Bathroom = value;
			if (bathroom == Bathroom)
			{
				return;
			}
			StatChangedEvent onBathroomChanged = OnBathroomChanged;
			if (onBathroomChanged != null)
			{
				onBathroomChanged(_followerID, Bathroom, bathroom, Bathroom - bathroom);
			}
			if (bathroom < 30f && Bathroom >= 30f)
			{
				StatStateChangedEvent onBathroomStateChanged = OnBathroomStateChanged;
				if (onBathroomStateChanged != null)
				{
					onBathroomStateChanged(_followerID, FollowerStatState.Urgent, FollowerStatState.On);
				}
			}
			else if (bathroom > 15f && Bathroom <= 15f)
			{
				StatStateChangedEvent onBathroomStateChanged2 = OnBathroomStateChanged;
				if (onBathroomStateChanged2 != null)
				{
					onBathroomStateChanged2(_followerID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			else if (bathroom <= 15f && Bathroom > 15f)
			{
				StatStateChangedEvent onBathroomStateChanged3 = OnBathroomStateChanged;
				if (onBathroomStateChanged3 != null)
				{
					onBathroomStateChanged3(_followerID, FollowerStatState.On, FollowerStatState.Off);
				}
			}
		}
	}

	public float BathroomFillRate
	{
		get
		{
			return _info.BathroomFillRate;
		}
		set
		{
			_info.BathroomFillRate = value;
		}
	}

	public float Social
	{
		get
		{
			return _info.Social;
		}
		set
		{
			_info.Social = value;
		}
	}

	public float Vomit
	{
		get
		{
			return _info.Vomit;
		}
		set
		{
			_info.Vomit = value;
		}
	}

	public float TargetBathroom
	{
		get
		{
			return _info.TargetBathroom;
		}
		set
		{
			_info.TargetBathroom = value;
		}
	}

	public float Rest
	{
		get
		{
			return _info.Rest;
		}
		set
		{
			if ((followerBrain.ThoughtExists(Thought.WorkThroughNight) || followerBrain._directInfoAccess.Necklace == InventoryItem.ITEM_TYPE.Necklace_5) && value < Rest)
			{
				return;
			}
			float rest = Rest;
			_info.Rest = value;
			if (rest == Rest)
			{
				return;
			}
			StatChangedEvent onRestChanged = OnRestChanged;
			if (onRestChanged != null)
			{
				onRestChanged(_followerID, Rest, rest, Rest - rest);
			}
			if (rest > 20f && Rest <= 20f)
			{
				StatStateChangedEvent onRestStateChanged = OnRestStateChanged;
				if (onRestStateChanged != null)
				{
					onRestStateChanged(_followerID, FollowerStatState.On, FollowerStatState.Off);
				}
			}
			else if (rest <= 20f && Rest > 20f)
			{
				StatStateChangedEvent onRestStateChanged2 = OnRestStateChanged;
				if (onRestStateChanged2 != null)
				{
					onRestStateChanged2(_followerID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			else if (rest > 0f && Rest <= 0f)
			{
				StatStateChangedEvent onRestStateChanged3 = OnRestStateChanged;
				if (onRestStateChanged3 != null)
				{
					onRestStateChanged3(_followerID, FollowerStatState.Urgent, FollowerStatState.On);
				}
			}
			else if (rest <= 0f && Rest > 0f)
			{
				StatStateChangedEvent onRestStateChanged4 = OnRestStateChanged;
				if (onRestStateChanged4 != null)
				{
					onRestStateChanged4(_followerID, FollowerStatState.On, FollowerStatState.Urgent);
				}
			}
		}
	}

	public List<StructureAndTime> ReactionsAndTime
	{
		get
		{
			return _info.ReactionsAndTime;
		}
	}

	public float LastVomit
	{
		get
		{
			return _info.LastVomit;
		}
		set
		{
			_info.LastVomit = value;
		}
	}

	public int LastHeal
	{
		get
		{
			return _info.LastHeal;
		}
		set
		{
			_info.LastHeal = value;
		}
	}

	public float Exhaustion
	{
		get
		{
			return _info.Exhaustion;
		}
		set
		{
			float exhaustion = Exhaustion;
			_info.Exhaustion = value;
			if (exhaustion == Exhaustion)
			{
				return;
			}
			StatChangedEvent onExhaustionChanged = OnExhaustionChanged;
			if (onExhaustionChanged != null)
			{
				onExhaustionChanged(_followerID, Exhaustion, exhaustion, Exhaustion - exhaustion);
			}
			if (exhaustion > 0f && Exhaustion <= 0f)
			{
				StatStateChangedEvent onExhaustionStateChanged = OnExhaustionStateChanged;
				if (onExhaustionStateChanged != null)
				{
					onExhaustionStateChanged(followerBrain.Info.ID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			if (exhaustion <= 0f && Exhaustion > 0f)
			{
				StatStateChangedEvent onExhaustionStateChanged2 = OnExhaustionStateChanged;
				if (onExhaustionStateChanged2 != null)
				{
					onExhaustionStateChanged2(followerBrain.Info.ID, FollowerStatState.On, FollowerStatState.Off);
				}
			}
		}
	}

	public float Illness
	{
		get
		{
			return _info.Illness;
		}
		set
		{
			float illness = Illness;
			_info.Illness = value;
			if (illness != Illness)
			{
				StatChangedEvent onIllnessChanged = OnIllnessChanged;
				if (onIllnessChanged != null)
				{
					onIllnessChanged(_followerID, Illness, illness, Illness - illness);
				}
				if (illness < 100f && Illness >= 100f)
				{
					followerBrain.DiedOfIllness = DataManager.Instance.OnboardingFinished;
				}
			}
		}
	}

	public float DissentGold
	{
		get
		{
			return _info.DissentGold;
		}
		set
		{
			_info.DissentGold = value;
		}
	}

	public bool GivenDissentWarning
	{
		get
		{
			return _info.GivenDissentWarning;
		}
		set
		{
			_info.GivenDissentWarning = value;
		}
	}

	public float Reeducation
	{
		get
		{
			return _info.Reeducation;
		}
		set
		{
			if (value != _info.Reeducation)
			{
				StatChangedEvent onReeducationChanged = OnReeducationChanged;
				if (onReeducationChanged != null)
				{
					onReeducationChanged(_info.ID, value, _info.Reeducation, value - _info.Reeducation);
				}
			}
			if (_info.Reeducation > 0f && value <= 0f)
			{
				_info.Reeducation = value;
				Action onReeducationComplete = OnReeducationComplete;
				if (onReeducationComplete != null)
				{
					onReeducationComplete();
				}
				if (followerBrain.Info.CursedState == Thought.Dissenter)
				{
					CultFaithManager.AddThought(Thought.Cult_CureDissenter, followerBrain.Info.ID, 1f);
				}
				followerBrain.RemoveCurseState(Thought.Dissenter);
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CureDissenter, followerBrain.Info.ID);
				StatStateChangedEvent onReeducationStateChanged = OnReeducationStateChanged;
				if (onReeducationStateChanged != null)
				{
					onReeducationStateChanged(_info.ID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			_info.Reeducation = value;
		}
	}

	public int InteractionCoolDownFasting
	{
		get
		{
			return _info.InteractionCoolDownFasting;
		}
		set
		{
			_info.InteractionCoolDownFasting = value;
		}
	}

	public int InteractionCoolDownEnergizing
	{
		get
		{
			return _info.InteractionCoolDownEnergizing;
		}
		set
		{
			_info.InteractionCoolDownEnergizing = value;
		}
	}

	public int InteractionCoolDownMotivational
	{
		get
		{
			return _info.InteractionCoolDownMotivational;
		}
		set
		{
			_info.InteractionCoolDownMotivational = value;
		}
	}

	public int InteractionCoolDemandDevotion
	{
		get
		{
			return _info.InteractionCoolDemandDevotion;
		}
		set
		{
			_info.InteractionCoolDemandDevotion = value;
		}
	}

	public static bool Fasting
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFastDeclared < 3600f;
		}
	}

	public static bool BrainWashed
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastBrainwashed < 3600f;
		}
	}

	public static bool IsHoliday
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastHolidayDeclared < 1200f;
		}
	}

	public static bool IsWorkThroughTheNight
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastWorkThroughTheNight < 3600f;
		}
	}

	public static bool IsConstruction
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastConstruction < 3600f;
		}
	}

	public static bool IsEnlightened
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastEnlightenment < 3600f;
		}
	}

	public static bool IsFishing
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFishingDeclared < 3600f;
		}
	}

	public static bool IsBloodMoon
	{
		get
		{
			return TimeManager.TotalElapsedGameTime - DataManager.Instance.LastHalloween < 3600f;
		}
	}

	public int GuaranteedGoodInteractionsUntil
	{
		get
		{
			return _info.GuaranteedGoodInteractionsUntil;
		}
		set
		{
			_info.GuaranteedGoodInteractionsUntil = value;
		}
	}

	public bool GuaranteedGoodInteractions
	{
		get
		{
			return _info.GuaranteedGoodInteractionsUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public int IncreasedDevotionOutputUntil
	{
		get
		{
			return _info.IncreasedDevotionOutputUntil;
		}
		set
		{
			_info.IncreasedDevotionOutputUntil = value;
		}
	}

	public bool IncreasedDevotionOutput
	{
		get
		{
			return _info.IncreasedDevotionOutputUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public int MotivatedUntil
	{
		get
		{
			return _info.MotivatedUntil;
		}
		private set
		{
			bool motivated = Motivated;
			_info.MotivatedUntil = value;
			if (motivated != Motivated)
			{
				StatusChangedEvent onMotivatedChanged = OnMotivatedChanged;
				if (onMotivatedChanged != null)
				{
					onMotivatedChanged(_followerID);
				}
			}
		}
	}

	public bool Motivated
	{
		get
		{
			return _info.MotivatedUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public int LastBlessing
	{
		get
		{
			return _info.LastBlessing;
		}
		set
		{
			_info.LastBlessing = value;
		}
	}

	public bool BlessedToday
	{
		get
		{
			return _info.LastBlessing == DataManager.Instance.CurrentDayIndex;
		}
	}

	public int CachedLumber
	{
		get
		{
			return _info.CachedLumber;
		}
		set
		{
			_info.CachedLumber = value;
		}
	}

	public int CachedLumberjackStationID
	{
		get
		{
			return _info.CachedLumberjackStationID;
		}
		set
		{
			_info.CachedLumberjackStationID = value;
		}
	}

	public FollowerBrainStats(FollowerInfo info, FollowerBrain followerBrain)
	{
		_followerID = info.ID;
		this.followerBrain = followerBrain;
		_info = info;
	}

	public void OnThoughtsChanged(FollowerBrain brain, float OldValue, float NewValue)
	{
	}

	public void Brainwash(FollowerBrain Brain)
	{
		Brain.AddThought(Thought.Brainwashed);
	}

	public void Motivate(int durationDays)
	{
		int b = DataManager.Instance.CurrentDayIndex + (durationDays - 1);
		MotivatedUntil = Mathf.Max(MotivatedUntil, b);
	}

	public int GetDevotionGeneration()
	{
		return 0;
	}
}
