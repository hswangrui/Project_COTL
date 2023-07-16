using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_SimpleConversation : Interaction
{
	[Serializable]
	public class VariableAndCondition
	{
		public DataManager.Variables Variable;

		public bool Condition = true;
	}

	public bool OverrideSetCameras;

	[HideInInspector]
	public bool Spoken;

	public Vector3 CameraOffset = Vector3.zero;

	public List<ConversationEntry> Entries;

	public bool IncrementStoryVariable;

	public DataManager.Variables StoryPosition;

	public DataManager.Variables LastRun;

	public bool PopulateStory;

	public int StoryDungeon;

	public int StoryDungeonPosition;

	[TermsPopup("")]
	public string StoryCharacterName = "-";

	public GameObject StorySpeakPosition;

	public SkeletonAnimation StorySpine;

	public int StartingIndex;

	public List<MMTools.Response> Responses;

	public bool MovePlayerToListenPosition = true;

	public Vector3 ListenPosition = Vector3.left * 2f;

	public bool DeleteIfConditionsMet = true;

	public List<VariableAndCondition> DeleteConditions = new List<VariableAndCondition>();

	public List<VariableAndCondition> SetConditions = new List<VariableAndCondition>();

	public UnityEvent Callback;

	public bool UnlockDoorsAfterConversation;

	protected string sLabel;

	private bool ConditionMet;

	public bool AnimateBeforeConversation;

	public SkeletonAnimation Spine;

	private MeshRenderer SpinemeshRenderer;

	public bool HideBeforeTriggered = true;

	[SpineAnimation("", "Spine", true, false)]
	public string TriggeredAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string EndOnAnimation;

	public UnityEvent CallbackBeforeConversation;

	public bool CallOnConversationEnd = true;

	public bool SetPlayerInactiveOnStart = true;

	public bool Finished { get; set; }

	public void AddEntry()
	{
		if (Entries.Count <= 0)
		{
			Entries.Add(new ConversationEntry(base.gameObject, ""));
		}
		else
		{
			Entries.Add(ConversationEntry.Clone(Entries[Entries.Count - 1]));
		}
	}

	private void PopulateFromManualPath()
	{
		string text = "Conversation_NPC/Story/Dungeon" + StoryDungeon + "/Leader" + StoryDungeonPosition + "/";
		Entries.Clear();
		int num = -1;
		while (true)
		{
			int num2 = ++num;
			if (LocalizationManager.GetTermData(text + num2) != null)
			{
				ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, text + num);
				conversationEntry.CharacterName = StoryCharacterName;
				conversationEntry.Speaker = StorySpeakPosition;
				conversationEntry.SkeletonData = StorySpine;
				Entries.Add(conversationEntry);
				continue;
			}
			break;
		}
	}

	private void IncrementEntry()
	{
		if (Entries.Count <= 0)
		{
			return;
		}
		int num = StartingIndex;
		while (true)
		{
			string termToSpeak = ConversationEntry.Clone(Entries[StartingIndex]).TermToSpeak;
			string oldValue = StartingIndex.ToString();
			int num2 = ++num;
			if (LocalizationManager.GetTermData(termToSpeak.Replace(oldValue, num2.ToString())) != null)
			{
				ConversationEntry conversationEntry = ConversationEntry.Clone(Entries[StartingIndex]);
				conversationEntry.TermToSpeak = conversationEntry.TermToSpeak.Replace(StartingIndex.ToString(), num.ToString());
				Entries.Add(conversationEntry);
				continue;
			}
			break;
		}
	}

	private void Start()
	{
		IgnoreTutorial = true;
		ConditionMet = false;
		if (DeleteConditions.Count > 0)
		{
			ConditionMet = true;
			foreach (VariableAndCondition deleteCondition in DeleteConditions)
			{
				Debug.Log(string.Concat(base.gameObject.name, " ", deleteCondition.Variable, " ", deleteCondition.Condition.ToString(), "  ", DataManager.Instance.GetVariable(deleteCondition.Variable).ToString()));
				if (DataManager.Instance.GetVariable(deleteCondition.Variable) != deleteCondition.Condition)
				{
					ConditionMet = false;
					break;
				}
			}
			if (ConditionMet)
			{
				Debug.Log("ConditionMet");
				if (DeleteIfConditionsMet)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
				else
				{
					base.enabled = false;
				}
				return;
			}
		}
		UpdateLocalisation();
		if (AnimateBeforeConversation && HideBeforeTriggered && Spine != null)
		{
			SpinemeshRenderer = Spine.gameObject.GetComponent<MeshRenderer>();
			SpinemeshRenderer.enabled = false;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = (Interactable ? ScriptLocalization.Interactions.Talk : "");
	}

	public override void GetLabel()
	{
		base.GetLabel();
		if (Spoken)
		{
			base.Label = "";
			return;
		}
		if (sLabel == "")
		{
			UpdateLocalisation();
		}
		base.Label = sLabel;
	}

	public void Play()
	{
		if (!ConditionMet)
		{
			StateMachine component = GameObject.FindWithTag("Player").GetComponent<StateMachine>();
			OnInteract(component);
		}
	}

	private void Complete(TrackEntry trackEntry)
	{
		if (OverrideSetCameras)
		{
			SimpleSetCamera.DisableAll();
		}
		MMConversation.Play(new ConversationObject(Entries, Responses, DoCallBack), CallOnConversationEnd);
		GameManager.GetInstance().CameraSetOffset(CameraOffset);
		Spine.AnimationState.Complete -= Complete;
	}

	public override void OnInteract(StateMachine state)
	{
		UnityEvent callbackBeforeConversation = CallbackBeforeConversation;
		if (callbackBeforeConversation != null)
		{
			callbackBeforeConversation.Invoke();
		}
		if (Spoken)
		{
			return;
		}
		base.OnInteract(state);
		if (AnimateBeforeConversation)
		{
			if (OverrideSetCameras)
			{
				SimpleSetCamera.DisableAll();
			}
			LetterBox.Show(false);
			GameManager.GetInstance().RemoveAllFromCamera();
			GameManager.GetInstance().AddToCamera(Spine.gameObject);
			Spine.AnimationState.SetAnimation(0, TriggeredAnimation, false);
			Spine.AnimationState.Complete += Complete;
			Spine.AnimationState.AddAnimation(0, EndOnAnimation, false, 0f);
			StartCoroutine(ReEnableMeshRenderer());
		}
		else
		{
			if (OverrideSetCameras)
			{
				SimpleSetCamera.DisableAll();
			}
			MMConversation.Play(new ConversationObject(Entries, Responses, DoCallBack), CallOnConversationEnd, SetPlayerInactiveOnStart);
			GameManager.GetInstance().CameraSetOffset(CameraOffset);
		}
		AudioManager.Instance.PlayOneShot("event:/ui/conversation_start");
		Spoken = true;
		base.Label = "";
		Finished = false;
		if (!(state != null) || !MovePlayerToListenPosition)
		{
			return;
		}
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.GoToAndStop(base.transform.position + ListenPosition, base.gameObject, false, false, delegate
			{
				GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
				{
					if (Finished && PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.InActive)
					{
						PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
					}
				}));
			});
		}
		PlayerPrisonerController component = state.GetComponent<PlayerPrisonerController>();
		if (component != null)
		{
			Debug.Log("Move prisoner to: " + (base.transform.position + ListenPosition));
			AstarPath p = AstarPath.active;
			AstarPath.active = null;
			component.GoToAndStop(base.transform.position + ListenPosition, StateMachine.State.InActive, delegate
			{
				AstarPath.active = p;
			});
		}
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator ReEnableMeshRenderer()
	{
		yield return new WaitForEndOfFrame();
		if (SpinemeshRenderer != null)
		{
			SpinemeshRenderer.enabled = true;
		}
	}

	public virtual void DoCallBack()
	{
		if (OverrideSetCameras)
		{
			SimpleSetCamera.EnableAll();
		}
		Finished = true;
		AudioManager.Instance.PlayOneShot("event:/ui/conversation_end");
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		foreach (VariableAndCondition setCondition in SetConditions)
		{
			DataManager.Instance.SetVariable(setCondition.Variable, setCondition.Condition);
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
		if (UnlockDoorsAfterConversation)
		{
			RoomLockController.RoomCompleted();
		}
		if (IncrementStoryVariable)
		{
			DataManager.Instance.SetVariableInt(StoryPosition, DataManager.Instance.GetVariableInt(StoryPosition) + 1);
			DataManager.Instance.SetVariableInt(LastRun, DataManager.Instance.dungeonRun);
		}
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (MovePlayerToListenPosition)
		{
			Utils.DrawCircleXY(base.transform.position + ListenPosition, 0.4f, Color.blue);
		}
	}
}
