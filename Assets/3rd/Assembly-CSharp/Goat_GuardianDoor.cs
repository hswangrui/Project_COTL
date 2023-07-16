using System.Collections;
using Spine.Unity;
using UnityEngine;

public class Goat_GuardianDoor : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	public EnemyGuardian1 Guardian;

	public EnemyGuardian2 Guardian2;

	public Interaction DoorInteraction;

	public SkeletonAnimation DoorSpine;

	public GameObject DoorCameraFocus;

	public void Play()
	{
		StartCoroutine(DoPlay());
	}

	private IEnumerator DoPlay()
	{
		DoorInteraction.enabled = false;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		BlockingDoor.CloseAll();
		Spine.AnimationState.SetAnimation(0, "lute-start", false);
		Spine.AnimationState.AddAnimation(0, "lute-loop", true, 0f);
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.MainBossA);
		yield return new WaitForSeconds(0.7f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		Guardian.Play();
	}

	public IEnumerator EndGuardianFightRoutine()
	{
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		yield return new WaitForSeconds(1f);
		Spine.AnimationState.SetAnimation(0, "lute-stop", true);
		AmbientMusicController.StopCombat();
		AudioManager.Instance.SetMusicCombatState(false);
		yield return new WaitForSeconds(1f);
		Spine.skeleton.ScaleX = -1f;
		Spine.AnimationState.SetAnimation(0, "teleport-out", false);
		yield return new WaitForSeconds(1.7f);
		Spine.enabled = false;
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}
}
