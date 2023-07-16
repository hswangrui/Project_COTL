using System.Collections;
using MMTools;
using UnityEngine;

public class SimpleSceneManager : BaseMonoBehaviour
{
	private GameObject player;

	public CameraFollowTarget Camera;

	public GameObject PlayerPrefab;

	public Transform StartPlayerPosition;

	public bool HideHUD;

	private void Start()
	{
		StartCoroutine(PlaceAndPositionPlayer());
		if (HideHUD)
		{
			HUD_Manager.Instance.Hide(true, 0);
		}
	}

	private IEnumerator PlaceAndPositionPlayer()
	{
		if (player == null)
		{
			player = Object.Instantiate(PlayerPrefab, GameObject.FindGameObjectWithTag("Unit Layer").transform, true);
		}
		if (StartPlayerPosition != null)
		{
			player.transform.position = StartPlayerPosition.position;
			player.GetComponent<StateMachine>().facingAngle = 90f;
		}
		yield return new WaitForEndOfFrame();
		GameObject gameObject = GameObject.FindWithTag("Player Camera Bone");
		Camera.SnapTo(gameObject.transform.position);
		yield return new WaitForSeconds(2f);
		HUD_DisplayName.Play("The Realm Beyond");
		MMTransition.ResumePlay();
	}
}
