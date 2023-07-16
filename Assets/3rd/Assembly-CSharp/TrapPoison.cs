using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TrapPoison : BaseMonoBehaviour
{
	public Collider2D collider;

	public Transform graphic;

	[SerializeField]
	private ParticleSystem particleSystem;

	public float lifetime;

	public float spawnDuration = 0.25f;

	public float despawnDuration = 0.25f;

	[Space]
	[SerializeField]
	private Sprite[] sprites;

	public Health.Team team;

	private float spawnTimestamp;

	private bool isSpawning;

	private bool isDespawning;

	private static GameObject trapPoisonGO;

	private List<Health> victims = new List<Health>();

	public static List<TrapPoison> ActivePoison = new List<TrapPoison>();

	public bool useMeshGoop;

	[SerializeField]
	private Mesh[] goopMeshes;

	public MeshRenderer meshRenderer;

	public MeshFilter meshFilter;

	private static List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	public static void CreatePoison(Vector3 position, int amount, float radius, Transform parent)
	{
		if (trapPoisonGO == null)
		{
			AsyncOperationHandle<GameObject> loadOp = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/Misc/Trap Poison.prefab");
			loadOp.Completed += delegate
			{
				trapPoisonGO = loadOp.Result;
				for (int j = 0; j < amount; j++)
				{
					Vector3 position3 = position + (Vector3)Random.insideUnitCircle * radius;
					position3.z = 0f;
					ObjectPool.Spawn(trapPoisonGO, parent, position3, Quaternion.identity);
				}
			};
		}
		else
		{
			for (int i = 0; i < amount; i++)
			{
				Vector3 position2 = position + (Vector3)Random.insideUnitCircle * radius;
				position2.z = 0f;
				ObjectPool.Spawn(trapPoisonGO, parent, position2, Quaternion.identity);
			}
		}
	}

	private void OnDestroy()
	{
		if (loadedAddressableAssets == null)
		{
			return;
		}
		foreach (AsyncOperationHandle<GameObject> loadedAddressableAsset in loadedAddressableAssets)
		{
			Addressables.Release((AsyncOperationHandle)loadedAddressableAsset);
		}
		loadedAddressableAssets.Clear();
	}

	private void OnEnable()
	{
		if (goopMeshes.Length != 0)
		{
			meshFilter.mesh = goopMeshes[Random.Range(0, goopMeshes.Length)];
		}
		meshRenderer.gameObject.SetActive(true);
		graphic.DOComplete();
		graphic.transform.localScale = Vector3.zero;
		graphic.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
		isSpawning = true;
		isDespawning = false;
		collider.enabled = false;
		graphic.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
		if ((bool)GameManager.GetInstance())
		{
			spawnTimestamp = GameManager.GetInstance().CurrentTime - Random.Range(0.1f, 0.3f);
		}
		ActivePoison.Add(this);
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
		if (PlayerFarming.Instance != null && (LetterBox.IsPlaying || PlayerFarming.Instance._state.CURRENT_STATE == StateMachine.State.CustomAnimation))
		{
			DespawnPoison();
		}
		if (!isDespawning && GameManager.GetInstance().TimeSince(spawnTimestamp) >= spawnDuration + lifetime)
		{
			DespawnPoison();
		}
		if (isDespawning && GameManager.GetInstance().TimeSince(spawnTimestamp) >= spawnDuration + lifetime + despawnDuration)
		{
			ObjectPool.Recycle(base.gameObject);
		}
	}

	private void DespawnPoison()
	{
		if ((bool)particleSystem)
		{
			particleSystem.Pause();
		}
		ActivePoison.Remove(this);
		isDespawning = true;
		collider.enabled = false;
		spawnTimestamp = 0f;
		graphic.DOComplete();
		graphic.transform.localScale = Vector3.one;
		graphic.transform.DOScale(0f, 0.5f).SetEase(Ease.InSine);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.layer == 14 || collider.gameObject.layer == 9)
		{
			Health component = collider.GetComponent<Health>();
			if (component != null && component.team != team && (component.team != Health.Team.PlayerTeam || !(PlayerFarming.Instance != null) || (!LetterBox.IsPlaying && PlayerFarming.Instance._state.CURRENT_STATE != StateMachine.State.CustomAnimation)))
			{
				component.AddPoison(base.gameObject);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != team)
		{
			component.RemovePoison();
		}
	}

	public static void RemoveAllPoison()
	{
		for (int num = ActivePoison.Count - 1; num >= 0; num--)
		{
			if (ActivePoison[num] != null)
			{
				ActivePoison[num].DespawnPoison();
			}
		}
		PlayerFarming.Instance.health.ClearPoison();
	}
}
