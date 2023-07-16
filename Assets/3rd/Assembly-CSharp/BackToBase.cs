using UnityEngine;

public class BackToBase : BaseMonoBehaviour
{
	private bool Activated;

	public GameObject WalkTo;

	public bool WalkedBack = true;

	public bool EnterTemple;

	public void PlayBackToBase()
	{
		BiomeBaseManager.EnterTemple = false;
		GameManager.ToShip();
	}

	public void Play(bool Walking)
	{
		if (Walking)
		{
			Vector3 position = WalkTo.transform.position;
			position.x = PlayerFarming.Instance.transform.position.x;
			WalkTo.transform.position = position;
			PlayerFarming.Instance.GoToAndStop(WalkTo, null, false, true);
			BiomeBaseManager.WalkedBack = true;
		}
		GameManager.ToShip();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Activated && collision.gameObject.tag == "Player")
		{
			Play(WalkedBack);
			Activated = true;
		}
	}
}
