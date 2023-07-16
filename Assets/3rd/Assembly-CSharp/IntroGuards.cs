using System.Collections;
using UnityEngine;

public class IntroGuards : BaseMonoBehaviour
{
	public StateMachine Guard1;

	public StateMachine Guard2;

	public GameObject Wall;

	private GameObject Player;

	private bool Activated;

	private void Start()
	{
		Wall.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Activated && collision.gameObject.tag == "Player")
		{
			Player = collision.gameObject;
			StartCoroutine(FollowPlayer());
			Activated = true;
		}
	}

	private IEnumerator FollowPlayer()
	{
		Wall.SetActive(true);
		float Progress = 0f;
		float Duration = 1.5f;
		Vector3 StartGuard1 = Guard1.transform.position;
		Vector3 StartGuard2 = Guard2.transform.position;
		Vector3 StartWall = Wall.transform.position;
		Guard1.CURRENT_STATE = StateMachine.State.Moving;
		Guard2.CURRENT_STATE = StateMachine.State.Moving;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Guard1.transform.position = Vector3.Lerp(StartGuard1, base.transform.position + Vector3.left * 0.75f, Progress / Duration);
			Guard2.transform.position = Vector3.Lerp(StartGuard2, base.transform.position + Vector3.right * 0.75f, Progress / Duration);
			Wall.transform.position = Vector3.Lerp(StartWall, base.transform.position, Progress / Duration);
			Guard1.facingAngle = 80f;
			Guard2.facingAngle = 95f;
			yield return null;
		}
		Guard1.CURRENT_STATE = StateMachine.State.Idle;
		Guard2.CURRENT_STATE = StateMachine.State.Idle;
	}
}
