using System;
using System.Collections.Generic;
using UnityEngine;

public class SoulCustomTargetLerp : BaseMonoBehaviour
{
	private Vector3 StartingPosition;

	private GameObject Target;

	private Action Callback;

	public SpriteRenderer Image;

	public TrailRenderer Trail;

	public AnimationCurve MovementCurve;

	private Vector3 MovementVector;

	public AnimationCurve HeightCurve;

	private float Progress;

	public float Duration = 0.5f;

	public static GameObject hitFX;

	public static GameObject SoulCustom;

	public static List<GameObject> pool;

	private static GameObject SoulCustomTargetLerpPool;

	public Vector3 Offset { get; set; } = Vector3.zero;


	public static GameObject Create(GameObject Target, Vector3 position, float Duration, Color color, Action Callback = null)
	{
		if (hitFX == null)
		{
			hitFX = Resources.Load("FX/HitFX/HitFX_Soul") as GameObject;
		}
		if (SoulCustom == null)
		{
			SoulCustom = Resources.Load("Prefabs/Resources/SoulCustomTarget Lerp") as GameObject;
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", position);
		GameObject objectObjectFromPool = GetObjectObjectFromPool(Target.transform.parent);
		objectObjectFromPool.transform.position = position;
		objectObjectFromPool.transform.eulerAngles = Vector3.zero;
		SoulCustomTargetLerp component = objectObjectFromPool.GetComponent<SoulCustomTargetLerp>();
		component.Trail.Clear();
		component.Init(Target, position, color, Duration, Callback);
		objectObjectFromPool.SetActive(true);
		return objectObjectFromPool;
	}

	public static GameObject GetObjectObjectFromPool(Transform TargetParent)
	{
		if (pool == null)
		{
			pool = new List<GameObject>();
			SoulCustomTargetLerpPool = new GameObject("SoulCustomTargetLerpPool");
			UnityEngine.Object.Instantiate(SoulCustomTargetLerpPool, Vector3.zero, Quaternion.identity);
		}
		if (pool.Count > 0)
		{
			for (int i = 0; i < pool.Count; i++)
			{
				if (!pool[i].activeInHierarchy)
				{
					Debug.Log("reused SoulCustomTargetLerp");
					pool[i].transform.parent = TargetParent;
					return pool[i];
				}
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/SoulCustomTarget Lerp") as GameObject, TargetParent, true);
		pool.Add(gameObject);
		return gameObject;
	}

	public void Init(GameObject Target, Vector3 position, Color color, float Duration, Action Callback)
	{
		Progress = 0f;
		this.Target = Target;
		StartingPosition = position;
		this.Duration = Duration;
		Image.color = color;
		Image.transform.localScale = Vector3.one * 0.4f;
		Trail.startColor = color;
		Trail.endColor = new Color(color.r, color.g, color.b, 0f);
		Trail.Clear();
		this.Callback = Callback;
	}

	private void Update()
	{
		MovementVector = Vector3.LerpUnclamped(StartingPosition, Target.transform.position + Offset, MovementCurve.Evaluate((Progress += Time.deltaTime) / Duration));
		HeightCurve.keys[HeightCurve.keys.Length - 1].value = Target.transform.position.z + Offset.z;
		MovementVector.z = 0f - HeightCurve.Evaluate(Progress / Duration);
		base.transform.position = MovementVector;
		if (Progress / Duration >= 1f)
		{
			CollectMe();
		}
	}

	private void CollectMe()
	{
		BiomeConstants.Instance.EmitHitVFXSoul(Image.gameObject.transform.position, Quaternion.identity);
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", Target);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.2f);
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
		Trail.Clear();
		ReturnToPool();
	}

	private void ReturnToPool()
	{
		Callback = null;
		base.gameObject.transform.parent = SoulCustomTargetLerpPool.transform;
		base.gameObject.SetActive(false);
		Trail.Clear();
	}

	private void OnDestroy()
	{
		pool = null;
	}
}
