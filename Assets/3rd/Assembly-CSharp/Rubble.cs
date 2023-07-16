using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Rubble : BaseMonoBehaviour
{
	public static List<Rubble> Rubbles = new List<Rubble>();

	public List<Transform> ShakeTransforms;

	private Vector2[] Shake = new Vector2[0];

	public Structure Structure;

	private Structures_Rubble _StructureInfo;

	[FormerlySerializedAs("ProgressIndicator")]
	public UIProgressIndicator _uiProgressIndicator;

	public RandomObjectPicker objectPick;

	[SerializeField]
	private ParticleSystem _particleSystem;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Rubble StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Rubble;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void OnEnable()
	{
		Rubbles.Add(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnDisable()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			Structures_Rubble structureBrain = StructureBrain;
			structureBrain.OnRemovalProgressChanged = (Action<int>)Delegate.Remove(structureBrain.OnRemovalProgressChanged, new Action<int>(OnRemovalProgressChanged));
		}
		Rubbles.Remove(this);
	}

	private void Start()
	{
		RandomObjectPicker randomObjectPicker = objectPick;
		randomObjectPicker.ObjectCreated = (UnityAction)Delegate.Combine(randomObjectPicker.ObjectCreated, new UnityAction(ObjectCreated));
	}

	private void ObjectCreated()
	{
		Transform[] componentsInChildren = objectPick.CreatedObject.GetComponentsInChildren<Transform>();
		foreach (Transform item in componentsInChildren)
		{
			ShakeTransforms.Add(item);
		}
		Shake = new Vector2[ShakeTransforms.Count];
		for (int j = 0; j < ShakeTransforms.Count; j++)
		{
			Shake[j] = ShakeTransforms[j].transform.localPosition;
		}
	}

	private void OnBrainAssigned()
	{
		CircleCollider2D componentInChildren = GetComponentInChildren<CircleCollider2D>();
		if (componentInChildren != null)
		{
			AstarPath.active.UpdateGraphs(componentInChildren.bounds);
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_Rubble structureBrain = StructureBrain;
		structureBrain.OnRemovalProgressChanged = (Action<int>)Delegate.Combine(structureBrain.OnRemovalProgressChanged, new Action<int>(OnRemovalProgressChanged));
	}

	private void OnDestroy()
	{
		if (StructureBrain == null || StructureInfo == null)
		{
			return;
		}
		if (StructureInfo.Destroyed && !StructureBrain.ForceRemoved)
		{
			float num = 1f;
			if (StructureInfo.FollowerID != -1)
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(StructureInfo.FollowerID);
				if (infoByID != null)
				{
					FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
					if (orCreateBrain != null)
					{
						num = orCreateBrain.ResourceHarvestingMultiplier;
					}
				}
			}
			AudioManager.Instance.PlayOneShot(SoundConstants.GetBreakSoundPathForMaterial(SoundConstants.SoundMaterial.Stone), base.transform.position);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
			InventoryItem.Spawn(StructureBrain.Data.LootToDrop, Mathf.RoundToInt((float)StructureBrain.RubbleDropAmount * num), base.transform.position);
		}
		if (_uiProgressIndicator != null)
		{
			_uiProgressIndicator.Recycle();
			_uiProgressIndicator = null;
		}
		Structures_Rubble structureBrain = StructureBrain;
		structureBrain.OnRemovalProgressChanged = (Action<int>)Delegate.Remove(structureBrain.OnRemovalProgressChanged, new Action<int>(OnRemovalProgressChanged));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	public void ShakeRubble()
	{
		float num = ((Structure.Type != global::StructureBrain.TYPES.RUBBLE_BIG) ? 0.5f : 1f);
		if (ShakeTransforms.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < ShakeTransforms.Count; i++)
		{
			if (ShakeTransforms[i] != null && ShakeTransforms[i].gameObject.activeSelf)
			{
				ShakeTransforms[i].DOKill();
				ShakeTransforms[i].transform.localPosition = Shake[i];
				ShakeTransforms[i].DOShakePosition(0.5f * num, new Vector2(UnityEngine.Random.Range(-0.25f, 0.25f) * num, 0f));
			}
		}
	}

	public void UpdateBar(int followerID)
	{
		float num = StructureBrain.RemovalProgress / StructureBrain.RemovalDurationInGameMinutes;
		if (num == 0f)
		{
			return;
		}
		if (_uiProgressIndicator == null)
		{
			_uiProgressIndicator = BiomeConstants.Instance.ProgressIndicatorTemplate.Spawn(BiomeConstants.Instance.transform, base.transform.position + Vector3.back * 1.5f - BiomeConstants.Instance.transform.position);
			_uiProgressIndicator.Show(num);
			UIProgressIndicator uiProgressIndicator = _uiProgressIndicator;
			uiProgressIndicator.OnHidden = (Action)Delegate.Combine(uiProgressIndicator.OnHidden, (Action)delegate
			{
				_uiProgressIndicator = null;
			});
		}
		else
		{
			_uiProgressIndicator.SetProgress(num);
		}
	}

	public void PlayerRubbleFX()
	{
		CameraManager.shakeCamera(0.3f);
		ShakeRubble();
		Vector3 position = base.transform.position;
		AudioManager.Instance.PlayOneShot(SoundConstants.GetImpactSoundPathForMaterial(SoundConstants.SoundMaterial.Stone), position);
		_particleSystem.Play();
		MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, GameManager.GetInstance());
	}

	private void RubbleFX()
	{
		ShakeRubble();
		Vector3 position = base.transform.position;
		AudioManager.Instance.PlayOneShot(SoundConstants.GetImpactSoundPathForMaterial(SoundConstants.SoundMaterial.Stone), position);
		_particleSystem.Play();
	}

	private void OnRemovalProgressChanged(int followerID)
	{
		RubbleFX();
		UpdateBar(followerID);
	}
}
