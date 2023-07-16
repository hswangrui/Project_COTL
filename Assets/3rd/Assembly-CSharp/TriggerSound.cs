using FMODUnity;
using MMTools;
using UnityEngine;
using UnityEngine.Events;

public class TriggerSound : MonoBehaviour
{
	private bool Activated;

	public float ActivatedTimer = 3f;

	private float Progress;

	private string DefaultAnimation;

	private UnitObject player;

	private Collider2D PlayerCollision;

	[EventRef]
	public string VOtoPlay = "event:/enemy/vocals/humanoid/warning";

	private GameObject p;

	public float ActivateDistance = 0.666f;

	public bool UsePlayerPrisoner;

	public UnityEvent Callback;

	public bool PlayOnce;

	public float Distance;

	private bool foundPlayer;

	public void PushPlayer()
	{
		AudioManager.Instance.PlayOneShot(VOtoPlay, base.gameObject);
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}

	private void Start()
	{
		FindPlayer();
	}

	private void FindPlayer()
	{
		if (!(p == null))
		{
			return;
		}
		if (UsePlayerPrisoner)
		{
			if (PlayerPrisonerController.Instance != null)
			{
				p = PlayerPrisonerController.Instance.gameObject;
				player = p.GetComponent<UnitObject>();
				foundPlayer = true;
			}
		}
		else if (PlayerFarming.Instance != null)
		{
			p = PlayerFarming.Instance.gameObject;
			player = p.GetComponent<UnitObject>();
			foundPlayer = true;
		}
	}

	private void Update()
	{
		if (player == null)
		{
			if (!foundPlayer)
			{
				FindPlayer();
			}
		}
		else
		{
			if (MMConversation.isPlaying)
			{
				return;
			}
			Distance = Vector3.Distance(base.gameObject.transform.position, player.gameObject.transform.position);
			if (Vector3.Distance(base.gameObject.transform.position, player.gameObject.transform.position) < ActivateDistance && !Activated)
			{
				Progress = 0f;
				Activated = true;
				AudioManager.Instance.PlayOneShot(VOtoPlay, base.gameObject);
				UnityEvent callback = Callback;
				if (callback != null)
				{
					callback.Invoke();
				}
			}
			if (Activated && !PlayOnce)
			{
				if (Progress < ActivatedTimer)
				{
					Progress += Time.deltaTime;
				}
				else
				{
					Activated = false;
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, ActivateDistance, Color.green);
	}
}
