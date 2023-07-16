using System.Collections;
using MMTools;
using UnityEngine;

public class BreakTempleChain : BaseMonoBehaviour
{
	public RoomSwapManager RoomSwapManager;

	public TempleChain TempleChain;

	public GameObject PlayerPosition;

	private void OnEnable()
	{
		StartCoroutine(EnableRoutine());
	}

	private IEnumerator EnableRoutine()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(FadeRoutine());
		float Progress = 0f;
		float Duration = 3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				GameManager.GetInstance().CameraSetZoom(6f - 4f * Progress / Duration);
				CameraManager.shakeCamera(0.1f + 0.6f * (Progress / Duration));
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator FadeRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.WhiteFade, MMTransition.NO_SCENE, 3f, "", ChangeRoom);
	}

	private void ChangeRoom()
	{
		PlayerFarming.Instance.transform.position = PlayerPosition.transform.position;
		PlayerFarming.Instance.state.facingAngle = 90f;
		RoomSwapManager.ToggleChurch();
		CameraManager.shakeCamera(0.5f);
		GameManager.GetInstance().CameraResetTargetZoom();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
		GameManager.GetInstance().CameraSnapToPosition(PlayerFarming.Instance.transform.position);
		TempleChain.Play();
	}
}
