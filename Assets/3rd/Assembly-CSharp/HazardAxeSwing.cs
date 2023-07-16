using DG.Tweening;
using UnityEngine;

public class HazardAxeSwing : BaseMonoBehaviour
{
	[SerializeField]
	private Transform swingingAxeTransform;

	[SerializeField]
	private Transform swingingAxeShadow;

	[SerializeField]
	private Transform shadowFollowTransform;

	[SerializeField]
	private float swingDist = 30f;

	[SerializeField]
	private float swingSpeed = 1f;

	[SerializeField]
	private int enemyDamage = 3;

	[SerializeField]
	private ParticleSystem movementParticles;

	[SerializeField]
	private ParticleSystem impactParticles;

	[SerializeField]
	private float axeSwingDangerousAngle = 10f;

	private Vector3 axeRotation;

	private float axeSwingTime;

	private bool axeSwingDangerous;

	private void FixedUpdate()
	{
		if (PlayerRelic.TimeFrozen)
		{
			return;
		}
		axeSwingTime += Time.deltaTime;
		if (swingingAxeTransform != null)
		{
			axeRotation = swingingAxeTransform.rotation.eulerAngles;
			axeRotation.y = Mathf.Sin(axeSwingTime * swingSpeed) * swingDist;
			if (Mathf.Abs(axeRotation.y) < axeSwingDangerousAngle)
			{
				if (!axeSwingDangerous)
				{
					AudioManager.Instance.PlayOneShot("event:/weapon/melee_swing_slow", AudioManager.Instance.Listener);
					axeSwingDangerous = true;
				}
				if (movementParticles != null)
				{
					movementParticles.Play();
				}
			}
			else
			{
				axeSwingDangerous = false;
			}
			swingingAxeTransform.rotation = Quaternion.Euler(axeRotation);
		}
		if (swingingAxeShadow != null)
		{
			Vector3 position = swingingAxeShadow.position;
			position.x = shadowFollowTransform.position.x;
			if (swingingAxeShadow != null)
			{
				swingingAxeShadow.position = position;
			}
		}
	}

	private void Start()
	{
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void OnDestroy()
	{
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	private void RoomLockController_OnRoomCleared()
	{
		if (swingingAxeTransform != null)
		{
			swingingAxeTransform.DOKill();
			swingingAxeTransform.DOMoveZ(swingingAxeTransform.position.z - 10f, 1.5f).SetEase(Ease.InQuart).OnComplete(delegate
			{
				base.gameObject.SetActive(false);
			});
		}
		if (swingingAxeShadow != null)
		{
			swingingAxeShadow.DOScale(Vector3.zero, 1.5f).SetEase(Ease.InQuart);
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		Debug.Log("Collider made contact");
		if (!axeSwingDangerous)
		{
			return;
		}
		Health component = collision.GetComponent<Health>();
		if (component != null && (component.team != Health.Team.PlayerTeam || !TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps)) && !component.ImmuneToTraps && (!component.isPlayer || !TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps)))
		{
			component.DealDamage((component.team != Health.Team.Team2) ? 1 : enemyDamage, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 2f));
			if (impactParticles != null)
			{
				impactParticles.Play();
			}
		}
	}
}
