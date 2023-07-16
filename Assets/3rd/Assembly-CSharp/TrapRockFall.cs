using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class TrapRockFall : BaseMonoBehaviour
{
	[SerializeField]
	private float playerDamage;

	[SerializeField]
	private float enemyDamage;

	[Space]
	[SerializeField]
	private float dropDelay;

	[SerializeField]
	private float dropSpeed;

	[SerializeField]
	private Vector2 rockTorque;

	[Space]
	[SerializeField]
	private GameObject rockContainer;

	[SerializeField]
	private GameObject[] rocks;

	[SerializeField]
	private GameObject pebbleContainer;

	[SerializeField]
	private SpriteRenderer[] pebbles;

	[SerializeField]
	private SpriteRenderer shadow;

	[SerializeField]
	private GameObject shadowToggle;

	[SerializeField]
	private GameObject debris;

	[SerializeField]
	private GameObject target;

	[SerializeField]
	private GameObject marking;

	[Space]
	[SerializeField]
	private CircleCollider2D collider;

	[SerializeField]
	private List<Sprite> particleChunks;

	[SerializeField]
	public float zSpawn;

	[SerializeField]
	public Material particleMaterial;

	[SerializeField]
	private ParticleSystem aoeParticles;

	[Space]
	[SerializeField]
	private UnityEvent onDrop;

	[SerializeField]
	private UnityEvent onLand;

	private bool dropped;

	private bool landed;

	private bool showDebris = true;

	private void OnDisable()
	{
		if (dropped && !landed)
		{
			aoeParticles.gameObject.SetActive(false);
			shadow.gameObject.SetActive(false);
			pebbleContainer.SetActive(false);
			rockContainer.SetActive(false);
			debris.SetActive(true);
			marking.SetActive(false);
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
		if (base.gameObject.activeInHierarchy)
		{
			if (!dropped)
			{
				dropped = true;
			}
			else if (dropped && !landed)
			{
				StopAllCoroutines();
				aoeParticles.gameObject.SetActive(false);
				shadow.gameObject.SetActive(false);
				pebbleContainer.SetActive(false);
				rockContainer.SetActive(false);
				debris.SetActive(false);
				marking.SetActive(true);
			}
			target.transform.DOScale(0f, 1f);
		}
	}

	public void Drop(bool showDebris = true)
	{
		this.showDebris = showDebris;
		StartCoroutine(DropIE());
	}

	private IEnumerator DropIE()
	{
		dropped = true;
		float increment = dropDelay / (float)pebbles.Length / 2f;
		shadowToggle.SetActive(true);
		shadow.transform.DOScale(Vector3.one * 3f, 4f);
		AudioManager.Instance.PlayOneShot("event:/material/stone_debris_fall", base.transform.position);
		pebbleContainer.SetActive(true);
		SpriteRenderer[] array = pebbles;
		foreach (SpriteRenderer pebble in array)
		{
			while (PlayerRelic.TimeFrozen)
			{
				yield return null;
			}
			pebble.transform.localPosition = Random.insideUnitCircle * 1.25f;
			pebble.transform.DOMoveZ(0f, Random.Range(dropDelay / 4f, dropDelay / 2f)).SetEase(Ease.OutBounce);
			pebble.transform.DOLocalRotate(new Vector3(0f, 0f, 1000f * Random.Range(1f, 3f)), dropDelay / 2f, RotateMode.LocalAxisAdd).OnComplete(delegate
			{
				pebble.DOFade(0f, Random.Range(0.25f, 0.5f));
			});
			yield return new WaitForSeconds(increment);
		}
		while (PlayerRelic.TimeFrozen)
		{
			yield return null;
		}
		UnityEvent unityEvent = onDrop;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		rockContainer.transform.DOMoveZ(0f, dropSpeed).SetSpeedBased().OnComplete(Landed)
			.SetEase(Ease.InSine);
		GameObject[] array2 = rocks;
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].transform.DOLocalRotate(new Vector3(0f, 0f, 360f) * Random.Range(rockTorque.x, rockTorque.y), dropSpeed, RotateMode.LocalAxisAdd);
		}
		shadow.DOFade(0f, 2.5f);
	}

	private void Landed()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, collider.radius);
		for (int i = 0; i < array.Length; i++)
		{
			Health component = array[i].GetComponent<Health>();
			if ((bool)component && (component.team != Health.Team.PlayerTeam || !TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps)) && !component.ImmuneToTraps)
			{
				component.DealDamage((component.team == Health.Team.PlayerTeam) ? playerDamage : enemyDamage, base.gameObject, base.transform.position);
			}
		}
		AudioManager.Instance.PlayOneShot("event:/material/stone_break", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/land_large", base.transform.position);
		rockContainer.SetActive(false);
		debris.SetActive(showDebris);
		marking.SetActive(false);
		shadowToggle.SetActive(false);
		int num = -1;
		if (particleChunks.Count > 0)
		{
			while (++num < 10)
			{
				if (particleMaterial == null)
				{
					Particle_Chunk.AddNew(base.transform.position, Random.Range(0, 360), particleChunks);
				}
				else
				{
					Particle_Chunk.AddNewMat(base.transform.position, Random.Range(0, 360), particleChunks, -1, particleMaterial);
				}
			}
		}
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.2f, false);
		aoeParticles.Play();
		UnityEvent unityEvent = onLand;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		landed = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!dropped && other.gameObject.tag == "Player")
		{
			StartCoroutine(DropIE());
		}
	}
}
