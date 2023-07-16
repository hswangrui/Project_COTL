using System.Collections;
using UnityEngine;

public class TestingCustomRoom : BaseMonoBehaviour
{
	public bool AutoPlacePlayer = true;

	public GameObject PlayerPrefab;

	private GameObject Player;

	private StateMachine PlayerState;

	private Door North;

	private Door East;

	private Door South;

	private Door West;

	private void Start()
	{
		if (AutoPlacePlayer)
		{
			StartCoroutine(PlacePlayerRoutine());
		}
	}

	private IEnumerator PlacePlayerRoutine()
	{
		yield return new WaitForEndOfFrame();
		PlacePlayer();
	}

	private void PlacePlayer()
	{
		Time.timeScale = 1f;
		GetDoors();
		if (Player == null)
		{
			Player = Object.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, GameObject.FindGameObjectWithTag("Unit Layer").transform);
			PlayerState = Player.GetComponent<StateMachine>();
		}
		Player.transform.position = Vector3.zero;
		if (South != null)
		{
			Player.transform.position = South.transform.position + Vector3.up * 0.5f;
			PlayerState.facingAngle = 90f;
		}
		else if (North != null)
		{
			Player.transform.position = North.transform.position + Vector3.down * 0.5f;
			PlayerState.facingAngle = 270f;
		}
		else if (West != null)
		{
			Player.transform.position = West.transform.position + Vector3.right * 0.5f;
			PlayerState.facingAngle = 0f;
		}
		else if (East != null)
		{
			Player.transform.position = East.transform.position + Vector3.left * 0.5f;
			PlayerState.facingAngle = 180f;
		}
		GameManager.GetInstance().CameraSnapToPosition(Player.transform.position);
		GameManager.GetInstance().AddPlayerToCamera();
	}

	private void GetDoors()
	{
		GameObject obj = GameObject.FindGameObjectWithTag("North Door");
		North = (((object)obj != null) ? obj.GetComponent<Door>() : null);
		GameObject obj2 = GameObject.FindGameObjectWithTag("East Door");
		East = (((object)obj2 != null) ? obj2.GetComponent<Door>() : null);
		GameObject obj3 = GameObject.FindGameObjectWithTag("South Door");
		South = (((object)obj3 != null) ? obj3.GetComponent<Door>() : null);
		GameObject obj4 = GameObject.FindGameObjectWithTag("West Door");
		West = (((object)obj4 != null) ? obj4.GetComponent<Door>() : null);
	}
}
