using System;
using DG.Tweening;
using UnityEngine;

public class HazardFireSpinner : MonoBehaviour
{
	public Vector3 spinSpeed = new Vector3(0f, 0f, 60f);

	public Vector3 initialRotation = new Vector3(0f, 0f, 0f);

	public Transform flameRotatorTransform;

	public Transform[] flames;

	public float playerDamage = 1f;

	public float enemyDamage = 1f;

	public GameObject TrapOn;

	public GameObject TrapOff;

	private bool destroyed;

	private void Start()
	{
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Combine(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(ChestAppeared));
		flameRotatorTransform.Rotate(initialRotation);
		for (int i = 0; i < flames.Length; i++)
		{
			flames[i].transform.Rotate(-initialRotation);
		}
	}

	private void ChestAppeared()
	{
		destroyed = true;
		for (int i = 0; i < flames.Length; i++)
		{
			Transform obj = flames[i];
			obj.transform.DOScale(0f, 1f);
			ParticleSystem componentInChildren = obj.GetComponentInChildren<ParticleSystem>();
			if (componentInChildren != null)
			{
				componentInChildren.Stop();
			}
			UnityEngine.Object.Destroy(obj.gameObject, 1f);
		}
		TrapOn.SetActive(false);
		TrapOff.SetActive(true);
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(ChestAppeared));
	}

	private void OnDestroy()
	{
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(ChestAppeared));
	}

	private void FixedUpdate()
	{
		if (!destroyed && !PlayerRelic.TimeFrozen)
		{
			flameRotatorTransform.Rotate(spinSpeed * Time.deltaTime);
			for (int i = 0; i < flames.Length; i++)
			{
				flames[i].transform.Rotate(-spinSpeed * Time.deltaTime);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		UnitObject component = collision.GetComponent<UnitObject>();
		if (component != null && component.health != null)
		{
			if (component.health.team == Health.Team.PlayerTeam && !TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps))
			{
				component.health.DealDamage(playerDamage, base.gameObject, base.transform.position);
			}
			else if (!component.isFlyingEnemy)
			{
				component.health.DealDamage(playerDamage, base.gameObject, base.transform.position);
			}
		}
	}
}
