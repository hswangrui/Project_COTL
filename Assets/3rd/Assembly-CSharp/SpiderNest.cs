using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpiderNest : BaseMonoBehaviour
{
	[Serializable]
	private enum DropType
	{
		None,
		Timed,
		Manual
	}

	[SerializeField]
	private DropType dropType;

	[SerializeField]
	private float dropGravity;

	[SerializeField]
	private AnimationCurve gravityCurve;

	[SerializeField]
	private float radius;

	[SerializeField]
	public AssetReferenceGameObject[] enemiesList;

	[SerializeField]
	public AssetReferenceGameObject[] ExtraEnemyList;

	[SerializeField]
	private bool random = true;

	[SerializeField]
	private Vector2 amount;

	[Space]
	[SerializeField]
	private GameObject unbroken;

	[SerializeField]
	private GameObject broken;

	[SerializeField]
	private GameObject breakParticle;

	private float gravity = -1f;

	private float dropTimer;

	private float shakeTimer;

	private bool dropping;

	private bool dropped;

	private bool enteredRoom;

	private List<KeyValuePair<EnemySpider, Vector3>> spawnedEnemies = new List<KeyValuePair<EnemySpider, Vector3>>();

	private static List<SpiderNest> activeNests = new List<SpiderNest>();

	private Queue<Health> spawnedEnemiesInDisabledMode = new Queue<Health>();

	private Vector2 shakeDelay = new Vector2(2f, 5f);

	private Vector2 tryToDropDelay = new Vector2(5f, 12f);

	public bool Droppable { get; set; } = true;


	private void Awake()
	{
	}

	private void Start()
	{
		if (dropType != 0)
		{
			PrewarmEnemies();
		}
		dropTimer = Time.time + UnityEngine.Random.Range(tryToDropDelay.x, tryToDropDelay.y);
		shakeTimer = Time.time + UnityEngine.Random.Range(shakeDelay.x, shakeDelay.y);
	}

	private void OnEnable()
	{
		if (!dropped && dropping)
		{
			dropping = false;
		}
		activeNests.Add(this);
		while (spawnedEnemiesInDisabledMode.Count > 0)
		{
			Health item = spawnedEnemiesInDisabledMode.Dequeue();
			Health.team2.Add(item);
		}
	}

	private void OnDisable()
	{
		activeNests.Remove(this);
	}

	private void PrewarmEnemies()
	{
		Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + base.transform.parent.position.z, base.transform.position.z - base.transform.parent.position.z);
		int num = (int)UnityEngine.Random.Range(amount.x, amount.y);
		AsyncOperationHandle<GameObject> asyncOperationHandle;
		if (random)
		{
			for (int i = 0; i < num; i++)
			{
				asyncOperationHandle = Addressables.InstantiateAsync(enemiesList[UnityEngine.Random.Range(0, enemiesList.Length)], vector + (Vector3)UnityEngine.Random.insideUnitCircle * radius, Quaternion.identity, base.transform.parent);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
				{
					EnemySpider component5 = obj.Result.GetComponent<EnemySpider>();
					Health component6 = component5.GetComponent<Health>();
					component5.enabled = false;
					component5.gameObject.SetActive(false);
					component6.enabled = false;
					if (base.gameObject.activeInHierarchy)
					{
						Health.team2.Add(component6);
					}
					else
					{
						spawnedEnemiesInDisabledMode.Enqueue(component6);
					}
					spawnedEnemies.Add(new KeyValuePair<EnemySpider, Vector3>(component5, Vector3.zero));
					Interaction_Chest instance3 = Interaction_Chest.Instance;
					if ((object)instance3 != null)
					{
						instance3.AddEnemy(component6);
					}
				};
			}
		}
		else
		{
			for (int j = 0; j < enemiesList.Length; j++)
			{
				asyncOperationHandle = Addressables.InstantiateAsync(enemiesList[j], vector + (Vector3)UnityEngine.Random.insideUnitCircle * radius, Quaternion.identity, base.transform.parent);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
				{
					EnemySpider component3 = obj.Result.GetComponent<EnemySpider>();
					Health component4 = component3.GetComponent<Health>();
					component3.enabled = false;
					component3.gameObject.SetActive(false);
					component4.enabled = false;
					if (base.gameObject.activeInHierarchy)
					{
						Health.team2.Add(component4);
					}
					else
					{
						spawnedEnemiesInDisabledMode.Enqueue(component4);
					}
					spawnedEnemies.Add(new KeyValuePair<EnemySpider, Vector3>(component3, Vector3.zero));
					Interaction_Chest instance2 = Interaction_Chest.Instance;
					if ((object)instance2 != null)
					{
						instance2.AddEnemy(component4);
					}
				};
			}
		}
		if (ExtraEnemyList == null || ExtraEnemyList.Length == 0)
		{
			return;
		}
		asyncOperationHandle = Addressables.InstantiateAsync(ExtraEnemyList[UnityEngine.Random.Range(0, ExtraEnemyList.Length)], vector + (Vector3)UnityEngine.Random.insideUnitCircle * radius, Quaternion.identity, base.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			EnemySpider component = obj.Result.GetComponent<EnemySpider>();
			Health component2 = component.GetComponent<Health>();
			component.enabled = false;
			component.gameObject.SetActive(false);
			component2.enabled = false;
			if (base.gameObject.activeInHierarchy)
			{
				Health.team2.Add(component2);
			}
			else
			{
				spawnedEnemiesInDisabledMode.Enqueue(component2);
			}
			spawnedEnemies.Add(new KeyValuePair<EnemySpider, Vector3>(component, Vector3.zero));
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.AddEnemy(component2);
			}
		};
	}

	private void Update()
	{
		if (!GameManager.RoomActive)
		{
			return;
		}
		if (!enteredRoom)
		{
			Shake();
			enteredRoom = true;
			dropTimer = Time.time + UnityEngine.Random.Range(tryToDropDelay.x, tryToDropDelay.y);
			shakeTimer = Time.time + UnityEngine.Random.Range(shakeDelay.x, shakeDelay.y);
		}
		if (!dropping && Time.time > shakeTimer)
		{
			Shake();
		}
		else if (!dropping && Time.time > dropTimer)
		{
			DropEnemies();
		}
		if (dropping && gravity != -1f)
		{
			gravity += dropGravity * Time.deltaTime;
			float time = gravity / 1f;
			for (int num = spawnedEnemies.Count - 1; num >= 0; num--)
			{
				if (spawnedEnemies[num].Key == null)
				{
					continue;
				}
				bool flag = false;
				Vector3 b = spawnedEnemies[num].Key.transform.TransformPoint(Vector3.zero);
				spawnedEnemies[num].Key.Spine.transform.position = Vector3.Lerp(spawnedEnemies[num].Value, b, gravityCurve.Evaluate(time));
				if (Vector3.Distance(spawnedEnemies[num].Key.Spine.transform.position, b) < 0.5f)
				{
					spawnedEnemies[num].Key.enabled = true;
					spawnedEnemies[num].Key.GetComponent<Health>().enabled = true;
					if (spawnedEnemies[num].Key.Spine.AnimationState == null)
					{
						continue;
					}
					spawnedEnemies[num].Key.Spine.AnimationState.SetAnimation(0, "land", false);
					spawnedEnemies[num].Key.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
					if (spawnedEnemies[num].Key.Spine.transform.parent == spawnedEnemies[num].Key)
					{
						spawnedEnemies[num].Key.Spine.transform.position = Vector3.zero;
					}
					else
					{
						spawnedEnemies[num].Key.Spine.transform.position = spawnedEnemies[num].Key.transform.TransformPoint(Vector3.zero);
					}
					spawnedEnemies[num].Key.transform.position = spawnedEnemies[num].Key.Spine.transform.position;
					spawnedEnemies[num].Key.Spine.transform.localPosition = Vector3.zero;
					Collider2D[] array = Physics2D.OverlapCircleAll(spawnedEnemies[num].Key.transform.position, 0.5f);
					foreach (Collider2D collider2D in array)
					{
						Health component = collider2D.GetComponent<Health>();
						if (component != null && component.team == Health.Team.Neutral)
						{
							collider2D.GetComponent<Health>().DealDamage(2.1474836E+09f, spawnedEnemies[num].Key.gameObject, Vector3.Lerp(component.transform.position, spawnedEnemies[num].Key.transform.position, 0.7f));
						}
					}
					flag = true;
				}
				if (flag)
				{
					spawnedEnemies.Remove(spawnedEnemies[num]);
				}
			}
			if (spawnedEnemies.Count == 0)
			{
				base.enabled = false;
			}
		}
		else if (!dropping && dropType != DropType.Manual && AllEnemiesDefeated())
		{
			if (dropType != 0)
			{
				DropEnemies();
				return;
			}
			dropping = true;
			StartCoroutine(ShakeIE());
		}
	}

	private bool AllEnemiesDefeated()
	{
		int num = 0;
		foreach (SpiderNest activeNest in activeNests)
		{
			num += activeNest.spawnedEnemies.Count;
		}
		int num2 = 0;
		foreach (Health item in Health.team2)
		{
			if (item != null)
			{
				num2++;
			}
		}
		return num2 <= num;
	}

	public void DropEnemies()
	{
		if (Droppable)
		{
			StartCoroutine(DropEnemiesIE());
		}
	}

	public void Shake()
	{
		StartCoroutine(ShakeIE());
	}

	private IEnumerator ShakeIE(float delay = 0f)
	{
		shakeTimer = Time.time + UnityEngine.Random.Range(shakeDelay.x, shakeDelay.y);
		Vector3 ogPosition = unbroken.transform.localPosition;
		Vector3 ogScale = unbroken.transform.localScale;
		yield return new WaitForSeconds(delay);
		unbroken.transform.DOShakeScale(1f, 0.025f, 10, 90f, false).SetEase(Ease.Linear);
		unbroken.transform.DOShakePosition(1f, 0.025f, 10, 90f, false, false).SetEase(Ease.Linear);
		yield return new WaitForSeconds(0.25f);
		unbroken.transform.localPosition = ogPosition;
		unbroken.transform.localScale = ogScale;
	}

	private IEnumerator DropEnemiesIE()
	{
		dropping = true;
		yield return StartCoroutine(ShakeIE());
		yield return new WaitForSeconds(0.3f);
		unbroken.transform.DOScaleX(1.25f, 0.6f).SetEase(Ease.InOutBounce);
		unbroken.transform.DOScaleY(0.75f, 0.6f).SetEase(Ease.InOutBounce);
		yield return new WaitForSeconds(0.6f);
		gravity = 0f;
		broken.SetActive(true);
		unbroken.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_impact", base.gameObject);
		for (int i = 0; i < spawnedEnemies.Count; i++)
		{
			if (spawnedEnemies[i].Key != null)
			{
				spawnedEnemies[i].Key.Spine.transform.position = base.transform.position;
				spawnedEnemies[i].Key.gameObject.SetActive(true);
				while (spawnedEnemies[i].Key.Spine.AnimationState == null)
				{
					yield return null;
				}
				spawnedEnemies[i].Key.Spine.AnimationState.SetAnimation(0, "falling", true);
				spawnedEnemies[i] = new KeyValuePair<EnemySpider, Vector3>(spawnedEnemies[i].Key, base.transform.position);
			}
		}
		UnityEngine.Object.Instantiate(breakParticle, base.transform.position, Quaternion.identity);
		dropped = true;
	}

	private void OnDrawGizmos()
	{
		Vector3 center = new Vector3(base.transform.position.x, base.transform.position.y + base.transform.parent.position.z, base.transform.position.z - base.transform.parent.position.z);
		center.z = 0f;
		Utils.DrawCircleXY(center, radius, Color.red);
	}
}
