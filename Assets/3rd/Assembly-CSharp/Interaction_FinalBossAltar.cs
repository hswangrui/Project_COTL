using System.Collections;
using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_FinalBossAltar : Interaction
{
	private bool UseSimpleSetCamera;

	public UnityEvent KneelCallback;

	public UnityEvent RefuseCallback;

	public GameObject LookToObject;

	public SimpleSetCamera SimpleSetCamera;

	private string sInteraction;

	public GameObject ChoiceIndicator;

	private ChoiceIndicator c;

	private bool Activated;

	private void Start()
	{
		UpdateLocalisation();
		HoldToInteract = false;
		AutomaticallyInteract = true;
		ActivateDistance = 3f;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sInteraction = ScriptLocalization.Interactions_Intro.Kneel;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sInteraction);
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activated)
		{
			return;
		}
		base.OnInteract(state);
		Activated = true;
		if (UseSimpleSetCamera)
		{
			SimpleSetCamera.Play();
		}
		HUD_Manager.Instance.Hide(false, 0);
		PlayerFarming.Instance.GoToAndStop(base.transform.position, LookToObject, false, false, delegate
		{
			if (DungeonSandboxManager.Active)
			{
				Refuse();
			}
			else
			{
				StartCoroutine(IOnInteract());
			}
		});
	}

	private IEnumerator IOnInteract()
	{
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		PlayerFarming.Instance._state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "final-boss/decide-start", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "final-boss/decide-loop", true, 0f);
		yield return new WaitForSeconds(1f);
		GiveChoice();
	}

	private void GiveChoice()
	{
		GameManager.GetInstance().CameraSetTargetZoom(6f);
		UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
		GameObject gameObject = Object.Instantiate(ChoiceIndicator, GameObject.FindWithTag("Canvas").transform);
		gameObject.SetActive(true);
		c = gameObject.GetComponent<ChoiceIndicator>();
		c.Offset = new Vector3(0f, -300f, 0f);
		c.Show("Conversation_NPC/Story/Dungeon2/Leader2/2_Choice2", "UI/Generic/Accept", Refuse, Kneel, state.transform.position);
		c.ShowPrompt("Interactions/Intro/Kneel");
	}

	private void LateUpdate()
	{
		if (c != null)
		{
			c.UpdatePosition(state.transform.position);
		}
	}

	private void Kneel()
	{
		UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
		GameObject gameObject = Object.Instantiate(ChoiceIndicator, GameObject.FindWithTag("Canvas").transform);
		gameObject.SetActive(true);
		c = gameObject.GetComponent<ChoiceIndicator>();
		c.Offset = new Vector3(0f, -300f, 0f);
		c.Show("Conversation_NPC/Fox/Response_No", "Conversation_NPC/Fox/Response_Yes", delegate
		{
			GiveChoice();
		}, delegate
		{
			if (UseSimpleSetCamera)
			{
				SimpleSetCamera.Reset();
			}
			UIManager.PlayAudio("event:/ui/heretics_defeated");
			KneelCallback.Invoke();
		}, state.transform.position);
		c.ShowPrompt("FollowerInteractions/AreYouSure");
	}

	private void Refuse()
	{
		if (UseSimpleSetCamera)
		{
			SimpleSetCamera.Reset();
		}
		UIManager.PlayAudio("event:/ui/heretics_defeated");
		UnityEvent refuseCallback = RefuseCallback;
		if (refuseCallback != null)
		{
			refuseCallback.Invoke();
		}
	}
}
