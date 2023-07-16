using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LongGrass : BaseMonoBehaviour
{
	[Serializable]
	public class GrassObject
	{
		public Transform transform;

		public float Rotation;

		public float RotationSpeed;

		public SpriteRenderer spriteRederer;
	}

	public List<GrassObject> GrassObjects = new List<GrassObject>();

	public List<Sprite> CutGrassSprites = new List<Sprite>();

	public List<Sprite> NonCutGrassSprites = new List<Sprite>();

	public Health health;

	public BiomeConstants.TypeOfParticle particleType = BiomeConstants.TypeOfParticle.grass;

	public int FrameIntervalOffset;

	public int UpdateInterval = 2;

	[SerializeField]
	private Material grassNormal;

	[SerializeField]
	private Material grassCut;

	[SerializeField]
	private Collider2D c;

	public bool TURN_OFF_ON_LOW_QUALITY;

	public Vector3 Position;

	public bool Dead;

	public float ShakeModifier = 2f;

	private List<GameObject> CurrentCollisions = new List<GameObject>();

	private List<GameObject> PreviousCollisions = new List<GameObject>();

	private Coroutine cShakeGrassRoutine;

	private float Progress;

	private float Duration;

	private List<Tween> resetTweens = new List<Tween>();

	[Range(0f, 1f)]
	public float v1 = 0.1f;

	[Range(0f, 1f)]
	public float v2 = 0.9f;

	private void OnEnable()
	{
		if (TURN_OFF_ON_LOW_QUALITY && SettingsManager.Settings != null && SettingsManager.Settings.Graphics.EnvironmentDetail == 0)
		{
			TurnOffEverything();
		}
		StartCoroutine(OnEnableIE());
	}

	private void TurnOffEverything()
	{
		if (health != null)
		{
			health.enabled = false;
		}
		if (c != null)
		{
			c.enabled = false;
		}
		base.enabled = false;
	}

	private IEnumerator OnEnableIE()
	{
		if (health == null)
		{
			health = GetComponent<Health>();
		}
		if (health != null)
		{
			health.OnDie += OnDie;
		}
		CurrentCollisions.Clear();
		PreviousCollisions.Clear();
		if (cShakeGrassRoutine != null)
		{
			StopCoroutine(cShakeGrassRoutine);
		}
		yield return null;
		Position = base.transform.position;
		TimeManager.AddToRegion(this);
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
		foreach (GrassObject grassObject in GrassObjects)
		{
			grassObject.Rotation = 0f;
			grassObject.RotationSpeed = 0f;
		}
		CurrentCollisions.Clear();
		PreviousCollisions.Clear();
		TimeManager.RemoveLongGrass(this);
	}

	private void OnDestroy()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
		TimeManager.RemoveLongGrass(this);
		GrassObjects.Clear();
		CutGrassSprites.Clear();
		NonCutGrassSprites.Clear();
	}

	private void Start()
	{
		FrameIntervalOffset = UnityEngine.Random.Range(0, UpdateInterval);
	}

	private void DropHeartChance()
	{
		int num = 0;
		if (DifficultyManager.PrimaryDifficulty == DifficultyManager.Difficulty.Easy)
		{
			num = 25;
		}
		else if (DifficultyManager.PrimaryDifficulty == DifficultyManager.Difficulty.Medium)
		{
			num = 40;
		}
		if (Interaction_BlueHeart.Hearts.Count + Interaction_RedHeart.Hearts.Count > 1)
		{
			num *= 3;
		}
		if (num != 0 && UnityEngine.Random.Range(0, num) == 17)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.HALF_HEART, 1, base.transform.position);
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		health.OnDie -= OnDie;
		Dead = true;
		if (PlayerFarming.Instance != null && DifficultyManager.AssistModeEnabled && PlayerFarming.Instance.health.HP + PlayerFarming.Instance.health.SpiritHearts + PlayerFarming.Instance.health.BlueHearts == 1f)
		{
			DropHeartChance();
		}
		foreach (GrassObject grassObject in GrassObjects)
		{
			if (grassObject != null && grassObject.transform != null)
			{
				grassObject.transform.eulerAngles = new Vector3(-60f, 0f, 0f);
			}
		}
		BiomeConstants.Instance.EmitParticleChunk(particleType, base.transform.position, (Attacker != null) ? (AttackLocation - Attacker.transform.position) : Vector3.zero, 10);
		AudioManager.Instance.PlayOneShot("event:/player/tall_grass_cut", base.gameObject);
		if (CutGrassSprites.Count > 0)
		{
			foreach (GrassObject grassObject2 in GrassObjects)
			{
				if (grassObject2 != null && grassObject2.spriteRederer != null && CutGrassSprites.Count > 0)
				{
					grassObject2.spriteRederer.sprite = CutGrassSprites[UnityEngine.Random.Range(0, CutGrassSprites.Count)];
					if (grassCut != null)
					{
						grassObject2.spriteRederer.material = grassCut;
					}
				}
			}
		}
		else
		{
			base.gameObject.Recycle();
		}
		base.enabled = false;
	}

	public void SetCut()
	{
		if (this == null || base.gameObject == null)
		{
			return;
		}
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.enabled = false;
		}
		if (c == null)
		{
			c = GetComponent<Collider2D>();
		}
		if (c != null)
		{
			c.enabled = false;
		}
		if (CutGrassSprites.Count > 0)
		{
			foreach (GrassObject grassObject in GrassObjects)
			{
				if (grassObject != null && grassObject.spriteRederer != null && CutGrassSprites.Count > 0)
				{
					if ((bool)grassObject.spriteRederer.GetComponent<RandomSpritePicker>())
					{
						grassObject.spriteRederer.GetComponent<RandomSpritePicker>().enabled = false;
					}
					grassObject.spriteRederer.sprite = CutGrassSprites[UnityEngine.Random.Range(0, CutGrassSprites.Count)];
					if (grassCut != null)
					{
						grassObject.spriteRederer.material = grassCut;
					}
				}
			}
		}
		foreach (GrassObject grassObject2 in GrassObjects)
		{
			if (grassObject2 != null)
			{
				grassObject2.transform.eulerAngles = new Vector3(-60f, 0f, 0f);
			}
		}
		Dead = true;
		base.enabled = false;
	}

	public void ResetCut()
	{
		health.OnDie += OnDie;
		health.enabled = true;
		health.HP = health.totalHP;
		if (c == null)
		{
			c = GetComponent<Collider2D>();
		}
		c.enabled = true;
		Dead = false;
		if (NonCutGrassSprites.Count > 0)
		{
			for (int i = 0; i < GrassObjects.Count; i++)
			{
				if (i < GrassObjects.Count - 1 && GrassObjects != null && GrassObjects[i].spriteRederer != null && NonCutGrassSprites.Count > 0)
				{
					if ((bool)GrassObjects[i].spriteRederer.GetComponent<RandomSpritePicker>())
					{
						GrassObjects[i].spriteRederer.GetComponent<RandomSpritePicker>().enabled = true;
					}
					GrassObjects[i].spriteRederer.sprite = NonCutGrassSprites[i];
					if (grassNormal != null)
					{
						GrassObjects[i].spriteRederer.material = grassNormal;
					}
				}
			}
		}
		base.enabled = true;
	}

	private IEnumerator EmitGrassParticles(GameObject Attacker)
	{
		Vector3 position = Attacker.transform.position;
		Vector3 LastPos = position;
		yield return null;
		if (!(Attacker != null))
		{
			Vector3 zero = Vector3.zero;
		}
		else
		{
			Vector3 vector = (position - LastPos) * 50f;
		}
	}

	public void Colliding(GameObject g)
	{
		if (!CurrentCollisions.Contains(g))
		{
			CurrentCollisions.Add(g);
		}
	}

	private void LateUpdate()
	{
		if (PerformanceTest.ReduceCPU || TURN_OFF_ON_LOW_QUALITY || (Time.frameCount + FrameIntervalOffset) % UpdateInterval != 0)
		{
			return;
		}
		foreach (GameObject currentCollision in CurrentCollisions)
		{
			if (!PreviousCollisions.Contains(currentCollision) && currentCollision != null)
			{
				CollisionEnter(currentCollision);
			}
		}
		foreach (GameObject previousCollision in PreviousCollisions)
		{
			if (!CurrentCollisions.Contains(previousCollision) && previousCollision != null)
			{
				CollisionExit(previousCollision);
			}
		}
		List<GameObject> previousCollisions = PreviousCollisions;
		PreviousCollisions = CurrentCollisions;
		CurrentCollisions = previousCollisions;
		CurrentCollisions.Clear();
	}

	private void CollisionEnter(GameObject collision)
	{
		if (!Dead)
		{
			AudioManager.Instance.PlayOneShot("event:/material/footstep_bush", collision.transform.position);
			AudioManager.Instance.PlayOneShot("event:/player/tall_grass_push", collision.transform.position);
			if (cShakeGrassRoutine != null)
			{
				StopCoroutine(cShakeGrassRoutine);
			}
			cShakeGrassRoutine = StartCoroutine(ShakeGrassRoutine(collision));
		}
	}

	private void CollisionExit(GameObject collision)
	{
		if (!Dead)
		{
			if (cShakeGrassRoutine != null)
			{
				StopCoroutine(cShakeGrassRoutine);
			}
			cShakeGrassRoutine = StartCoroutine(ShakeGrassRoutine(collision));
		}
	}

	public IEnumerator ShakeGrassRoutine(GameObject collision, float clampDistance = -1f)
	{
		if (collision == null)
		{
			yield break;
		}
		foreach (GrassObject grassObject in GrassObjects)
		{
			if (grassObject != null && grassObject.transform != null)
			{
				Vector3 position = grassObject.transform.position;
				Vector3 position2 = collision.transform.position;
				float num = ((clampDistance == -1f) ? Vector3.Distance(position, position2) : Mathf.Clamp(Vector3.Distance(position, position2), 0f, clampDistance));
				grassObject.RotationSpeed += (10f + (float)UnityEngine.Random.Range(-2, 2)) * (float)((!(position.x < position2.x)) ? 1 : (-1)) * ShakeModifier * (1f - num / 1f);
			}
		}
		Progress = 0f;
		Duration = 1f;
		while ((Progress += Time.fixedDeltaTime) < Duration)
		{
			if (Dead)
			{
				yield break;
			}
			foreach (GrassObject grassObject2 in GrassObjects)
			{
				if (Time.deltaTime > 0f && grassObject2 != null && grassObject2.transform != null)
				{
					grassObject2.RotationSpeed += (0f - grassObject2.Rotation) * v1 / (Time.fixedDeltaTime * 60f);
					grassObject2.Rotation += (grassObject2.RotationSpeed *= v2) * (Time.fixedDeltaTime * 60f);
					grassObject2.transform.eulerAngles = new Vector3(-60f, 0f, grassObject2.Rotation);
				}
			}
			yield return new WaitForFixedUpdate();
		}
		foreach (Tween resetTween in resetTweens)
		{
			resetTween.Complete();
		}
		resetTweens.Clear();
		foreach (GrassObject grassObject3 in GrassObjects)
		{
			if (Mathf.Abs(grassObject3.Rotation) > 0.2f)
			{
				grassObject3.RotationSpeed = 0f;
				grassObject3.Rotation = 0f;
				resetTweens.Add(grassObject3.transform.DORotate(new Vector3(-60f, 0f, 0f), UnityEngine.Random.Range(0f, 1f)).SetEase(Ease.OutBounce));
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Projectile" || collision.tag == "ProjectileFly")
		{
			Colliding(collision.gameObject);
		}
	}
}
