using System;
using System.Collections.Generic;
using UnityEngine;

public class BlackSoul : BaseMonoBehaviour
{
	[Serializable]
	public struct BiomeColour
	{
		public Color Color;

		public FollowerLocation Location;
	}

	public static List<BlackSoul> BlackSouls = new List<BlackSoul>();

	public float SeperationRadius = 0.5f;

	private GameObject Target;

	public float Delay = 0.5f;

	public float Speed;

	private float Angle;

	private float TargetAngle;

	public GameObject BounceChild;

	public GameObject Image;

	public GameObject Trail;

	private float ImageZ;

	private float ImageZSpeed;

	private float TurnSpeed = 5f;

	private float Distance;

	private float ChaseTimer;

	private float LifeTime;

	public float LifeTimeDuration = 7f;

	private float Bobbing;

	public float BobbingArc = 0.1f;

	public float BobbingSpeed = 1f;

	public bool Moving;

	private float TargetHeight;

	private float MaxHeight;

	private float MinHeight;

	private float gravity = -9.8f;

	public List<BiomeColour> Colours;

	private float z;

	public float magnetiseDistance = 1.5f;

	public int Delta = 1;

	public bool Completed;

	public bool Simulated;

	private BlackSoulUpdater BlacksoulsupdaterInstance;

	private Vector3 seperator;

	public bool GiveXP { get; set; }

	private bool CanPickUp
	{
		get
		{
			if (!(FaithAmmo.Ammo < FaithAmmo.Total))
			{
				if (PlayerFarming.Instance != null && PlayerFarming.Instance.playerWeapon.CurrentWeapon != null && PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData != null)
				{
					return PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.ContainsAttachmentType(AttachmentEffect.Fervour);
				}
				return false;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			LifeTimeDuration *= 2f;
		}
		BlacksoulsupdaterInstance = BlackSoulUpdater.Instance;
		base.gameObject.SetActive(true);
		if (Target == null && PlayerFarming.Instance != null)
		{
			Target = PlayerFarming.Instance.gameObject;
		}
		Bobbing = UnityEngine.Random.Range(0, 360);
		MaxHeight = UnityEngine.Random.Range(0.5f, 0.9f);
		MinHeight = UnityEngine.Random.Range(0.1f, 0.3f);
		TargetHeight = MinHeight;
		Completed = false;
		Delay = 0.5f;
		Moving = false;
		TurnSpeed = 5f;
		ImageZ = 0f;
		ImageZSpeed = 0f;
		LifeTime = 0f;
		Image.SetActive(true);
		Color color = new Color(UnityEngine.Random.Range(-0.25f, 0.25f), Colours[0].Color.g, Colours[0].Color.b, Colours[0].Color.a);
		Image.GetComponent<SpriteRenderer>().color = Colours[0].Color + color;
		Trail.GetComponent<TrailRenderer>().startColor = Colours[0].Color + color;
		foreach (BiomeColour colour in Colours)
		{
			if (colour.Location == PlayerFarming.Location)
			{
				Image.GetComponent<SpriteRenderer>().material.SetColor("_Tint", colour.Color + color);
				Trail.GetComponent<TrailRenderer>().startColor = colour.Color + color;
				break;
			}
		}
		if (EnemyDeathCatBoss.Instance != null)
		{
			Image.GetComponent<SpriteRenderer>().material.SetColor("_Tint", Color.black + color);
			Trail.GetComponent<TrailRenderer>().startColor = Color.black;
		}
		BlackSouls.Add(this);
		magnetiseDistance = 1.5f;
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Combine(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(OnChestRevealed));
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			if ((bool)demon.GetComponent<Demon_Arrows>())
			{
				magnetiseDistance = 100f;
			}
		}
	}

	private void OnDisable()
	{
		BlackSouls.Remove(this);
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(OnChestRevealed));
	}

	private void OnChestRevealed()
	{
		magnetiseDistance = 100f;
	}

	public void SetAngle(float Angle)
	{
		this.Angle = Angle;
		Speed = UnityEngine.Random.Range(0.5f, 1.5f);
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f))) * Time.fixedDeltaTime;
	}

	public void SetAngle(float Angle, Vector2 RandomSpeedRange)
	{
		this.Angle = Angle;
		Speed = UnityEngine.Random.Range(RandomSpeedRange.x, RandomSpeedRange.y);
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f))) * Time.fixedDeltaTime;
	}

	public void Seperate(float SeperationRadius)
	{
		seperator = Vector3.zero;
		float num = Time.fixedDeltaTime * 60f;
		foreach (BlackSoul blackSoul in BlackSouls)
		{
			if (blackSoul != this && blackSoul != null && !blackSoul.Moving)
			{
				float num2 = MagnitudeFindDistanceBetween(blackSoul.gameObject.transform.position, base.transform.position);
				if (num2 < SeperationRadius)
				{
					float f = Utils.GetAngleR(blackSoul.gameObject.transform.position, base.transform.position) * ((float)Math.PI / 180f);
					float num3 = (SeperationRadius - num2) / 2f;
					seperator.x += num3 * Mathf.Cos(f) * num;
					seperator.y += num3 * Mathf.Sin(f) * num;
				}
			}
		}
		base.transform.position += seperator;
	}

	private void FixedUpdate()
	{
		FixedUpdateMethod();
	}

	private void FixedUpdateMethod()
	{
		if (Target == null && (bool)PlayerFarming.Instance)
		{
			Target = PlayerFarming.Instance.transform.gameObject;
		}
		if (Target == null)
		{
			DisableMe();
			return;
		}
		Trail.SetActive(Moving);
		if (Time.fixedDeltaTime > 0f && (!Simulated || base.transform.position.z >= 0f))
		{
			ImageZSpeed += (0f - TargetHeight - ImageZ) * 0.3f / (Time.fixedDeltaTime * 60f);
			ImageZ += (ImageZSpeed *= 0.7f * (Time.fixedDeltaTime * 60f)) * (Time.fixedDeltaTime * 60f);
			BounceChild.transform.localPosition = Vector3.zero + Vector3.forward * ImageZ + Vector3.forward * BobbingArc * Mathf.Cos(Bobbing += BobbingSpeed * Time.fixedDeltaTime);
		}
		Vector3 vector = base.transform.position + new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f))) * Time.fixedDeltaTime;
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z))
		{
			base.transform.position = new Vector3(vector.x, vector.y, (Simulated && base.transform.position.z < 0f) ? (base.transform.position.z + z * Time.deltaTime) : 0f);
		}
		z -= gravity * Time.deltaTime;
		if ((Delay -= Time.fixedDeltaTime) > 0f)
		{
			return;
		}
		Distance = MagnitudeFindDistanceBetween(base.transform.position, Target.transform.position);
		if (Moving && !CanPickUp)
		{
			Speed = 0f;
			Moving = false;
		}
		if (Moving)
		{
			if (Target == null)
			{
				DisableMe();
				return;
			}
			if ((ChaseTimer += Time.fixedDeltaTime) > 1f)
			{
				TurnSpeed = Mathf.Lerp(TurnSpeed, 1f, 10f * Time.fixedDeltaTime);
			}
			TargetAngle = Utils.GetAngle(base.transform.position, Target.transform.position);
			Angle = Utils.SmoothAngle(Angle, TargetAngle, TurnSpeed);
			if (Speed < 25f)
			{
				Speed += 1f * (Time.fixedDeltaTime * 60f);
			}
			if (Distance < Speed * Time.fixedDeltaTime)
			{
				CollectMe();
			}
			return;
		}
		if (Speed > 0f)
		{
			Speed -= 0.5f * Time.fixedDeltaTime * 60f;
			if (Speed < 0f)
			{
				Speed = 0f;
			}
		}
		if (!LetterBox.IsPlaying && (LifeTime += Time.fixedDeltaTime) > LifeTimeDuration - 2f)
		{
			if (Time.frameCount % 5 == 0)
			{
				Image.SetActive(!Image.activeSelf);
			}
			if (LifeTime > LifeTimeDuration)
			{
				DisableMe();
			}
		}
		if (Distance < magnetiseDistance + 0.5f && CanPickUp)
		{
			TargetHeight = MaxHeight;
		}
		else
		{
			TargetHeight = MinHeight;
		}
		if (Distance < magnetiseDistance && CanPickUp)
		{
			TargetHeight = MaxHeight;
			Moving = true;
			Speed = -10f;
			Image.SetActive(true);
		}
	}

	private void DisableMe()
	{
		Completed = true;
		ObjectPool.Recycle(base.gameObject);
	}

	private void CollectMe()
	{
		if (CanPickUp)
		{
			PlayerFarming.Instance.GetBlackSoul(Delta, GiveXP);
			BiomeConstants.Instance.EmitPickUpVFX(BounceChild.transform.position, "StarBurst_4");
			CameraManager.shakeCamera(0.2f, Angle);
			DisableMe();
		}
	}

	public static void Clear()
	{
		for (int num = BlackSouls.Count - 1; num >= 0; num--)
		{
			BlackSouls[num].DisableMe();
		}
		BlackSouls.Clear();
	}

	private float MagnitudeFindDistanceBetween(Vector2 a, Vector2 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return num * num + num2 * num2;
	}
}
