using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Interaction_FollowerSpawn : Interaction
{
	public bool DisableOnHighlighted;

	private FollowerInfo v_i;

	private FollowerInfoManager wim;

	private StateMachine RecruitState;

	public SkeletonAnimation Spine;

	[SerializeField]
	private SkeletonAnimation portalSpine;

	public ParticleSystem recruitParticles;

	private string ForceSkin = "";

	public FollowerInfo _followerInfo;

	private FollowerOutfit _outfit;

	private LayerMask collisionMask;

	private string q1Title = "Interactions/FollowerSpawn/Convert/Title";

	private string q1Description = "Interactions/FollowerSpawn/Convert/Description";

	private string q2Title = "Interactions/FollowerSpawn/Consume/Title";

	private string q2Description = "Interactions/FollowerSpawn/Consume/Description";

	private Thought cursedState;

	private EventInstance receiveLoop;

	public Action followerInfoAssigned;

	public Material NormalMaterial;

	public Material BW_Material;

	private EventInstance LoopInstance;

	public AnimationCurve absorbSoulCurve;

	protected string sRescue;

	[HideInInspector]
	public int Cost;

	private bool Activated;

	public SkeletonAnimation PortalSpine
	{
		get
		{
			return portalSpine;
		}
	}

	protected virtual void Start()
	{
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Island"));
		RecruitState = GetComponent<StateMachine>();
	}

	private IEnumerator WaitForFollowerToStopMoving(float Duration)
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		GetLabel();
		yield return new WaitForSeconds(Duration);
		GameManager.GetInstance().OnConversationEnd();
		Interactable = true;
		GetLabel();
	}

	public override void IndicateHighlighted()
	{
		if (!DisableOnHighlighted)
		{
			base.IndicateHighlighted();
		}
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
	}

	public void Play(string ForceSkin = "Boss Mama Maggot", string ForceName = "", bool animate = true, Thought cursedState = Thought.None)
	{
		StopAllCoroutines();
		this.cursedState = cursedState;
		if (animate)
		{
			Interactable = false;
			float duration = Spine.AnimationState.SetAnimation(0, "transform", false).Animation.Duration;
			StartCoroutine(WaitForFollowerToStopMoving(duration));
		}
		Spine.AnimationState.AddAnimation(0, "unconverted", true, 0f);
		this.ForceSkin = ForceSkin;
		wim = GetComponent<FollowerInfoManager>();
		wim.ForceSkin = true;
		wim.ForceOutfitSkin = true;
		wim.ForceSkinOverride = ForceSkin;
		wim.ForceOutfitSkinOverride = "Clothes/Rags";
		if (Vector3.Distance(base.transform.position, Vector3.zero) > 4.5f)
		{
			Vector3 vector = (Vector3.zero - base.transform.position) * 0.5f;
			base.transform.DOMove(base.transform.position + vector, 0.3f);
		}
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(ForceSkin);
		if (colourData != null)
		{
			foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[0].SlotAndColours)
			{
				wim.SetSlotColour(slotAndColour.Slot, slotAndColour.color);
			}
		}
		wim.NewV_I();
		UpdateLocalisation();
		_followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base, ForceSkin);
		if (_followerInfo.SkinName == "Giraffe")
		{
			ForceName = LocalizationManager.GetTranslation("FollowerNames/Sparkles");
		}
		if (ForceName != "")
		{
			_followerInfo.Name = ForceName;
		}
		ActivateDistance = 3f;
		if (ForceName == ScriptLocalization.NAMES.DeathNPC)
		{
			_followerInfo.ID = FollowerManager.DeathCatID;
			FollowerBrain.GetOrCreateBrain(_followerInfo).AddTrait(FollowerTrait.TraitType.Immortal);
		}
		else if (ForceName == ScriptLocalization.NAMES_CultLeaders.Dungeon1)
		{
			_followerInfo.ID = FollowerManager.LeshyID;
		}
		else if (ForceName == ScriptLocalization.NAMES_CultLeaders.Dungeon2)
		{
			_followerInfo.ID = FollowerManager.HeketID;
		}
		else if (ForceName == ScriptLocalization.NAMES_CultLeaders.Dungeon3)
		{
			_followerInfo.ID = FollowerManager.KallamarID;
		}
		else if (ForceName == ScriptLocalization.NAMES_CultLeaders.Dungeon4)
		{
			_followerInfo.ID = FollowerManager.ShamuraID;
		}
		if (FollowerManager.UniqueFollowerIDs.Contains(_followerInfo.ID))
		{
			_followerInfo.LifeExpectancy *= 2;
		}
		Action action = followerInfoAssigned;
		if (action != null)
		{
			action();
		}
	}

	private IEnumerator FollowerChoiceIE()
	{
		GameManager.GetInstance().AddPlayerToCamera();
		if (DataManager.Instance.FirstFollowerSpawnInteraction && (bool)MiniBossController.Instance)
		{
			List<ConversationEntry> list = new List<ConversationEntry>
			{
				new ConversationEntry(base.gameObject, "Conversation_NPC/FollowerSpawn/Line1"),
				new ConversationEntry(base.gameObject, "Conversation_NPC/FollowerSpawn/Line2")
			};
			list[0].CharacterName = MiniBossController.Instance.DisplayName;
			list[0].Zoom = 5f;
			list[0].SetZoom = true;
			list[1].CharacterName = MiniBossController.Instance.DisplayName;
			list[1].Zoom = 5f;
			list[1].SetZoom = true;
			list[0].DefaultAnimation = "unconverted";
			list[1].DefaultAnimation = "unconverted";
			list[0].Animation = "unconverted-talk";
			list[0].LoopAnimation = true;
			list[1].Animation = "unconverted-talk";
			list[1].LoopAnimation = true;
			foreach (ConversationEntry item in list)
			{
				item.soundPath = "event:/dialogue/followers/general_talk";
				item.pitchValue = _followerInfo.follower_pitch;
				item.vibratoValue = _followerInfo.follower_vibrato;
				item.followerID = _followerInfo.ID;
			}
			MMConversation.Play(new ConversationObject(list, null, null), false);
			MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
			DataManager.Instance.FirstFollowerSpawnInteraction = false;
		}
		Interactable = false;
		if (true)
		{
			yield return StartCoroutine(ConvertFollower());
		}
		else
		{
			yield return StartCoroutine(ConsumefollowerRoutine());
		}
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.75f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public IEnumerator ConsumefollowerRoutine(bool move = true)
	{
		if (state == null)
		{
			state = GetComponent<StateMachine>();
		}
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		if (move)
		{
			Vector3 vector = ((state.transform.position.x < base.transform.position.x) ? Vector3.left : Vector3.right);
			Vector3 targetPosition = base.transform.position + vector * 2f;
			PlayerFarming.Instance.GoToAndStop(targetPosition);
			while (PlayerFarming.Instance.GoToAndStopping)
			{
				yield return null;
			}
		}
		AudioManager.Instance.PlayOneShot("event:/followers/consume_start", base.gameObject);
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sacrifice-long", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		Spine.AnimationState.SetAnimation(0, "sacrifice-long", false);
		yield return new WaitForSeconds(0.1f);
		Spine.CustomMaterialOverride.Clear();
		Spine.CustomMaterialOverride.Add(NormalMaterial, BW_Material);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(0.33f, 0f, 1f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6f, 2f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().CamFollowTarget.targetDistance += 2f;
		LoopInstance = AudioManager.Instance.CreateLoop("event:/followers/consume_loop", base.gameObject, true);
		yield return new WaitForSeconds(3f);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.StopLoop(LoopInstance);
		AudioManager.Instance.PlayOneShot("event:/followers/consume_end", base.gameObject);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		Spine.CustomMaterialOverride.Clear();
		HUD_Manager.Instance.ShowBW(0.33f, 1f, 0f);
		PlayerFarming.Instance.GetXP(10f);
	}

	private IEnumerator SpawnSouls(Vector3 fromPosition, Vector3 targetPosition, float startingDelay, float min)
	{
		float delay = startingDelay;
		for (int i = 0; i < 30; i++)
		{
			float time = (float)i / 30f;
			delay = Mathf.Clamp(delay * (1f - absorbSoulCurve.Evaluate(time)), min, float.MaxValue);
			SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, fromPosition, Color.red, null, 0.2f, 100f * (1f + absorbSoulCurve.Evaluate(time)));
			yield return new WaitForSeconds(delay);
		}
	}

	public IEnumerator ConvertFollower()
	{
		if (state == null)
		{
			state = GetComponent<StateMachine>();
		}
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		AudioManager.Instance.PlayOneShot("event:/followers/ascend", base.gameObject);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Spine.AnimationState.SetAnimation(0, "convert-short", false);
		if ((bool)recruitParticles)
		{
			recruitParticles.Play();
		}
		portalSpine.gameObject.SetActive(true);
		portalSpine.AnimationState.SetAnimation(0, "convert-short", false);
		yield return new WaitForEndOfFrame();
		float duration = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "specials/special-activate-long", false).Animation.Duration;
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		yield return new WaitForSeconds(duration - 1f);
		_followerInfo.StartingCursedState = cursedState;
		FollowerManager.CreateNewRecruit(_followerInfo, NotificationCentre.NotificationType.NewRecruit);
		float value = UnityEngine.Random.value;
		Thought thought = Thought.None;
		if (value < 0.7f)
		{
			value = UnityEngine.Random.value;
			if (value <= 0.3f)
			{
				thought = Thought.HappyConvert;
			}
			else if (value > 0.3f && value < 0.6f)
			{
				thought = Thought.GratefulConvert;
			}
			else if (value >= 0.6f)
			{
				thought = Thought.SkepticalConvert;
			}
		}
		else
		{
			value = UnityEngine.Random.value;
			thought = ((!(value <= 0.3f) || DataManager.Instance.Followers.Count <= 0) ? Thought.InstantBelieverConvert : Thought.ResentfulConvert);
		}
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		_followerInfo.Thoughts.Add(data);
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sRescue = ScriptLocalization.Interactions.Convert;
	}

	public override void GetLabel()
	{
		if (Activated)
		{
			base.Label = "";
		}
		else if (Interactable)
		{
			if (sRescue == null)
			{
				UpdateLocalisation();
			}
			base.Label = sRescue;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		base.state = state;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		StartCoroutine(PositionPlayer());
		Activated = true;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		AudioManager.Instance.StopLoop(LoopInstance);
	}

	private new void OnDestroy()
	{
		AudioManager.Instance.StopLoop(LoopInstance);
	}

	private IEnumerator PositionPlayer()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		yield return new WaitForSeconds(0.25f);
		GetComponent<Collider2D>().enabled = false;
		Vector3 vector = ((state.transform.position.x < base.transform.position.x) ? Vector3.left : Vector3.right);
		if (Physics2D.Raycast(base.transform.position, vector, 1.5f, collisionMask).collider != null)
		{
			vector *= -1f;
		}
		Vector3 targetPosition = base.transform.position + vector * 1.5f;
		PlayerFarming.Instance.GoToAndStop(targetPosition);
		RecruitState.facingAngle = Utils.GetAngle(RecruitState.transform.position, PlayerFarming.Instance.transform.position);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		StartCoroutine(FollowerChoiceIE());
	}
}
