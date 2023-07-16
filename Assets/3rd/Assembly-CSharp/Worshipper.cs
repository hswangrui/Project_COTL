using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(WorshipperInfoManager))]
public class Worshipper : TaskDoer
{
	public static List<Worshipper> worshippers = new List<Worshipper>();

	public SkeletonAnimation Spine;

	private GameObject NameOnHUD;

	private SimpleInventory inventory;

	public Interaction_AwaitRecruit interaction_AwaitRecruit;

	public ParticleSystem NewJobParticles;

	private Task WorshipLeaderTask;

	public bool GoToAndStopping;

	private Task GoToAndStopTask;

	private System.Action GoToAndCallback;

	private GameObject LookToObject;

	private GameObject GoToAndStopTargetPosition;

	public bool GivenPreachSoul;

	public bool BlessedToday;

	public bool BeenFed;

	private float HungerComplaint;

	public float Effeciency = 1f;

	public WorshipperInfoManager wim;

	[HideInInspector]
	public WorshipperBubble bubble;

	private float FacingAngle;

	private float Delay;

	private float Timer;

	private Vector3 TargetPosition;

	public bool BeingCarried;

	private ThrowWorshipper throwWorshipper;

	private List<string> EmotionalSpectrum = new List<string> { "Emotions/emotion-angry", "Emotions/emotion-unhappy", "Emotions/emotion-nomal", "Emotions/emotion-happy" };

	private float TimedTimer;

	private System.Action TimedCallBack;

	public SimpleSpineAnimator simpleAnimator;

	private float TIME_END_OF_DAY = 1f;

	private float TIME_MORNING = 0.03f;

	public bool MORNING_ASLEEP = true;

	public bool EATEN_DINNNER;

	public bool TRAPPED;

	private float YellTimer;

	private bool _InRitual;

	private float AssignParticles;

	public float Faith
	{
		get
		{
			return wim.v_i.Faith;
		}
		set
		{
			if (value != wim.v_i.Faith)
			{
				UITextPopUp.Create(((value > Faith) ? "+" : "") + (value - Faith), (value > Faith) ? Color.green : Color.red, base.gameObject, new Vector3(0f, 2f));
			}
			wim.v_i.Faith = value;
		}
	}

	public float FearLove
	{
		get
		{
			return wim.v_i.FearLove;
		}
		set
		{
			if (value != wim.v_i.FearLove)
			{
				UITextPopUp.Create(((value > FearLove) ? "+" : "") + (value - FearLove), (value > FearLove) ? Color.green : Color.red, base.gameObject, new Vector3(0f, 2f));
			}
			wim.v_i.FearLove = Mathf.Clamp(value, 0f, 100f);
		}
	}

	public bool GuaranteedGoodInteractions
	{
		get
		{
			return wim.v_i.GuaranteedGoodInteractionsUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public bool Motivated
	{
		get
		{
			return wim.v_i.MotivatedUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public float Sleep
	{
		get
		{
			return wim.v_i.Sleep;
		}
		set
		{
			wim.v_i.Sleep = Mathf.Max(0f, value);
		}
	}

	public float Hunger
	{
		get
		{
			return wim.v_i.Hunger;
		}
		set
		{
			wim.v_i.Hunger = Mathf.Clamp(value, -100f, 100f);
		}
	}

	public new virtual Task CurrentTask
	{
		get
		{
			return _CurrentTask;
		}
		set
		{
			StopAllCoroutines();
			if (_CurrentTask != value && _CurrentTask != null)
			{
				_CurrentTask.ClearTask();
			}
			_CurrentTask = value;
		}
	}

	public bool InRitual
	{
		get
		{
			return _InRitual;
		}
		set
		{
			if (value)
			{
				if (CurrentTask != null)
				{
					CurrentTask.ClearTask();
				}
				CurrentTask = null;
				ClearPaths();
			}
			_InRitual = value;
		}
	}

	private void OnDissentor(Villager_Info.StatusState State)
	{
		switch (State)
		{
		case Villager_Info.StatusState.On:
			if (CurrentTask == null || CurrentTask.Type != Task_Type.DISSENTER)
			{
				CurrentTask = new Task_Dissenter();
				CurrentTask.StartTask(this, null);
			}
			break;
		case Villager_Info.StatusState.Off:
			if (CurrentTask != null && CurrentTask.Type == Task_Type.DISSENTER)
			{
				CurrentTask = null;
			}
			break;
		}
	}

	private void OnIllness(Villager_Info.StatusState Status)
	{
		switch (Status)
		{
		case Villager_Info.StatusState.On:
			if (CurrentTask == null || CurrentTask.Type != Task_Type.ILL)
			{
				CurrentTask = new Task_Ill();
				CurrentTask.StartTask(this, null);
			}
			break;
		case Villager_Info.StatusState.Off:
			if (CurrentTask != null && CurrentTask.Type == Task_Type.ILL)
			{
				CurrentTask = null;
			}
			break;
		case Villager_Info.StatusState.Kill:
			DieFromIllness();
			break;
		}
	}

	private void OnStarve(Villager_Info.StatusState Status)
	{
		switch (Status)
		{
		case Villager_Info.StatusState.Off:
			if (CurrentTask != null && CurrentTask.Type == Task_Type.ILL)
			{
				CurrentTask = null;
			}
			break;
		case Villager_Info.StatusState.Kill:
			TimedAnimation("tantrum-hungry", 3.2f, DieFromStarvation);
			break;
		case Villager_Info.StatusState.On:
			break;
		}
	}

	private void DissentorUp()
	{
		wim.v_i.Dissentor += 10f;
	}

	private void DissentorDown()
	{
		wim.v_i.Dissentor -= 10f;
	}

	private void IllnessUp()
	{
		wim.v_i.Illness += 10f;
	}

	private void IllnessDown()
	{
		wim.v_i.Illness -= 10f;
	}

	private void DebugCurrentTask()
	{
		Debug.Log(string.Concat("current task: ", CurrentTask, "  ", (CurrentTask != null) ? CurrentTask.Type.ToString() : ""));
	}

	private void DebugCurrentState()
	{
		Debug.Log("Current state " + state.CURRENT_STATE);
	}

	private void DieFromStarvation()
	{
		Die();
	}

	private void DieFromIllness()
	{
		Die();
	}

	public void Die()
	{
		CurrentTask = null;
		((GameObject)null).GetComponent<Structure>();
		DeadWorshipper component = ((GameObject)null).GetComponent<DeadWorshipper>();
		component.StructureInfo.Dir = simpleAnimator.Dir;
		component.PlayAnimation = true;
		ClearDwelling(this);
		ClearJob(this);
		wim.v_i.Faith = 0f;
		wim.v_i.Starve = 0f;
		wim.OnDie();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void StartWorship(GameObject TargetObject)
	{
		if (CurrentTask != null && CurrentTask.Type == Task_Type.NONE)
		{
			CurrentTask.ClearTask();
		}
		WorshipLeaderTask = new Task_WorshipLeader();
		WorshipLeaderTask.StartTask(this, TargetObject);
	}

	public void EndWorship()
	{
		WorshipLeaderTask.ClearTask();
		WorshipLeaderTask = null;
	}

	public override void Update()
	{
		wim.v_i.IncreaseDevotion(1f);
		if (!InConversation)
		{
			if (CurrentTask == null || CurrentTask.Type != Task_Type.IMPRISONED)
			{
				if (state.CURRENT_STATE != StateMachine.State.Sleeping)
				{
					Sleep -= Time.deltaTime;
				}
				if (!wim.v_i.Fasting)
				{
					wim.v_i.DecreaseHunger(1f);
					if (Hunger <= -60f)
					{
						wim.v_i.Starve += Time.deltaTime;
					}
				}
			}
			if (CurrentTask != null && CurrentTask.Type == Task_Type.IMPRISONED)
			{
				wim.v_i.Dissentor -= Time.deltaTime;
			}
		}
		if (AssignParticles > 0f && (AssignParticles -= Time.deltaTime) <= 0f)
		{
			NewJobParticles.Stop();
		}
		if (GoToAndStopping)
		{
			GoToAndStopTask.TaskUpdate();
			base.Update();
			return;
		}
		if (InRitual && state.CURRENT_STATE != StateMachine.State.TimedAction)
		{
			base.Update();
			return;
		}
		if (WorshipLeaderTask != null)
		{
			WorshipLeaderTask.TaskUpdate();
			base.Update();
			return;
		}
		if (state.CURRENT_STATE == StateMachine.State.TimedAction)
		{
			TimedTimer -= Time.deltaTime;
			if (!(TimedTimer < 0f))
			{
				base.Update();
				return;
			}
			if (TimedCallBack != null)
			{
				TimedCallBack();
				return;
			}
		}
		if (state.CURRENT_STATE == StateMachine.State.AwaitRecruit)
		{
			speed = 0f;
			moveVX = 0f;
			moveVY = 0f;
			vx = 0f;
			vy = 0f;
		}
		else if (state.CURRENT_STATE == StateMachine.State.SpawnIn)
		{
			state.Timer += Time.deltaTime;
			float num = 1f;
		}
		else if (state.CURRENT_STATE == StateMachine.State.SpawnOut)
		{
			if ((state.Timer += Time.deltaTime) > 2f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (state.CURRENT_STATE == StateMachine.State.BeingCarried || state.CURRENT_STATE == StateMachine.State.PickedUp || state.CURRENT_STATE == StateMachine.State.SacrificeRecruit || state.CURRENT_STATE == StateMachine.State.Recruited)
			{
				return;
			}
			if (TRAPPED && (YellTimer += Time.deltaTime) > 12f)
			{
				YellTimer = 0f;
				bubble.Play(WorshipperBubble.SPEECH_TYPE.HELP);
			}
			if (CurrentTask == null)
			{
				DoWorkOrWonder();
			}
			else
			{
				if (Sleep <= 0f && !InConversation && CurrentTask.Type == Task_Type.NONE)
				{
					SleepAtDwellingOrWonder();
				}
				else if (Hunger <= 0f && !InConversation && CurrentTask.Type == Task_Type.NONE && !wim.v_i.Fasting)
				{
					if (!wim.v_i.Complaint_Food)
					{
						bubble.Play(WorshipperBubble.SPEECH_TYPE.FOOD);
						TimedAnimation("tantrum-hungry", 3.2f, BackToIdle);
						wim.v_i.Complaint_Food = true;
					}
					else if ((HungerComplaint += Time.deltaTime) > 600f)
					{
						bubble.Play(WorshipperBubble.SPEECH_TYPE.FOOD);
						TimedAnimation("tantrum-hungry", 3.2f, BackToIdle);
					}
				}
				CurrentTask.TaskUpdate();
			}
			base.Update();
		}
	}

	public void GoToAndStop(GameObject TargetPosition, System.Action GoToAndCallback, GameObject LookToObject, bool ClearCurrentTaskAfterGoToAndStop)
	{
		if (GoToAndStopTask != null)
		{
			GoToAndStopTask.ClearTask();
		}
		ClearPaths();
		GoToAndStopTargetPosition = TargetPosition;
		GoToAndStopTask = new Task_GoToAndStop();
		GoToAndStopTask.StartTask(this, TargetPosition);
		((Task_GoToAndStop)GoToAndStopTask).ClearCurrentTaskAfterGoToAndStop = ClearCurrentTaskAfterGoToAndStop;
		GoToAndStopping = true;
		this.GoToAndCallback = GoToAndCallback;
		this.LookToObject = LookToObject;
	}

	public void EndGoToAndStop()
	{
		if (GoToAndStopTargetPosition != null)
		{
			base.transform.position = GoToAndStopTargetPosition.transform.position;
		}
		if (GoToAndStopTask != null)
		{
			GoToAndStopTask.ClearTask();
		}
		GoToAndStopTask = null;
		GoToAndStopping = false;
		if (GoToAndCallback != null)
		{
			GoToAndCallback();
		}
		if (LookToObject != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, LookToObject.transform.position);
		}
	}

	public void Pray()
	{
		state.CURRENT_STATE = StateMachine.State.Worshipping;
	}

	public void BackToIdle()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public void Inactive()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
	}

	public void CrowdWorship()
	{
		state.CURRENT_STATE = StateMachine.State.CrowdWorship;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		worshippers.Add(this);
		NewJobParticles.Stop();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		CurrentTask = null;
		Villager_Info v_i = wim.v_i;
		v_i.OnDissenter = (Villager_Info.StatusEffectEvent)Delegate.Remove(v_i.OnDissenter, new Villager_Info.StatusEffectEvent(OnDissentor));
		Villager_Info v_i2 = wim.v_i;
		v_i2.OnIllness = (Villager_Info.StatusEffectEvent)Delegate.Remove(v_i2.OnIllness, new Villager_Info.StatusEffectEvent(OnIllness));
		Villager_Info v_i3 = wim.v_i;
		v_i3.OnStarve = (Villager_Info.StatusEffectEvent)Delegate.Remove(v_i3.OnStarve, new Villager_Info.StatusEffectEvent(OnStarve));
	}

	public override void OnDisable()
	{
		base.OnDisable();
		worshippers.Remove(this);
		if (NameOnHUD != null)
		{
			UnityEngine.Object.Destroy(NameOnHUD);
		}
	}

	private void Start()
	{
		Delay = UnityEngine.Random.Range(0f, 3f);
		Spine = GetComponentInChildren<SkeletonAnimation>();
		inventory = GetComponent<SimpleInventory>();
		bubble = GetComponentInChildren<WorshipperBubble>();
		simpleAnimator = GetComponentInChildren<SimpleSpineAnimator>();
		simpleAnimator.AnimationTrack = 1;
		Spine.AnimationState.Start += SetEmotionAnimation;
		interaction_AwaitRecruit.enabled = false;
		wim = GetComponent<WorshipperInfoManager>();
		Villager_Info v_i = wim.v_i;
		v_i.OnDissenter = (Villager_Info.StatusEffectEvent)Delegate.Combine(v_i.OnDissenter, new Villager_Info.StatusEffectEvent(OnDissentor));
		Villager_Info v_i2 = wim.v_i;
		v_i2.OnIllness = (Villager_Info.StatusEffectEvent)Delegate.Combine(v_i2.OnIllness, new Villager_Info.StatusEffectEvent(OnIllness));
		Villager_Info v_i3 = wim.v_i;
		v_i3.OnStarve = (Villager_Info.StatusEffectEvent)Delegate.Combine(v_i3.OnStarve, new Villager_Info.StatusEffectEvent(OnStarve));
		if (wim.v_i.Faith < 40f && wim.v_i.Dissentor < Villager_Info.DissentorThreshold && UnityEngine.Random.Range(0, 3) == 0)
		{
			wim.v_i.Dissentor = 100f;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		CameraManager.shakeCamera(0.1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		health.HP = health.totalHP;
		FearLove--;
		wim.v_i.Dissentor -= 20f;
		simpleAnimator.FlashRedTint();
		if (AttackLocation.x > base.transform.position.x)
		{
			Spine.AnimationState.SetAnimation(2, "hurt-front", false);
		}
		else
		{
			Spine.AnimationState.SetAnimation(2, "hurt-back", false);
		}
		Spine.AnimationState.Complete += Complete;
		wim.v_i.MotivatedUntil = DataManager.Instance.CurrentDayIndex;
		base.OnHit(Attacker, AttackLocation, AttackType);
	}

	private void Complete(TrackEntry trackEntry)
	{
		if (trackEntry.TrackIndex == 2)
		{
			Spine.AnimationState.SetEmptyAnimation(2, 0.1f);
			Spine.AnimationState.Complete -= Complete;
		}
	}

	public void ShowStats()
	{
	}

	public void HideStats()
	{
	}

	public void PickUp()
	{
		inventory.DropItem();
		BeingCarried = true;
		CurrentTask = null;
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.PickedUp;
		if (wim.v_i.FearLove <= Villager_Info.FearThreshold)
		{
			SetAnimation("picked-up-hate", true);
		}
		else if (wim.v_i.FearLove >= Villager_Info.LoveThreshold)
		{
			SetAnimation("picked-up-love", true);
		}
		else
		{
			SetAnimation("picked-up", true);
		}
		MORNING_ASLEEP = false;
		ShowStats();
	}

	public IEnumerator LowerZ()
	{
		float Timer = 0f;
		Vector3 position;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.2f))
			{
				break;
			}
			position = base.transform.position;
			position.z = 0f;
			base.transform.position = Vector3.Lerp(base.transform.position, position, Timer / 0.1f);
			yield return null;
		}
		position = base.transform.position;
		position.z = 0f;
		base.transform.position = position;
	}

	public void DropMe()
	{
		Vector3 position = base.transform.position;
		position.z = 0f;
		base.transform.position = position;
		TimedAnimation("put-down", 0.3f, Dropped);
		HideStats();
	}

	private void Dropped()
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.Idle;
		BeingCarried = false;
		TRAPPED = false;
	}

	public void RecoverFromThrow()
	{
		TimedAnimation("put-down", 0.3f, RecoveredFromThrow);
	}

	private void RecoveredFromThrow()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		BeingCarried = false;
	}

	public void ThrowMe(float Direction)
	{
		throwWorshipper = GetComponent<ThrowWorshipper>();
		throwWorshipper.FacingAngle = Direction;
		throwWorshipper.enabled = true;
		base.enabled = false;
		Spine.AnimationState.SetAnimation(0, "thrown", true);
		TRAPPED = false;
		ClearJob(this);
		FearLove -= 3f;
		HideStats();
	}

	public void GiveItem(InventoryItem.ITEM_TYPE itemType)
	{
		Vector3 position = base.transform.position;
		position.z = 0f;
		base.transform.position = position;
		TimedAnimation("put-down", 0.3f, delegate
		{
			TimedAnimation("cheer", 1f, delegate
			{
				Dropped();
			});
		});
		HideStats();
	}

	public void CapturedByBigSpider()
	{
		Spine.AnimationState.SetAnimation(0, "spider", true);
		BeingCarried = false;
		TRAPPED = true;
	}

	public void FreeFromHive()
	{
		DropMe();
		bubble.Play(WorshipperBubble.SPEECH_TYPE.LOVE);
	}

	private void SetEmotionAnimation(TrackEntry trackEntry)
	{
		if (trackEntry.TrackIndex == 1)
		{
			if (wim.v_i.Brainwashed)
			{
				Spine.AnimationState.SetAnimation(0, "Emotions/emotion-enlightened", true);
			}
			else if (wim.v_i.Dissentor >= Villager_Info.DissentorThreshold)
			{
				Spine.AnimationState.SetAnimation(0, "Emotions/emotion-dissenter", true);
			}
			else if (wim.v_i.Illness >= Villager_Info.IllnessThreshold)
			{
				Spine.AnimationState.SetAnimation(0, "Emotions/emotion-sick", true);
			}
			else
			{
				Spine.AnimationState.SetAnimation(0, "Emotions/emotion-normal", true);
			}
		}
	}

	public void TimedAnimation(string Animation, float Timer, System.Action Callback)
	{
		state.CURRENT_STATE = StateMachine.State.TimedAction;
		Spine.AnimationState.SetAnimation(1, Animation, true);
		TimedTimer = Timer;
		TimedCallBack = Callback;
	}

	public void SetAnimation(string Animation, bool Loop)
	{
		StartCoroutine(SetAnimationAtEndOfFrame(Animation, Loop));
	}

	private IEnumerator SetAnimationAtEndOfFrame(string Animation, bool Loop)
	{
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(1, Animation, Loop);
	}

	public void AddAnimation(string Animation, bool Loop)
	{
		StartCoroutine(AddAnimationAtEndOfFrame(Animation, Loop));
	}

	private IEnumerator AddAnimationAtEndOfFrame(string Animation, bool Loop)
	{
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.AddAnimation(1, Animation, Loop, 0f);
	}

	public void ChangeStateAnimation(StateMachine.State s, string NewAnimation)
	{
		simpleAnimator.ChangeStateAnimation(s, NewAnimation);
	}

	public void ResetAnimationsToDefaults()
	{
		simpleAnimator.ResetAnimationsToDefaults();
	}

	private void SleepAtDwellingOrWonder()
	{
		Sleep = 60f;
		bubble.Play(WorshipperBubble.SPEECH_TYPE.HOME);
		TimedAnimation("tantrum", 3.2f, BackToIdle);
		wim.v_i.Complaint_House = true;
	}

	private void DoWorkOrWonder()
	{
		WorkPlace workPlaceByID = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
		if (wim.v_i.WorkPlace != WorkPlace.NO_JOB)
		{
			workPlaceByID.BeginJob(this, wim.v_i.WorkPlaceSlot);
			CurrentTask = Task.GetTaskByType(workPlaceByID.JobType);
			CurrentTask.StartTask(this, null);
		}
		else if (wim.v_i.Illness >= Villager_Info.IllnessThreshold)
		{
			if (CurrentTask == null || CurrentTask.Type != Task_Type.ILL)
			{
				CurrentTask = new Task_Ill();
				CurrentTask.StartTask(this, null);
			}
		}
		else
		{
			CurrentTask = new Task_IdleWorhipper();
			CurrentTask.StartTask(this, null);
		}
	}

	private void Repath()
	{
		if ((Timer += Time.deltaTime) > 1f)
		{
			Timer = 0f;
			givePath(TargetPosition);
		}
	}

	public static Worshipper GetAvailableWorshipper()
	{
		foreach (Worshipper worshipper in worshippers)
		{
			if (!worshipper.InConversation && worshipper.CurrentTask != null && (worshipper.CurrentTask.Type == Task_Type.NONE || worshipper.CurrentTask.Type == Task_Type.SLEEP))
			{
				return worshipper;
			}
		}
		return null;
	}

	public static List<Worshipper> GetAllAvailableWorshipper(bool IgnoreConversation = false)
	{
		List<Worshipper> list = new List<Worshipper>();
		foreach (Worshipper worshipper in worshippers)
		{
			if ((!worshipper.InConversation || IgnoreConversation) && worshipper.CurrentTask != null && (worshipper.CurrentTask.Type == Task_Type.NONE || worshipper.CurrentTask.Type == Task_Type.SLEEP))
			{
				list.Add(worshipper);
			}
		}
		return list;
	}

	public void AssignJob(string WorkPlaceID, int WorkPlaceSlot)
	{
		if (wim.v_i.WorkPlace != WorkPlace.NO_JOB)
		{
			ClearJob(wim.v_i.WorkPlace, wim.v_i.WorkPlaceSlot);
		}
		wim.v_i.WorkPlace = WorkPlaceID;
		wim.v_i.WorkPlaceSlot = WorkPlaceSlot;
		MORNING_ASLEEP = false;
		PlayNewAssignParticles();
	}

	private void PlayNewAssignParticles()
	{
		NewJobParticles.Play();
		AssignParticles = 1f;
	}

	public static void ClearJob(string workplace, int workplaceslot)
	{
		foreach (Worshipper worshipper in worshippers)
		{
			if (worshipper.wim.v_i.WorkPlace == workplace && worshipper.wim.v_i.WorkPlaceSlot == workplaceslot)
			{
				worshipper.wim.v_i.WorkPlace = WorkPlace.NO_JOB;
				worshipper.state.CURRENT_STATE = StateMachine.State.Idle;
				worshipper.Delay = 0f;
				worshipper.CurrentTask = null;
			}
		}
	}

	public static void ClearJob(Worshipper w)
	{
		w.wim.v_i.WorkPlace = WorkPlace.NO_JOB;
		w.state.CURRENT_STATE = StateMachine.State.Idle;
		w.Delay = 0f;
		w.CurrentTask = null;
	}

	public void AssignDwelling(Dwelling dwelling, int dwellingslot)
	{
		ClearDwelling(this);
		wim.v_i.DwellingSlot = dwellingslot;
		wim.v_i.DwellingClaimed = false;
		dwelling.SetBedImage(dwellingslot, Dwelling.SlotState.CLAIMED);
		MORNING_ASLEEP = true;
		if (bubble != null)
		{
			bubble.Play(WorshipperBubble.SPEECH_TYPE.LOVE);
		}
		PlayNewAssignParticles();
	}

	public static void ClearDwelling(string dwelling, int dwellingslot)
	{
	}

	public static void ClearDwelling(Worshipper w)
	{
	}

	public static Worshipper GetWorshipperByID(int ID)
	{
		foreach (Worshipper worshipper in worshippers)
		{
			if (worshipper.wim.v_i.ID == ID)
			{
				return worshipper;
			}
		}
		return null;
	}

	public static Worshipper GetWorshipperByInfo(Villager_Info v_i)
	{
		foreach (Worshipper worshipper in worshippers)
		{
			if (worshipper.wim.v_i == v_i)
			{
				return worshipper;
			}
		}
		return null;
	}

	public static Worshipper GetWorshipperByJobID(string ID)
	{
		foreach (Worshipper worshipper in worshippers)
		{
			if (worshipper.wim.v_i != null && worshipper.wim.v_i.WorkPlace == ID)
			{
				return worshipper;
			}
		}
		return null;
	}

	public static Worshipper GetWorshipperByDwellingID(string ID)
	{
		foreach (Worshipper worshipper in worshippers)
		{
			if (worshipper.wim.v_i != null && worshipper.wim.v_i.Dwelling == ID)
			{
				return worshipper;
			}
		}
		return null;
	}
}
