using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Tree : BaseMonoBehaviour
{
	private Health health;

	private float rotateY;

	private float rotateSpeedY;

	public GameObject image;

	public float RotationToCamera = -60f;

	public GameObject lighting;

	public GameObject halo;

	public SkeletonAnimation Spine;

	public Sprite Image;

	public Sprite ImageStump;

	public SpriteRenderer spriteRenderer;

	public CircleCollider2D collider;

	[HideInInspector]
	public bool Dead;

	[HideInInspector]
	public GameObject TaskDoer;

	public static List<Tree> Trees = new List<Tree>();

	public ParticleSystem[] particleSystems;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
	}

	private void OnEnable()
	{
		Trees.Add(this);
	}

	private void OnDisable()
	{
		Trees.Remove(this);
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (health.HP > health.totalHP / 2f)
		{
			Spine.skeleton.SetSkin("normal-chop1");
		}
		else
		{
			Spine.skeleton.SetSkin("normal-chop2");
		}
		BiomeConstants.Instance.EmitHitVFX(base.transform.position - Vector3.down * Random.Range(0.2f, 1.5f), Quaternion.identity.z, "HitFX_Weak");
		if (Spine != null)
		{
			Spine.AnimationState.SetAnimation(0, "hit", true);
			Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (lighting != null)
		{
			Object.Destroy(lighting);
		}
		if (halo != null)
		{
			Object.Destroy(halo);
		}
		rotateSpeedY = (5f + (float)Random.Range(-2, 2)) * (float)((!(Attacker.transform.position.x < base.transform.position.x)) ? 1 : (-1));
		CameraManager.shakeCamera(0.25f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		if (DungeonDecorator.getInsance() != null)
		{
			DungeonDecorator.getInsance().UpdateStructures(NavigateRooms.r, base.transform.position, 5);
		}
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = ImageStump;
		}
		if (collider != null)
		{
			collider.isTrigger = true;
		}
		if (Spine != null)
		{
			Spine.skeleton.SetSkin("cut");
			Spine.skeleton.SetSlotsToSetupPose();
		}
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
		Dead = true;
		TaskDoer = null;
	}

	public void StartAsStump()
	{
		Object.Destroy(GetComponent<Health>());
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = ImageStump;
		}
		if (Spine != null)
		{
			Spine.skeleton.SetSkin("cut");
			Spine.skeleton.SetSlotsToSetupPose();
		}
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
		Dead = true;
	}

	private void Update()
	{
		rotateSpeedY += (0f - rotateY) * 0.1f;
		rotateY += (rotateSpeedY *= 0.8f);
		if (image != null)
		{
			image.transform.eulerAngles = new Vector3(RotationToCamera, rotateY, 0f);
		}
		if (Spine != null)
		{
			Spine.transform.eulerAngles = new Vector3(RotationToCamera, rotateY, 0f);
		}
	}

	private void onDestroy()
	{
		health.OnHit -= OnHit;
		health.OnDie -= OnDie;
	}
}
