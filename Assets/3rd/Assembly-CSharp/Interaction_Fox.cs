using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Interaction_Fox : Interaction
{
	public Interaction_SimpleConversation firstMeeting;

	public Interaction_SimpleConversation secondMeeting;

	public Interaction_SimpleConversation YesOption;

	public Interaction_SimpleConversation NoOption;

	public DOTweenAnimation Animation;

	public Transform inWater;

	public Transform outWater;

	public GameObject Fox;

	private bool Activated;

	private bool showingFox;

	public GameObject ChoiceIndicator;

	public GameObject CameraObject;

	public GameObject Conversations;

	public GameObject KeyPiece;

	private ChoiceIndicator c;

	public GameObject Moon;

	private string sYes;

	private string sYesSubtitle;

	private string sNo;

	private string sNoSubtitle;

	public void GiveOverFollowerChoice()
	{
		GameObject gameObject = Object.Instantiate(ChoiceIndicator, GameObject.FindWithTag("Canvas").transform);
		gameObject.SetActive(true);
		c = gameObject.GetComponent<ChoiceIndicator>();
		c.Offset = new Vector3(0f, 200f, 0f);
		c.Show(sYes, sYesSubtitle, sNo, sNoSubtitle, Yes, No, Fox.transform.position);
	}

	private void Yes()
	{
		StartCoroutine(YesRoutine());
	}

	private IEnumerator YesRoutine()
	{
		DataManager.Instance.GaveFollowerToFox = true;
		BiomeConstants.Instance.ShakeCamera();
		RumbleManager.Instance.Rumble();
		yield return new WaitForSeconds(1f);
		Random.Range(0, DataManager.Instance.Followers.Count);
		bool flag = false;
		FollowerBrain followerBrain = FollowerBrain.AllBrains[Random.Range(0, FollowerBrain.AllBrains.Count)];
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.ID == followerBrain.Info.ID)
			{
				Debug.Log("Kill Follower: " + allBrain.Info.Name);
				allBrain.Die(NotificationCentre.NotificationType.Died);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.Log("Didnt find a follower :(");
		}
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		YesOption.gameObject.SetActive(true);
	}

	public void TakeFollower()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTask.Type == FollowerTaskType.FollowPlayer)
			{
				allBrain.Die(NotificationCentre.NotificationType.Died);
				break;
			}
		}
	}

	private void No()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		NoOption.gameObject.SetActive(true);
	}

	public void GiveKey()
	{
		StartCoroutine(GiveKeyRoutine());
	}

	private IEnumerator GiveKeyRoutine()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		KeyPiece.GetComponent<PickUp>().enabled = false;
		KeyPiece.GetComponent<Interaction_KeyPiece>().enabled = false;
		KeyPiece.GetComponent<Interaction_KeyPiece>().AutomaticallyInteract = true;
		KeyPiece.GetComponent<Interaction_KeyPiece>().ActivateDistance = 5f;
		KeyPiece.SetActive(true);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(KeyPiece, 6f);
		KeyPiece.transform.DOPunchScale(Vector3.one, 1f);
		BiomeConstants.Instance.ShakeCamera();
		RumbleManager.Instance.Rumble();
		yield return new WaitForSeconds(1f);
		KeyPiece.transform.DOMove(PlayerFarming.Instance.gameObject.transform.position, 5f).SetEase(Ease.InCirc);
		yield return new WaitForSeconds(4.5f);
		KeyPiece.GetComponent<Interaction_KeyPiece>().enabled = true;
		yield return new WaitForSeconds(1f);
		base.gameObject.SetActive(false);
		GameManager.GetInstance().OnConversationEnd();
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private void Start()
	{
		KeyPiece.SetActive(false);
		UpdateLocalisation();
		CheckTimeOfDay();
	}

	private new void Update()
	{
		if (!DataManager.Instance.GaveFollowerToFox)
		{
			CheckTimeOfDay();
			if (c != null)
			{
				c.UpdatePosition(base.transform.position);
			}
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (DataManager.Instance.GaveFollowerToFox)
		{
			base.gameObject.SetActive(false);
		}
		secondMeeting.gameObject.SetActive(false);
	}

	public void CheckIfFollowerFromConvo()
	{
		secondMeeting.gameObject.SetActive(false);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTask.Type == FollowerTaskType.FollowPlayer)
			{
				secondMeeting.gameObject.SetActive(true);
				break;
			}
		}
	}

	public void CheckTimeOfDay()
	{
		if (!DataManager.Instance.GaveFollowerToFox)
		{
			if (TimeManager.CurrentPhase == DayPhase.Night && DataManager.Instance.Followers.Count > 1)
			{
				if (!showingFox)
				{
					Moon.SetActive(true);
					Fox.SetActive(true);
					Conversations.SetActive(true);
					Animation.target = inWater;
					showingFox = true;
				}
			}
			else if (showingFox)
			{
				Moon.SetActive(false);
			}
			else if (Fox != null)
			{
				Fox.SetActive(false);
				Conversations.SetActive(false);
			}
		}
		else
		{
			base.gameObject.SetActive(false);
			Fox.SetActive(false);
			Moon.SetActive(false);
			Conversations.SetActive(false);
		}
	}

	private IEnumerator CheckTime()
	{
		while (TimeManager.CurrentPhase != DayPhase.Night)
		{
			CheckTimeOfDay();
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator TurnOffWolf()
	{
		yield return new WaitForSeconds(1f);
		showingFox = false;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sYes = "Interactions/SacrificeFollower";
		sYesSubtitle = "Interactions/SacrificeFollowerSubtitle";
		sNo = "Interactions/SacrificeMaybeNot";
		sNoSubtitle = "Interactions/SacrificeMaybeNotSubtitle";
	}

	public override void GetLabel()
	{
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
	}
}
