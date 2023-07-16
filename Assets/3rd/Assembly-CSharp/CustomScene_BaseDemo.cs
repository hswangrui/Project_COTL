using System.Collections;
using UnityEngine;

public class CustomScene_BaseDemo : BaseMonoBehaviour
{
	public GameObject PlayerStartPosition;

	private GameObject Player;

	private StateMachine PlayerState;

	private float Timer;

	public void Play()
	{
		if (DataManager.Instance.PlayerIsASpirit)
		{
			Player = GameObject.FindGameObjectWithTag("Player");
			Player.transform.position = PlayerStartPosition.transform.position;
			GameManager.GetInstance().CamFollowTarget.SnapTo(Player.transform.position);
			GameManager.GetInstance().CameraSetZoom(4f);
			GameManager.GetInstance().OnConversationNew(false, true);
			PlayerState = Player.GetComponent<StateMachine>();
			StartCoroutine(ConvertPlayer());
			DataManager.Instance.PlayerIsASpirit = false;
		}
	}

	private IEnumerator ConvertPlayer()
	{
		yield return new WaitForEndOfFrame();
		PlayerState.CURRENT_STATE = StateMachine.State.Unconverted;
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationNext(Player, 4f);
		yield return new WaitForSeconds(0.5f);
		PlayerState.CURRENT_STATE = StateMachine.State.Converting;
		yield return new WaitForSeconds(4f);
		GameManager.GetInstance().OnConversationNext(Player, 6f);
		CameraManager.shakeCamera(0.5f, Random.Range(0, 360));
		while (PlayerState.CURRENT_STATE == StateMachine.State.Converting)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		GameManager.GetInstance().AddPlayerToCamera();
	}
}
