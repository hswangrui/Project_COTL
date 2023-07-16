using System;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Chunk : BaseMonoBehaviour
{
	private float vz;

	private float childZ;

	private float vx;

	private float vy;

	private float Speed;

	private float Scale;

	private float FacingAngle;

	public GameObject child;

	public SpriteRenderer blood;

	private float BloodTimer;

	private float Timer;

	public bool scaleObjectOut = true;

	private void Start()
	{
		Scale = 2f;
		vz = UnityEngine.Random.Range(-0.15f, -0.1f);
		childZ = UnityEngine.Random.Range(-0.7f, -0.3f);
		Speed = UnityEngine.Random.Range(3, 5);
		Timer = 0f;
	}

	public static void AddNew(Vector3 position, float FacingAngle, List<Sprite> frames = null, int frame = -1, bool scaleObjectOut = true)
	{
		Particle_Chunk particle_Chunk = BiomeConstants.Instance.SpawnParticleChunk(position);
		particle_Chunk.FacingAngle = FacingAngle;
		particle_Chunk.scaleObjectOut = scaleObjectOut;
		if (frames != null)
		{
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().frames = frames;
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().frame = frame;
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().Start();
			particle_Chunk.Start();
		}
	}

	public static void AddNew(Vector3 position, float FacingAngle, Color color, List<Sprite> frames = null, int frame = -1, bool scaleObjectOut = true)
	{
		Particle_Chunk particle_Chunk = BiomeConstants.Instance.SpawnParticleChunk(position);
		particle_Chunk.FacingAngle = FacingAngle;
		particle_Chunk.scaleObjectOut = scaleObjectOut;
		particle_Chunk.child.GetComponent<SpriteRenderer>().color = color;
		if (frames != null)
		{
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().frames = frames;
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().frame = frame;
			particle_Chunk.Start();
		}
	}

	public static void AddNewMat(Vector3 position, float FacingAngle, List<Sprite> frames = null, int frame = -1, Material mat = null, bool scaleObjectOut = true)
	{
		Particle_Chunk particle_Chunk = BiomeConstants.Instance.SpawnParticleChunk(position);
		particle_Chunk.FacingAngle = FacingAngle;
		particle_Chunk.FacingAngle = FacingAngle;
		if (frames != null)
		{
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().mat = mat;
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().frames = frames;
			particle_Chunk.gameObject.GetComponentInChildren<RandomFrame>().frame = frame;
		}
	}

	private void FixedUpdate()
	{
		BounceChild();
		if (scaleObjectOut)
		{
			if ((Timer += Time.fixedDeltaTime) > 1f)
			{
				if ((Scale -= 0.1f * Time.fixedDeltaTime * 60f) <= 0f)
				{
					base.gameObject.Recycle();
				}
			}
			else
			{
				Scale += (1f - Scale) / 4f;
			}
		}
		else if ((Timer += Time.fixedDeltaTime) > 5f)
		{
			Speed += (0f - Speed) / 50f / (Time.fixedDeltaTime * 60f);
		}
		else
		{
			Speed = 0f;
		}
		child.transform.localScale = new Vector3(Scale, Scale, Scale);
		child.transform.Rotate(new Vector3(0f, 0f, Speed * (Time.fixedDeltaTime * 60f) * 6f * (float)((!(FacingAngle < 90f) || !(FacingAngle > -90f)) ? 1 : (-1))));
	}

	private void Update()
	{
		vx = Speed * Mathf.Cos(FacingAngle * ((float)Math.PI / 180f));
		vy = Speed * Mathf.Sin(FacingAngle * ((float)Math.PI / 180f));
		base.transform.position = base.transform.position + new Vector3(vx, vy) * Time.deltaTime;
	}

	private void BounceChild()
	{
		if (childZ >= 0f)
		{
			if (vz > 0.05f)
			{
				Speed *= 0.7f;
				vz *= -0.6f;
			}
			else
			{
				vz = 0f;
			}
			childZ = 0f;
		}
		else
		{
			vz += 0.01f * Time.fixedDeltaTime * 60f;
		}
		childZ += vz * Time.fixedDeltaTime * 60f;
		child.transform.localPosition = new Vector3(0f, 0f, childZ);
	}
}
