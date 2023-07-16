using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemySpawningEnemies : BaseMonoBehaviour
{
	public AssetReferenceGameObject EnemyPrefab;

	public int Limit = 5;

	public float SpawnDelay = 3f;

	public float ScaleUpDuration = 1f;

	public float DistanceFromPlayerToSpawn = 5f;

	public bool PlayAnimations;

	private SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string PreSpawnAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SpawnAnimation;

	private TrackEntry TrackEntry;

	public bool DoScaleUp;

	public bool DoTweenMove;

	public float TweenMoveDuration = 0.5f;

	public float TweenMoveTowardsPlayerDistance = 0.5f;

	private StateMachine state;

	private bool Spawning;

	private Health health;

	private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	private List<Health> ChildrenList = new List<Health>();

	private GameObject g;

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
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		health.OnDie += OnDie;
		Spawning = false;
	}

	private void OnDisable()
	{
		health.OnDie -= OnDie;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		foreach (Health children in ChildrenList)
		{
			children.DestroyNextFrame();
		}
	}

	private void Update()
	{
		if (Time.frameCount % 10 == 0 && !Spawning)
		{
			StateMachine.State cURRENT_STATE = state.CURRENT_STATE;
			if ((uint)cURRENT_STATE <= 1u)
			{
				Spawn();
			}
		}
	}

	public void Spawn()
	{
		if (ChildrenList.Count < Limit && PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > DistanceFromPlayerToSpawn)
		{
			StartCoroutine(SpawnRoutine());
		}
	}

	private IEnumerator SpawnRoutine()
	{
		Spawning = true;
		AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.transform.position);
		if (PlayAnimations)
		{
			TrackEntry = Spine.AnimationState.SetAnimation(0, PreSpawnAnimation, false);
			yield return new WaitForSeconds(TrackEntry.TrackEnd);
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(EnemyPrefab);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			loadedAddressableAssets.Add(obj);
			g = ObjectPool.Spawn(obj.Result, base.transform.parent, base.transform.position, Quaternion.identity);
			EnemyScuttleSwiper component = g.GetComponent<EnemyScuttleSwiper>();
			if (component != null)
			{
				component.AttackDelay = ScaleUpDuration + 0.5f;
			}
			DropLootOnDeath[] components = g.GetComponents<DropLootOnDeath>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].GiveXP = false;
			}
			DropMultipleLootOnDeath[] components2 = g.GetComponents<DropMultipleLootOnDeath>();
			for (int i = 0; i < components2.Length; i++)
			{
				components2[i].enabled = false;
			}
			Health component2 = g.GetComponent<Health>();
			component2.OnDie += OnSpawnedDie;
			ChildrenList.Add(component2);
			if (DoScaleUp)
			{
				component2.StartCoroutine(ScaleUp(g.transform));
			}
			if (DoTweenMove)
			{
				component2.StartCoroutine(MoveTweenRoutine(g.transform));
			}
			if (PlayAnimations)
			{
				TrackEntry = Spine.AnimationState.SetAnimation(0, SpawnAnimation, false);
			}
		};
		yield return new WaitForSeconds(SpawnDelay + (PlayAnimations ? 1.25f : 0f));
		Spawning = false;
	}

	private void OnSpawnedDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		Victim.OnDie -= OnSpawnedDie;
		ChildrenList.Remove(Victim);
	}

	private IEnumerator MoveTweenRoutine(Transform t)
	{
		yield return new WaitForEndOfFrame();
		float Progress = 0f;
		float Duration = TweenMoveDuration;
		Vector3 StartPosition = base.transform.position;
		Vector3 TargetPosition = base.transform.position + ((PlayerFarming.Instance == null) ? Vector3.zero : ((PlayerFarming.Instance.transform.position - base.transform.position) * TweenMoveTowardsPlayerDistance));
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			t.position = Vector3.Lerp(StartPosition, TargetPosition, Progress / Duration);
			yield return null;
		}
		t.position = TargetPosition;
	}

	private IEnumerator ScaleUp(Transform t)
	{
		yield return new WaitForEndOfFrame();
		float Progress = 0f;
		float Duration = ScaleUpDuration;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			t.localScale = Vector3.one * Mathf.SmoothStep(0.2f, 1f, Progress / Duration);
			yield return null;
		}
		t.localScale = Vector3.one;
	}
}
