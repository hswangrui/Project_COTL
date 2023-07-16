using System;
using System.Collections;
using System.Collections.Generic;
using Lamb.UI;
using UnityEngine;

public class BuildSitePlot : BaseMonoBehaviour
{
	public static List<BuildSitePlot> BuildSitePlots = new List<BuildSitePlot>();

	public GameObject SmokePrefab;

	private GameObject SmokeObject;

	public Interaction Interaction;

	public GameObject PlacementSquare;

	public Structure Structure;

	private Structures_BuildSite _StructureInfo;

	public Vector2Int Bounds = new Vector2Int(1, 1);

	public Transform RotatedObject;

	public List<Worshipper> Worshippers = new List<Worshipper>();

	public UIProgressIndicator _uiProgressIndicator;

	public Vector3 CentrePosition;

	private Coroutine cBuildSmokeRoutine;

	private ParticleSystem[] particleSystems;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public virtual Structures_BuildSite StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_BuildSite;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public static bool StructureOfTypeUnderConstruction(StructureBrain.TYPES Type)
	{
		foreach (BuildSitePlot buildSitePlot in BuildSitePlots)
		{
			if (buildSitePlot.StructureBrain.Data.ToBuildType == Type)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		BuildSitePlots.Add(this);
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnDisable()
	{
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		if (StructureBrain != null)
		{
			Structures_BuildSite structureBrain = StructureBrain;
			structureBrain.OnBuildProgressChanged = (Action)Delegate.Remove(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
		}
		BuildSitePlots.Remove(this);
	}

	private void Start()
	{
		SpawnTiles();
	}

	private void OnBrainAssigned()
	{
		Structures_BuildSite structureBrain = StructureBrain;
		structureBrain.OnBuildProgressChanged = (Action)Delegate.Combine(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
		StartCoroutine(CheckTutorialRoutine());
		Bounds = StructureBrain.Data.Bounds;
	}

	private IEnumerator CheckTutorialRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		switch (StructureInfo.ToBuildType)
		{
		case global::StructureBrain.TYPES.SURVEILLANCE:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Surveillance))
			{
				MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Surveillance);
			}
			break;
		case global::StructureBrain.TYPES.REFINERY:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.RefiningResources))
			{
				MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.RefiningResources);
			}
			break;
		}
	}

	private void OnDestroy()
	{
		if (_uiProgressIndicator != null)
		{
			_uiProgressIndicator.Recycle();
			_uiProgressIndicator = null;
		}
		if (StructureBrain != null)
		{
			Structures_BuildSite structureBrain = StructureBrain;
			structureBrain.OnBuildProgressChanged = (Action)Delegate.Remove(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
		}
	}

	public void UpdateBar()
	{
		if (LetterBox.IsPlaying || StructureBrain == null || StructureBrain.Data == null)
		{
			return;
		}
		float progress = StructureBrain.BuildProgress / (float)StructuresData.BuildDurationGameMinutes(StructureBrain.Data.ToBuildType);
		if (_uiProgressIndicator == null)
		{
			_uiProgressIndicator = BiomeConstants.Instance.ProgressIndicatorTemplate.Spawn(BiomeConstants.Instance.transform, base.transform.position + Vector3.back * 1.5f - BiomeConstants.Instance.transform.position);
			_uiProgressIndicator.Show(progress);
			UIProgressIndicator uiProgressIndicator = _uiProgressIndicator;
			uiProgressIndicator.OnHidden = (Action)Delegate.Combine(uiProgressIndicator.OnHidden, (Action)delegate
			{
				_uiProgressIndicator = null;
			});
		}
		else
		{
			_uiProgressIndicator.SetProgress(progress);
		}
	}

	private void OnBuildProgressChanged()
	{
		UpdateBar();
		if (cBuildSmokeRoutine != null)
		{
			StopCoroutine(cBuildSmokeRoutine);
		}
		if (base.gameObject != null && base.gameObject.activeInHierarchy)
		{
			cBuildSmokeRoutine = StartCoroutine(BuildSmokeRoutine());
		}
	}

	private IEnumerator BuildSmokeRoutine()
	{
		ParticleSystem[] array;
		if (SmokeObject == null)
		{
			SmokeObject = UnityEngine.Object.Instantiate(SmokePrefab, Vector3.zero, Quaternion.Euler(-180f, 0f, 0f), base.transform);
			SmokeObject.transform.localScale = Vector3.one * Mathf.Max(Bounds.x, Bounds.y);
			SmokeObject.transform.localPosition = CentrePosition;
			particleSystems = SmokeObject.GetComponentsInChildren<ParticleSystem>();
			array = particleSystems;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem.EmissionModule emission = array[i].emission;
				emission.rateOverTimeMultiplier *= Mathf.Max(Bounds.x, Bounds.y);
			}
		}
		else
		{
			array = particleSystems;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}
		yield return new WaitForSeconds(0.5f);
		array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}

	private void SpawnTiles()
	{
		int num = -1;
		while (++num < Bounds.x)
		{
			int num2 = -1;
			while (++num2 < Bounds.y)
			{
				UnityEngine.Object.Instantiate(PlacementSquare, RotatedObject.transform, false).transform.localPosition = new Vector3(num, num2);
			}
		}
		CentrePosition = new Vector3(0f, (float)Bounds.y / 2f);
		Interaction.ActivatorOffset = CentrePosition;
		Interaction.ActivateDistance = Bounds.x;
	}

	private void OnDrawGizmos()
	{
		if (StructureInfo != null)
		{
			Debug.Log((float)StructureInfo.Bounds.y / 2f);
			Utils.DrawCircleXY(StructureInfo.Position + new Vector3(0f, (float)StructureInfo.Bounds.y / 2f), (float)StructureInfo.Bounds.x * 0.5f, Color.green);
			Utils.DrawCircleXY(StructureInfo.Position + new Vector3(0f, (float)StructureInfo.Bounds.y / 2f), 0.5f, Color.blue);
			Utils.DrawCircleXY(StructureInfo.Position, 0.5f, Color.yellow);
			Utils.DrawCircleXY(base.transform.position, 0.4f, Color.red);
		}
	}
}
