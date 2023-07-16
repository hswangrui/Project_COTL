using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Map;
using UnityEngine;

public class Interaction_TeleporterMap : Interaction
{
	private enum State
	{
		Inactive,
		Activating,
		Active
	}

	public bool Activated;

	public GameObject InactivatedGO;

	public GameObject ActivatedGO;

	public static Interaction_TeleporterMap Instance;

	public static List<Interaction_TeleporterMap> Teleporters = new List<Interaction_TeleporterMap>();

	private string sTeleport;

	private bool activating;

	private State CurrentState;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sTeleport = ScriptLocalization.Interactions.Teleport;
	}

	public override void GetLabel()
	{
		base.Label = sTeleport;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		CurrentState = State.Active;
		InactivatedGO.SetActive(false);
		ActivatedGO.SetActive(true);
		Teleporters.Add(this);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Teleporters.Remove(this);
	}

	private void InitEnemies()
	{
		if (RoomManager.r.activeTeleporter)
		{
			CurrentState = State.Active;
			SetGameObjectActive();
		}
	}

	private void SetGameObjectActive()
	{
		if (CurrentState == State.Inactive)
		{
			InactivatedGO.SetActive(true);
			ActivatedGO.SetActive(false);
		}
		else
		{
			CurrentState = State.Active;
			InactivatedGO.SetActive(false);
			ActivatedGO.SetActive(true);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!activating && !PlayerFarming.Instance.GoToAndStopping)
		{
			PlayerFarming.Instance.GoToAndStop(base.transform.position, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoTeleportOut());
			});
		}
	}

	private IEnumerator DoTeleportOut()
	{
		activating = true;
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("warp-out-down", 0, false, 0f);
		yield return new WaitForSeconds(2f);
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		Node randomNodeOnLayer = MapGenerator.GetRandomNodeOnLayer(MapManager.Instance.CurrentLayer + 2);
		if (randomNodeOnLayer != null)
		{
			yield return adventureMapOverlayController.TeleportNode(randomNodeOnLayer);
		}
		activating = false;
	}

	private void Activate()
	{
		SetGameObjectActive();
		RoomManager.r.activeTeleporter = true;
		MiniMap.CurrentRoomShowTeleporter();
	}
}
