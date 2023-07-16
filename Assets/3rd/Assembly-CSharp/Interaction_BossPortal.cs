using System.Collections;
using I2.Loc;
using MMBiomeGeneration;
using MMTools;
using UnityEngine;

public class Interaction_BossPortal : Interaction
{
	private string sTeleport;

	private bool activating;

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
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "warp-out-down", false);
		yield return new WaitForSeconds(2f);
		MMTransition.StopCurrentTransition();
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", ChangeRoom);
		DungeonModifier.SetActiveModifier(null);
		activating = false;
	}

	private void ChangeRoom()
	{
		if (BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_1 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_2 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_3 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_4)
		{
			BiomeGenerator.ChangeRoom(0, 1);
		}
		else
		{
			BiomeGenerator.ChangeRoom(BiomeGenerator.BossCoords.x, BiomeGenerator.BossCoords.y);
		}
	}
}
