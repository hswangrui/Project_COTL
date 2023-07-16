using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class RescueFollowerController : BaseMonoBehaviour
{
	public List<BaseMonoBehaviour> DisableForConversation = new List<BaseMonoBehaviour>();

	public List<SkeletonAnimation> ConversationSkeletons = new List<SkeletonAnimation>();

	public GameObject GoopCylinder;

	public GameObject Altar;

	public GameObject AltarBroken;

	public UnityEvent IntroConversationCallbacks;

	public GameObject GameObjectToDestroyIfAlreadyCompleted;

	public GameObject CameraTarget;

	public Vector3 TriggerArea = Vector3.zero;

	public float TriggerRadius = 5f;

	private GameObject Player;

	public BarricadeLine barricadeLine;

	public UnityEvent Callbacks;

	private bool Completed;

	[SerializeField]
	private Interaction_Follower follower;

	[SerializeField]
	private string freedAnimation = "tied-to-altar-rescue";

	public bool SetVariableOnComplete;

	public DataManager.Variables VariableToComplete;

	private bool introCompleted;

	private bool releasedFollower;

	private void OnEnable()
	{
		Invoke("DisableFollower", 0.1f);
		StartCoroutine(WaitForPlayer());
	}

	private void Awake()
	{
		foreach (BaseMonoBehaviour item in DisableForConversation)
		{
			item.enabled = false;
		}
	}

	public void IntroPlay()
	{
		TriggerRadius = 10f;
	}

	private void DisableFollower()
	{
		if (!releasedFollower)
		{
			follower.Interactable = false;
		}
	}

	private void OnDisable()
	{
	}

	private IEnumerator WaitForPlayer()
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.FollowerAmbience);
		if (!introCompleted)
		{
			bool requiresIntro = false;
			if (!DataManager.Instance.FirstFollowerRescue || (!DataManager.Instance.FirstDungeon1RescueRoom && PlayerFarming.Location == FollowerLocation.Dungeon1_1) || (!DataManager.Instance.FirstDungeon2RescueRoom && PlayerFarming.Location == FollowerLocation.Dungeon1_2) || (!DataManager.Instance.FirstDungeon3RescueRoom && PlayerFarming.Location == FollowerLocation.Dungeon1_3) || (!DataManager.Instance.FirstDungeon4RescueRoom && PlayerFarming.Location == FollowerLocation.Dungeon1_4))
			{
				requiresIntro = true;
			}
			foreach (BaseMonoBehaviour item in DisableForConversation)
			{
				item.enabled = false;
			}
			while ((Player = GameObject.FindGameObjectWithTag("Player")) == null)
			{
				yield return null;
			}
			while (LetterBox.IsPlaying)
			{
				yield return null;
			}
			BlockingDoor.CloseAll();
			RoomLockController.CloseAll();
			while (Vector3.Distance(base.transform.position + TriggerArea, Player.transform.position) > TriggerRadius)
			{
				yield return null;
			}
			foreach (BaseMonoBehaviour item2 in DisableForConversation)
			{
				if (item2 == null)
				{
					requiresIntro = false;
				}
			}
			if (requiresIntro)
			{
				DoConversation();
				while (MMConversation.CURRENT_CONVERSATION != null)
				{
					yield return null;
				}
				UnityEvent introConversationCallbacks = IntroConversationCallbacks;
				if (introConversationCallbacks != null)
				{
					introConversationCallbacks.Invoke();
				}
			}
			Debug.Log("BEGIN COMBAT");
			EnemyRoundsBase instance = EnemyRoundsBase.Instance;
			if ((object)instance != null)
			{
				instance.BeginCombat(true, Close);
			}
		}
		foreach (BaseMonoBehaviour item3 in DisableForConversation)
		{
			if (item3 != null)
			{
				item3.enabled = true;
			}
		}
		BiomeGenerator.Instance.CurrentRoom.Active = true;
		GoopCylinder.SetActive(true);
		if ((bool)barricadeLine)
		{
			barricadeLine.Close();
		}
		BlockingDoor.CloseAll();
		RoomLockController.CloseAll();
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.SpecialCombat);
		introCompleted = true;
	}

	public void ReleaseFollower()
	{
		if (SetVariableOnComplete)
		{
			DataManager.Instance.SetVariable(VariableToComplete, true);
		}
		releasedFollower = true;
		UnityEvent callbacks = Callbacks;
		if (callbacks != null)
		{
			callbacks.Invoke();
		}
		follower.Interactable = true;
	}

	private void Close()
	{
		if (!Completed)
		{
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.StandardAmbience);
			Debug.Log("CLOSE!");
			StartCoroutine(CloseRoutine());
		}
	}

	private IEnumerator CloseRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraTarget, 4f);
		yield return new WaitForSeconds(1f);
		CircleCollider2D component = GoopCylinder.GetComponent<CircleCollider2D>();
		Bounds bounds = component.bounds;
		component.enabled = false;
		AstarPath.active.UpdateGraphs(bounds);
		GoopCylinder.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		if ((bool)barricadeLine)
		{
			CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f);
			barricadeLine.Open();
			yield return new WaitForSeconds(1.5f);
		}
		CameraManager.instance.ShakeCameraForDuration(1.3f, 1.4f, 0.4f);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(Altar.transform.position + Vector3.back * 0.5f);
		BiomeConstants.Instance.EmitGroundSmashVFXParticles(Altar.transform.position + Vector3.back * 0.05f);
		BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.stone, base.transform.position, Vector3.forward * 50f, 20);
		Altar.SetActive(false);
		AltarBroken.SetActive(true);
		follower.skeletonAnimation.AnimationState.SetAnimation(0, freedAnimation, false);
		follower.skeletonAnimation.AnimationState.AddAnimation(0, "unconverted", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/followers/break_free");
		yield return new WaitForSeconds(1.5f);
		ReleaseFollower();
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		Completed = true;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + TriggerArea, TriggerRadius, Color.yellow);
	}

	private void DoConversation()
	{
		List<ConversationEntry> list = ((PlayerFarming.Location == FollowerLocation.IntroDungeon) ? GetFirstConvo() : GetDungeonConvo());
		foreach (ConversationEntry item in list)
		{
			item.soundPath = "event:/enemy/vocals/humanoid/warning";
		}
		MMConversation.Play(new ConversationObject(list, null, null));
	}

	private List<ConversationEntry> GetFirstConvo()
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(DisableForConversation[0].gameObject, "Conversation_NPC/FollowerRescue/FollowerRescue/Ritual0"));
		list.Add(new ConversationEntry(DisableForConversation[1].gameObject, "Conversation_NPC/FollowerRescue/FollowerRescue/Ritual1"));
		list[0].Animation = "worship";
		list[0].Offset = new Vector3(0f, 1f, 0f);
		list[1].Offset = new Vector3(0f, 1f, 0f);
		list[1].Animation = "notice-player";
		list[1].LoopAnimation = false;
		list[0].Speaker = DisableForConversation[0].gameObject;
		list[1].Speaker = DisableForConversation[1].gameObject;
		list[0].SkeletonData = ConversationSkeletons[0];
		list[1].SkeletonData = ConversationSkeletons[1];
		return list;
	}

	private List<ConversationEntry> GetDungeonConvo()
	{
		BaseMonoBehaviour baseMonoBehaviour = DisableForConversation[Random.Range(0, DisableForConversation.Count)];
		string termToSpeak = "";
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			termToSpeak = "Conversation_NPC/Leshy/BackStory/0";
			break;
		case FollowerLocation.Dungeon1_2:
			termToSpeak = "Conversation_NPC/Heket/BackStory/0";
			break;
		case FollowerLocation.Dungeon1_3:
			termToSpeak = "Conversation_NPC/Kallamar/BackStory/0";
			break;
		case FollowerLocation.Dungeon1_4:
			termToSpeak = "Conversation_NPC/Shamura/BackStory/0";
			break;
		}
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(baseMonoBehaviour.gameObject, termToSpeak));
		list[0].Animation = "worship";
		list[0].Offset = new Vector3(0f, 3f, 0f);
		list[0].Speaker = DisableForConversation[0].gameObject;
		list[0].SkeletonData = ConversationSkeletons[0];
		return list;
	}
}
