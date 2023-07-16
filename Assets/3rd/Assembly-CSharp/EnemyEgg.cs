using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyEgg : UnitObject
{
	public List<SkeletonAnimation> Spine = new List<SkeletonAnimation>();

	public SimpleSpineFlash SimpleSpineFlash;

	public ParticleSystem hatchParticles;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	[EventRef]
	public string onWarningCrackPath = string.Empty;

	[EventRef]
	public string onHatchPath = string.Empty;

	public AssetReferenceGameObject EnemyPrefab;

	public int numHatchlings;

	public float hatchlingDistanceFromEgg;

	public float hatchTime = 5f;

	public int numWarningCracks = 5;

	private int warningCrackCounter;

	private GameManager gm;

	private float laidTimestamp;

	private bool isBroken;

	public static List<EnemyEgg> EnemyEggs = new List<EnemyEgg>();

	public override void OnEnable()
	{
		base.OnEnable();
		state.CURRENT_STATE = StateMachine.State.Idle;
		warningCrackCounter = 0;
		foreach (SkeletonAnimation item in Spine)
		{
			if (item.AnimationState != null)
			{
				item.AnimationState.SetAnimation(0, "get-laid", false);
				item.transform.DOComplete();
			}
		}
		health.OnDieCallback.AddListener(BreakAnEgg);
	}

	public override void OnDisable()
	{
		SimpleSpineFlash.FlashWhite(false);
		health.OnDieCallback.RemoveListener(BreakAnEgg);
		base.OnDisable();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		foreach (SkeletonAnimation item in Spine)
		{
			if (item.AnimationState != null)
			{
				item.AnimationState.SetAnimation(0, "squash", false);
				item.AnimationState.AddAnimation(0, "idle", false, 0f);
			}
		}
		SimpleSpineFlash.FlashFillRed();
	}

	public override void Update()
	{
		if (isBroken)
		{
			return;
		}
		base.Update();
		if (gm == null)
		{
			gm = GameManager.GetInstance();
			if (!(gm != null))
			{
				return;
			}
			laidTimestamp = gm.CurrentTime;
			laidTimestamp += Random.Range(0f, 0.5f);
		}
		if (gm.TimeSince(laidTimestamp) >= hatchTime)
		{
			Hatch();
		}
		else if ((float)warningCrackCounter < gm.TimeSince(laidTimestamp) / hatchTime * (float)numWarningCracks)
		{
			WarningCrack();
			warningCrackCounter++;
		}
	}

	private void WarningCrack()
	{
		foreach (SkeletonAnimation item in Spine)
		{
			if (item.AnimationState != null)
			{
				item.AnimationState.SetAnimation(0, "squash", false);
				item.AnimationState.AddAnimation(0, "idle", false, 0f);
				switch (warningCrackCounter)
				{
				case 0:
					item.skeleton.SetSkin("egg_0");
					break;
				case 1:
					item.skeleton.SetSkin("egg_1");
					break;
				case 2:
					item.skeleton.SetSkin("egg_2");
					break;
				case 3:
					item.skeleton.SetSkin("egg_3");
					break;
				}
			}
		}
		if (!string.IsNullOrEmpty(onWarningCrackPath))
		{
			AudioManager.Instance.PlayOneShot(onWarningCrackPath, base.transform.position);
		}
	}

	private void Hatch()
	{
		BreakAnEgg();
		for (int i = 0; i < numHatchlings; i++)
		{
			Vector3 vector = Random.insideUnitCircle.normalized * hatchlingDistanceFromEgg;
			if ((bool)Physics2D.Raycast(base.transform.position, vector.normalized, hatchlingDistanceFromEgg, layerToCheck))
			{
				continue;
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(EnemyPrefab, base.transform.position + vector, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				GameObject obj2 = obj.Result.gameObject;
				obj2.transform.DOComplete();
				obj2.transform.localScale = Vector3.zero;
				obj2.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
				UnitObject component = obj2.GetComponent<UnitObject>();
				if (component != null)
				{
					component.RemoveModifier();
				}
			};
		}
	}

	private void BreakAnEgg()
	{
		isBroken = true;
		if (hatchParticles != null)
		{
			hatchParticles.Play();
		}
		foreach (SkeletonAnimation item in Spine)
		{
			if (item.AnimationState != null)
			{
				item.AnimationState.SetAnimation(0, "squash", false);
				item.AnimationState.AddAnimation(0, "idle", false, 0f);
				item.skeleton.SetSkin("egg_4");
			}
		}
		if (!string.IsNullOrEmpty(onHatchPath))
		{
			AudioManager.Instance.PlayOneShot(onHatchPath, base.transform.position);
		}
		ShowHPBar component = GetComponent<ShowHPBar>();
		if (component != null)
		{
			component.DestroyHPBar();
		}
		health.ClearElectrified();
		health.ClearPoison();
		health.ClearIce();
		health.ClearCharm();
		health.enabled = false;
		base.enabled = false;
		Collider2D component2 = GetComponent<Collider2D>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
	}
}
