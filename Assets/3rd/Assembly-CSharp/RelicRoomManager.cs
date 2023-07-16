using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using MMRoomGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RelicRoomManager : MonoBehaviour
{
	public static RelicRoomManager Instance;

	[SerializeField]
	private List<SpriteRenderer> stainedGlasses = new List<SpriteRenderer>();

	[SerializeField]
	private List<MeshRenderer> decalStainedMesh = new List<MeshRenderer>();

	[SerializeField]
	private BiomeLightingSettings relicRoomLighting;

	[SerializeField]
	private Interaction_SimpleConversation equippedRelicConversation;

	[SerializeField]
	private Interaction_SimpleConversation completedRelicConversation;

	[SerializeField]
	private AssetReferenceGameObject enemy;

	[SerializeField]
	private SkeletonAnimation spine;

	[SerializeField]
	private Vector3 middle;

	[SerializeField]
	private SimpleBark angryBark;

	[SerializeField]
	private SimpleBark defaultBark;

	[SerializeField]
	private GameObject npc;

	[SerializeField]
	private CanvasGroup controlsHUD;

	[SerializeField]
	private Interaction_WeaponSelectionPodium[] podiums;

	[SerializeField]
	private Health[] effigies;

	private List<Material> decalStainedMaterial = new List<Material>();

	private float _randomAlpha0 = 0.5f;

	private float _randomAlpha1 = 0.5f;

	[CompilerGenerated]
	private readonly int _003CRelicTargetCount_003Ek__BackingField = 1;

	private ObjectivesData objective;

	private float previousChargeAmount;

	private bool barksEnabled = true;

	private float time;

	private static readonly int Color1 = Shader.PropertyToID("_Color");

	public Interaction_SimpleConversation EquippedRelicConversation
	{
		get
		{
			return equippedRelicConversation;
		}
	}

	public int RelicUsedCount { get; set; }

	public int RelicTargetCount
	{
		[CompilerGenerated]
		get
		{
			return _003CRelicTargetCount_003Ek__BackingField;
		}
	}

	private void Awake()
	{
		Instance = this;
		controlsHUD.alpha = 0f;
		foreach (MeshRenderer item in decalStainedMesh)
		{
			Material sharedMaterial = new Material(item.material);
			item.sharedMaterial = sharedMaterial;
			decalStainedMaterial.Add(item.sharedMaterial);
		}
		_randomAlpha0 = Random.Range(0.5f, 1f);
		_randomAlpha1 = Random.Range(0.5f, 1f);
		if (DataManager.Instance.OnboardedRelics)
		{
			defaultBark.gameObject.SetActive(true);
			defaultBark.OnPlay += DefaultBark_OnPlay;
			MakeEffigiesEnemies(true);
			Interaction_WeaponSelectionPodium[] array = podiums;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnInteraction += Pod_OnInteraction;
			}
		}
		else
		{
			GetComponentInParent<GenerateRoom>().LockingDoors = true;
			barksEnabled = false;
		}
	}

	private void OnDestroy()
	{
		Instance = null;
		PlayerRelic.OnRelicUsed -= PlayerRelic_OnRelicUsed;
		PlayerRelic.OnRelicChargeModified -= PlayerRelic_OnRelicChargeModified;
		Interaction_WeaponSelectionPodium[] array = podiums;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnInteraction -= Pod_OnInteraction;
		}
		defaultBark.OnPlay -= DefaultBark_OnPlay;
	}

	private void DefaultBark_OnPlay()
	{
		defaultBark.ActivateDistance = 100f;
	}

	private void Pod_OnInteraction(StateMachine state)
	{
		defaultBark.gameObject.SetActive(true);
		barksEnabled = false;
		GameManager.GetInstance().WaitForSeconds(6f, delegate
		{
			Interaction_WeaponSelectionPodium[] array = podiums;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnInteraction -= Pod_OnInteraction;
			}
			if (base.gameObject != null)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/chemach_leaves_whoosh", base.gameObject);
			}
			if (spine != null)
			{
				spine.AnimationState.SetAnimation(0, "exit", false);
			}
		});
	}

	private void Update()
	{
		Color color = Color.HSVToRGB(TimeManager.TimeRemainingUntilPhase(DayPhase.Night) / 1200f, 1f, 1f);
		Color color2 = color;
		Color color3 = new Color(0.2f, 0.2f, 0.2f);
		color2.a = Mathf.Lerp(0.1f, 0.5f, TimeManager.TimeRemainingUntilPhase(DayPhase.Night) / 1200f);
		foreach (Material item in decalStainedMaterial)
		{
			item.SetColor(Color1, color2 + color3);
			item.color = new Color(item.color.r, item.color.g, item.color.g, 0.75f);
		}
		relicRoomLighting.AmbientColour = new Color(Mathf.Clamp01(color.g - 0.5f), Mathf.Clamp01(color.b - 0.5f), Mathf.Clamp01(color.r - 0.5f));
		color.r = Mathf.Clamp01(color.r + 0.8f);
		color.g = Mathf.Clamp01(color.g + 0.8f);
		color.b = Mathf.Clamp01(color.b + 0.8f);
		foreach (SpriteRenderer stainedGlass in stainedGlasses)
		{
			if (stainedGlass != null)
			{
				stainedGlass.color = color;
			}
		}
	}

	public void AttackedEffigy()
	{
		if (barksEnabled)
		{
			defaultBark.gameObject.SetActive(false);
			if (!angryBark.gameObject.activeSelf)
			{
				angryBark.gameObject.SetActive(true);
			}
			else if (MMConversation.CURRENT_CONVERSATION == null)
			{
				angryBark.Show();
			}
		}
	}

	public void SpawnEnemyAndRechargeRelic()
	{
		DataManager.Instance.SpawnedRelicsThisRun.Add(RelicType.LightningStrike);
		podiums[0].transform.DOMoveZ(1f, 0.5f);
		ObjectiveManager.Add(objective = new Objectives_Custom("Objectives/GroupTitles/GiveRelic", Objectives.CustomQuestTypes.ChargeRelic), true);
		PlayerFarming.Instance.health.untouchable = true;
		StartCoroutine(SpawnEnemyIE());
		PlayerFarming.Instance.playerRelic.CurrentRelic.DamageRequiredToCharge /= 5f;
		PlayerRelic.OnRelicUsed += PlayerRelic_OnRelicUsed;
		PlayerRelic.OnRelicChargeModified += PlayerRelic_OnRelicChargeModified;
	}

	private void PlayerRelic_OnRelicChargeModified(RelicData relic)
	{
		if (previousChargeAmount != PlayerFarming.Instance.playerRelic.ChargedAmount && PlayerFarming.Instance.playerRelic.IsFullyCharged)
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.ChargeRelic);
			ObjectiveManager.Add(objective = new Objectives_UseRelic("Objectives/GroupTitles/GiveRelic"));
			ShowControlsIE();
			PlayerRelic.OnRelicChargeModified -= PlayerRelic_OnRelicChargeModified;
		}
		previousChargeAmount = PlayerFarming.Instance.playerRelic.ChargedAmount;
	}

	private void PlayerRelic_OnRelicUsed(RelicData relic)
	{
		RelicUsedCount++;
		ObjectiveManager.UpdateObjective(objective);
		PlayerRelic.OnRelicUsed -= PlayerRelic_OnRelicUsed;
		if (RelicUsedCount >= RelicTargetCount)
		{
			StopAllCoroutines();
			StartCoroutine(CompletedIE());
		}
	}

	private void ShowControlsIE()
	{
		controlsHUD.transform.localScale = Vector3.one;
		controlsHUD.alpha = 1f;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(controlsHUD.transform.DOPunchScale(Vector3.one * 0.3f, 0.25f));
		sequence.AppendInterval(0.5f);
		sequence.Append(controlsHUD.transform.DOLocalMoveY(-440f, 0.5f).SetEase(Ease.InBack)).OnComplete(delegate
		{
			sequence = DOTween.Sequence();
			sequence.AppendInterval(2f);
			sequence.Append(controlsHUD.transform.DOPunchScale(Vector3.one * 0.3f, 0.25f));
			sequence.SetLoops(-1);
		});
	}

	private IEnumerator SpawnEnemyIE()
	{
		int maxEnemies = 3;
		while (RelicUsedCount < RelicTargetCount)
		{
			while (Health.team2.Count >= maxEnemies)
			{
				yield return null;
			}
			yield return new WaitForSeconds(1f);
			for (int i = 0; i < maxEnemies - Health.team2.Count; i++)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(enemy);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
				{
					EnemySpawner.Create(middle + (Vector3)(Random.insideUnitCircle * 2.5f), base.transform.parent, obj.Result.gameObject);
				};
				yield return new WaitForSeconds(0.5f);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator CompletedIE()
	{
		PlayerFarming.Instance.playerRelic.CurrentRelic.DamageRequiredToCharge *= 5f;
		controlsHUD.alpha = 0f;
		yield return new WaitForSeconds(1.75f);
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null)
			{
				Health.team2[num].DealDamage(Health.team2[num].HP, base.gameObject, base.transform.position);
			}
		}
		yield return new WaitForSeconds(0.25f);
		PlayerFarming.Instance.health.untouchable = false;
		PlayerReturnToBase.Disabled = false;
		completedRelicConversation.Play();
		MakeEffigiesEnemies(true);
	}

	public void MakeEffigiesEnemies(bool enable)
	{
		Health[] array = effigies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].team = (enable ? Health.Team.Team2 : Health.Team.Neutral);
		}
	}
}
