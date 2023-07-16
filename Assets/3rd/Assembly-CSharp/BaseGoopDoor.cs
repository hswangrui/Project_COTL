using System;
using System.Collections;
using I2.Loc;
using MMTools;
using UnityEngine;

public class BaseGoopDoor : Interaction
{
	public static BaseGoopDoor Instance;

	public bool IsOpen = true;

	public GameObject BlockingCollider;

	public Animator Animator;

	private string doorLockedLabel = "";

	private string sIndoctrinateFollower;

	public SimpleSetCamera SimpleSetCamera;

	public bool LockDoor { get; set; }

	private void Awake()
	{
		BlockingCollider.SetActive(false);
	}

	private void Start()
	{
		Interactable = false;
		UpdateLocalisation();
		OnNewRecruit();
		if (DataManager.Instance.BaseGoopDoorLocked)
		{
			doorLockedLabel = DataManager.Instance.BaseGoopDoorLoc;
			BlockGoopDoor();
		}
	}

	public void BlockGoopDoor(string loc = "")
	{
		Debug.Log("BlockGoopDoor()".Colour(Color.red));
		DoorUp();
		LockDoor = true;
		DataManager.Instance.BaseGoopDoorLocked = true;
		if (!string.IsNullOrEmpty(loc))
		{
			DataManager.Instance.BaseGoopDoorLoc = loc;
			doorLockedLabel = loc;
		}
	}

	public void UnblockGoopDoor()
	{
		Debug.Log("BlockGoopDoor()".Colour(Color.green));
		LockDoor = false;
		DoorDown();
		DataManager.Instance.BaseGoopDoorLocked = false;
	}

	public override void OnEnableInteraction()
	{
		Instance = this;
		base.OnEnableInteraction();
		FollowerRecruit.OnNewRecruit = (Action)Delegate.Combine(FollowerRecruit.OnNewRecruit, new Action(OnNewRecruit));
		if ((bool)BiomeBaseManager.Instance)
		{
			BiomeBaseManager instance = BiomeBaseManager.Instance;
			instance.OnNewRecruitRevealed = (Action)Delegate.Combine(instance.OnNewRecruitRevealed, new Action(OnNewRecruit));
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		FollowerRecruit.OnNewRecruit = (Action)Delegate.Remove(FollowerRecruit.OnNewRecruit, new Action(OnNewRecruit));
		if ((bool)BiomeBaseManager.Instance)
		{
			BiomeBaseManager instance = BiomeBaseManager.Instance;
			instance.OnNewRecruitRevealed = (Action)Delegate.Remove(instance.OnNewRecruitRevealed, new Action(OnNewRecruit));
		}
	}

	private void OnNewRecruit()
	{
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sIndoctrinateFollower = ScriptLocalization.Interactions.IndoctrinateBeforeLeaving;
	}

	public override void GetLabel()
	{
		if (LockDoor)
		{
			if (DataManager.Instance.InTutorial && ObjectiveManager.AllObjectiveGroupIDs().Count == 1)
			{
				foreach (ObjectivesData item in ObjectiveManager.GetAllObjectivesOfGroup(ObjectiveManager.AllObjectiveGroupIDs()[0]))
				{
					if (DataManager.Instance.Objectives.Contains(item))
					{
						base.Label = item.Text;
						break;
					}
				}
				return;
			}
			base.Label = (string.IsNullOrEmpty(doorLockedLabel) ? "" : LocalizationManager.GetTranslation(doorLockedLabel));
		}
		else
		{
			base.Label = ((!IsOpen) ? sIndoctrinateFollower : "");
		}
	}

	public override void IndicateHighlighted()
	{
	}

	public override void EndIndicateHighlighted()
	{
	}

	public void DoorUp(string label = "")
	{
		if (!LockDoor)
		{
			if (IsOpen)
			{
				Animator.Play("GoopWallIntro");
			}
			IsOpen = false;
			BlockingCollider.SetActive(true);
			doorLockedLabel = label;
		}
	}

	public void DoorDown()
	{
		Debug.Log("LockDoor: " + LockDoor);
		if (!LockDoor)
		{
			if (!IsOpen)
			{
				Animator.Play("GoopWallDown");
			}
			IsOpen = true;
			BlockingCollider.SetActive(false);
			doorLockedLabel = "";
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!(PlayerFarming.Instance == null) && !(collision.gameObject != PlayerFarming.Instance.gameObject) && !IsOpen)
		{
			Animator.Play("GoopWallColliding");
			AudioManager.Instance.PlayOneShot("event:/Stings/generic_negative", base.gameObject);
		}
	}

	public void PlayOpenDoorSequence(Action Callback)
	{
		StartCoroutine(PlayOpenDoorSequenceRoutine(Callback));
	}

	private IEnumerator PlayOpenDoorSequenceRoutine(Action Callback)
	{
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		SimpleSetCamera.Play();
		yield return new WaitForSeconds(2.5f);
		UnblockGoopDoor();
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", base.gameObject);
		yield return new WaitForSeconds(2f);
		SimpleSetCamera.Reset();
		if (Callback != null)
		{
			Callback();
		}
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
	}
}
