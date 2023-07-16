using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TrapGoop : BaseMonoBehaviour, ICurseProduct
{
	public Collider2D collider;

	public SpriteRenderer spriteRenderer;

	public Transform graphics;

	public float lifetime;

	public float spawnDuration = 0.25f;

	public float despawnDuration = 0.25f;

	[Space]
	[SerializeField]
	private Sprite[] sprites;

	public bool useMeshGoop;

	[SerializeField]
	private Mesh[] goopMeshes;

	public MeshRenderer meshRenderer;

	public MeshFilter meshFilter;

	private float spawnTimestamp;

	private bool isSpawning;

	private bool isDespawning;

	private static List<TrapGoop> activeGoop = new List<TrapGoop>();

	public float DamageMultiplier { get; set; } = 1f;


	public float TickDurationMultiplier { get; set; } = 1f;


	public static void CreateGoop(Vector3 position, int amount, float radius, Transform parent)
	{
		for (int i = 0; i < amount; i++)
		{
			Addressables.InstantiateAsync("Assets/Prefabs/Enemies/Misc/Goop.prefab", position + (Vector3)Random.insideUnitCircle * radius, Quaternion.identity, parent);
		}
	}

	private void OnEnable()
	{
		if (useMeshGoop)
		{
			spriteRenderer.enabled = false;
			if (goopMeshes.Length != 0)
			{
				meshFilter.mesh = goopMeshes[Random.Range(0, goopMeshes.Length)];
			}
			meshRenderer.gameObject.SetActive(true);
		}
		else
		{
			meshRenderer.gameObject.SetActive(false);
			if (sprites.Length != 0)
			{
				spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
			}
		}
		graphics.DOComplete();
		graphics.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
		graphics.transform.localScale = Vector3.zero;
		graphics.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
		isSpawning = true;
		isDespawning = false;
		collider.enabled = false;
		if ((bool)GameManager.GetInstance())
		{
			spawnTimestamp = GameManager.GetInstance().CurrentTime - Random.Range(0.1f, 0.3f);
		}
		activeGoop.Add(this);
	}

	private void OnDisable()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (isSpawning)
		{
			if (!(GameManager.GetInstance().TimeSince(spawnTimestamp) >= spawnDuration))
			{
				return;
			}
			isSpawning = false;
			collider.enabled = true;
		}
		if (!isDespawning && GameManager.GetInstance().TimeSince(spawnTimestamp) >= spawnDuration + lifetime)
		{
			DespawnGoop();
		}
		if (isDespawning && GameManager.GetInstance().TimeSince(spawnTimestamp) >= spawnDuration + lifetime + despawnDuration)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void DespawnGoop()
	{
		activeGoop.Remove(this);
		isDespawning = true;
		collider.enabled = false;
		graphics.DOComplete();
		graphics.transform.localScale = Vector3.one;
		graphics.transform.DOScale(0f, 0.25f).SetEase(Ease.InSine);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team == Health.Team.Team2)
		{
			component.enemyPoisonDamage = 0.2f * DamageMultiplier;
			component.poisonTickDuration = 1f * TickDurationMultiplier;
			component.AddPoison(base.gameObject);
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team == Health.Team.Team2)
		{
			component.RemovePoison();
		}
	}

	public static void RemoveAllGoop()
	{
		for (int num = activeGoop.Count - 1; num >= 0; num--)
		{
			if (activeGoop[num] != null)
			{
				activeGoop[num].DespawnGoop();
			}
		}
	}
}
