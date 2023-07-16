using System;
using UnityEngine;

public class SoulCustomTarget : BaseMonoBehaviour
{
	public AnimationCurve ZCurve;

	private GameObject Target;

	private float Delay = 1.25f;

	private float Speed = -15f;

	private float Angle;

	private float TargetAngle;

	public SpriteRenderer Image;

	public TrailRenderer Trail;

	private float ImageZ;

	private float ImageZSpeed;

	private float TurnSpeed = 7f;

	private Vector3 ZPosition;

	private Action Callback;

	private float MaxSpeed = 20f;

	private bool init;

	private bool AddZOffset = true;

	private Vector3 targetPosition = Vector3.zero;

	private bool UseDeltaTime;

	private static GameObject projectilePrefab;

	private float DeltaTime
	{
		get
		{
			if (!UseDeltaTime)
			{
				return Time.fixedUnscaledDeltaTime;
			}
			return Time.fixedDeltaTime;
		}
	}

	public void Init(GameObject Target, Color color, Action Callback, Sprite sprite = null, float Scale = 0.4f, float MaxSpeed = 20f, bool AddZOffset = true, bool UseDeltaTime = true)
	{
		Speed = -15f;
		Delay = 1.25f;
		this.Target = Target;
		this.AddZOffset = AddZOffset;
		Angle = UnityEngine.Random.Range(0, 360);
		Image.color = color;
		Image.transform.localScale = Vector3.one * Scale;
		Trail.startWidth = Scale;
		Trail.startColor = color;
		Trail.endColor = new Color(color.r, color.g, color.b, 0f);
		Trail.Clear();
		this.Callback = Callback;
		this.MaxSpeed = MaxSpeed;
		this.UseDeltaTime = UseDeltaTime;
		init = true;
		if (sprite != null)
		{
			Image.sprite = sprite;
		}
	}

	public void Init(Vector3 targetPosition, Color color, Action Callback, Sprite sprite = null, float Scale = 0.4f, float MaxSpeed = 20f, bool AddZOffset = true, bool UseDeltaTime = true)
	{
		this.targetPosition = targetPosition;
		Init(Target, color, Callback, sprite, Scale, MaxSpeed, AddZOffset, UseDeltaTime);
	}

	public static GameObject Create(GameObject Target, Vector3 position, Color color, Action Callback, float Scale = 0.4f, float MaxSpeed = 20f, bool AddZOffset = true, bool UseDeltaTime = true)
	{
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", position);
		if (projectilePrefab == null)
		{
			projectilePrefab = Resources.Load("Prefabs/Resources/SoulCustomTarget") as GameObject;
		}
		GameObject obj = ObjectPool.Spawn(projectilePrefab, (RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		obj.transform.position = position;
		obj.transform.eulerAngles = Vector3.zero;
		obj.GetComponent<SoulCustomTarget>().Init(Target, color, Callback, null, Scale, MaxSpeed, AddZOffset, UseDeltaTime);
		return obj;
	}

	public static GameObject Create(Vector3 targetPosition, Vector3 position, Color color, Action Callback, float Scale = 0.4f, float MaxSpeed = 20f, bool AddZOffset = true, bool UseDeltaTime = true)
	{
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", position);
		if (projectilePrefab == null)
		{
			projectilePrefab = Resources.Load("Prefabs/Resources/SoulCustomTarget") as GameObject;
		}
		GameObject obj = ObjectPool.Spawn(projectilePrefab, (RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		obj.transform.position = position;
		obj.transform.eulerAngles = Vector3.zero;
		obj.GetComponent<SoulCustomTarget>().Init(targetPosition, color, Callback, null, Scale, MaxSpeed, AddZOffset, UseDeltaTime);
		return obj;
	}

	private void FixedUpdate()
	{
		if (!init || PlayerRelic.TimeFrozen)
		{
			return;
		}
		if (Target == null && targetPosition == Vector3.zero)
		{
			Trail.Clear();
			base.gameObject.Recycle();
			return;
		}
		if ((double)(10f * DeltaTime) < 0.1)
		{
			Trail.Clear();
		}
		if (Speed > 0f)
		{
			ZPosition = Image.gameObject.transform.position;
			if ((bool)Target)
			{
				ZPosition.z = Lerp(ZPosition.z, Target.transform.position.z - (AddZOffset ? 0.5f : 0f), 10f * DeltaTime);
			}
			else
			{
				ZPosition.z = Lerp(ZPosition.z, targetPosition.z - (AddZOffset ? 0.5f : 0f), 10f * DeltaTime);
			}
			Image.gameObject.transform.position = ZPosition;
		}
		Delay -= DeltaTime;
		if (Delay <= 0f)
		{
			TurnSpeed = Lerp(TurnSpeed, 1f, 10f * DeltaTime);
		}
		if ((bool)Target)
		{
			TargetAngle = Utils.GetAngle(base.transform.position, Target.transform.position);
		}
		else
		{
			TargetAngle = Utils.GetAngle(base.transform.position, targetPosition);
		}
		Angle += Mathf.Atan2(Mathf.Sin((TargetAngle - Angle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - Angle) * ((float)Math.PI / 180f))) * 57.29578f / TurnSpeed * DeltaTime * 60f;
		if (Speed < MaxSpeed)
		{
			Speed += 1f * DeltaTime * 60f;
		}
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)) * DeltaTime, Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f)) * DeltaTime);
		if ((bool)Target && Distance(base.transform.position, Target.transform.position) < Speed * DeltaTime)
		{
			CollectMe();
		}
		else if (targetPosition != Vector3.zero && Distance(base.transform.position, targetPosition) < Speed * DeltaTime)
		{
			CollectMe();
		}
	}

	private void CollectMe()
	{
		BiomeConstants.Instance.EmitHitVFXSoul(Image.gameObject.transform.position, Quaternion.identity);
		if ((bool)Target)
		{
			AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", Target);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", targetPosition);
		}
		if (Time.timeScale == 1f)
		{
			CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.2f);
		}
		if (Callback != null)
		{
			Callback();
		}
		Trail.Clear();
		base.gameObject.SetActive(false);
		base.gameObject.Recycle();
	}

	public static float Distance(Vector2 a, Vector2 b)
	{
		return (a - b).sqrMagnitude;
	}

	private float Lerp(float firstFloat, float secondFloat, float by)
	{
		return firstFloat + (secondFloat - firstFloat) * by;
	}

	private Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
	{
		float x = Lerp(firstVector.x, secondVector.x, by);
		float y = Lerp(firstVector.y, secondVector.y, by);
		return new Vector2(x, y);
	}
}
