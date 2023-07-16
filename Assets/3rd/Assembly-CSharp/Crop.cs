using System.Collections.Generic;
using UnityEngine;

public class Crop : BaseMonoBehaviour
{
	public static List<Crop> Crops = new List<Crop>();

	public Sprite[] sprites;

	public SpriteRenderer image;

	private float rotateSpeedY;

	private float rotateY;

	private float delay;

	private float WorkRequired = 5f;

	private Health health;

	[HideInInspector]
	public bool WorkCompleted;

	[HideInInspector]
	public GameObject Reserved;

	private StructuresData CropInfo;

	private Animator animator;

	public AnimationClip a;

	private float progress;

	private float Progress
	{
		get
		{
			return progress;
		}
		set
		{
			progress = value;
			if (progress >= 1f)
			{
				progress = 1f;
			}
		}
	}

	private void Start()
	{
		animator = GetComponentInChildren<Animator>();
		Structure component = GetComponent<Structure>();
		CropInfo = component.Structure_Info;
		SetImage();
	}

	private void OnEnable()
	{
		Crops.Add(this);
	}

	private void OnDisable()
	{
		Crops.Remove(this);
	}

	public void DoWork(float WorkDone)
	{
		CropInfo.Progress += WorkDone;
		if (CropInfo.Progress >= WorkRequired)
		{
			WorkCompleted = true;
			CropInfo.Progress = WorkRequired;
		}
		SetImage();
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
	}

	public void SetImage()
	{
		Progress = CropInfo.Progress / WorkRequired;
		animator.speed = 0f;
		animator.Play("CropGrow", 0, Progress);
		if (Progress >= 1f && base.gameObject.GetComponent<Health>() == null)
		{
			health = base.gameObject.AddComponent<Health>();
			health.OnDie += OnDie;
			GetComponent<DropLootOnDeath>().SetHealth();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (delay < 0f)
		{
			delay = 0.5f;
			rotateSpeedY = (10f + (float)Random.Range(-2, 2)) * (float)((!(collision.transform.position.x < base.transform.position.x)) ? 1 : (-1));
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (delay < 0f)
		{
			delay = 0.5f;
			rotateSpeedY = (10f + (float)Random.Range(-2, 2)) * (float)((!(collision.transform.position.x < base.transform.position.x)) ? 1 : (-1));
		}
	}

	private void Update()
	{
		delay -= Time.deltaTime;
		rotateSpeedY += (0f - rotateY) * 0.1f;
		rotateY += (rotateSpeedY *= 0.8f);
		image.gameObject.transform.eulerAngles = new Vector3(-90f, rotateY, 0f);
	}

	private void onDestroy()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
	}
}
