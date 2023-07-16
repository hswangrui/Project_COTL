using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TrapSpikes : BaseMonoBehaviour
{
	private enum State
	{
		Idle,
		Warning,
		Attacking
	}

	private State CurrentState;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private Collider2D boxCollider2D;

	private List<Collider2D> colliders;

	private ContactFilter2D contactFilter2D;

	[SerializeField]
	private GameObject SpikesParent;

	[SerializeField]
	private GameObject SpikeObject;

	private BoxCollider2D thisCollider2D;

	[SerializeField]
	private List<TrapSpike> spikes = new List<TrapSpike>();

	private bool deactivated;

	private bool showSpikesTrigger;

	private Health EnemyHealth;

	public GameObject ParentToDestroy;

	private void Start()
	{
		Transform obj = base.transform;
		Vector3 position = obj.position;
		position = new Vector3(position.x, position.y, -0.015f);
		obj.position = position;
		contactFilter2D = default(ContactFilter2D);
		contactFilter2D.NoFilter();
		for (int i = 0; i < SpikesParent.transform.childCount; i++)
		{
			spikes.Add(SpikesParent.transform.GetChild(i).gameObject.GetComponent<TrapSpike>());
		}
		showSpikesDefault();
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		showSpikesDefault();
	}

	private void OnDestroy()
	{
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	private void RoomLockController_OnRoomCleared()
	{
		if (base.gameObject.activeInHierarchy)
		{
			deactivated = true;
			for (int i = 0; i < spikes.Count; i++)
			{
				spikes[i].DisableSpike();
			}
		}
	}

	private void showSpikesDefault()
	{
		spriteRenderer.enabled = false;
		for (int i = 0; i < spikes.Count; i++)
		{
			spikes[i].AnimateSpike("in-idle", Color.white);
		}
	}

	private void showSpikes()
	{
		showSpikesTrigger = true;
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		for (int i = 0; i < spikes.Count; i++)
		{
			spikes[i].SetRedSprite();
			spikes[i].AnimateSpike("out-idle", Color.red);
		}
	}

	private IEnumerator DoAttack()
	{
		AudioManager.Instance.PlayOneShot("event:/material/footstep_hard", base.transform.position);
		float Timer = 0f;
		foreach (TrapSpike spike in spikes)
		{
			spike.SetWarningSprite();
			spike.transform.DOShakePosition(0.5f - Timer, new Vector3(0.1f, 0f, 0f));
		}
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			while (PlayerRelic.TimeFrozen)
			{
				yield return null;
			}
			spriteRenderer.color = Color.yellow;
			yield return null;
		}
		CurrentState = State.Attacking;
		if (!showSpikesTrigger)
		{
			showSpikes();
		}
		Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + ((Time.deltaTime == 0f) ? 0.6f : Time.deltaTime));
			if (!(num < 0.5f))
			{
				break;
			}
			while (PlayerRelic.TimeFrozen)
			{
				yield return null;
			}
			if (Timer < 0.3f)
			{
				colliders = new List<Collider2D>();
				boxCollider2D.OverlapCollider(contactFilter2D, colliders);
				foreach (Collider2D collider in colliders)
				{
					EnemyHealth = collider.GetComponent<Health>();
					if (EnemyHealth != null && (EnemyHealth.team != Health.Team.PlayerTeam || !TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps)) && !EnemyHealth.ImmuneToTraps)
					{
						EnemyHealth.DealDamage(1f, base.gameObject, base.transform.position);
					}
				}
			}
			spriteRenderer.color = Color.red;
			yield return null;
		}
		for (int i = 0; i < spikes.Count; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_retract", base.gameObject);
			spikes[i].AnimateSpike("in", Color.white);
		}
		showSpikesTrigger = false;
		CurrentState = State.Idle;
		spriteRenderer.color = Color.white;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!deactivated)
		{
			EnemyHealth = collision.gameObject.GetComponent<Health>();
			if (CurrentState == State.Idle && EnemyHealth != null && EnemyHealth.team == Health.Team.PlayerTeam && EnemyHealth.state != null && EnemyHealth.state.CURRENT_STATE != StateMachine.State.Dodging)
			{
				CurrentState = State.Warning;
				StartCoroutine(DoAttack());
			}
		}
	}

	public void DestroySpikes()
	{
		Object.Destroy(ParentToDestroy);
	}
}
