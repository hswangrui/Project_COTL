using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSitePlotProject : BaseMonoBehaviour
{
	public static List<BuildSitePlotProject> BuildSitePlots = new List<BuildSitePlotProject>();

	public GameObject SmokePrefab;

	private GameObject SmokeObject;

	public Interaction Interaction;

	public GameObject PlacementSquare;

	public Structure Structure;

	private Structures_BuildSiteProject _StructureInfo;

	public Vector2Int Bounds = new Vector2Int(1, 1);

	public Transform RotatedObject;

	public List<Worshipper> Worshippers = new List<Worshipper>();

	private UIProgressIndicator _uiProgressIndicator;

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

	public virtual Structures_BuildSiteProject StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_BuildSiteProject;
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
		foreach (BuildSitePlotProject buildSitePlot in BuildSitePlots)
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
	}

	private void OnDisable()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_BuildSiteProject structureBrain = StructureBrain;
		structureBrain.OnBuildProgressChanged = (Action)Delegate.Remove(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
		BuildSitePlots.Remove(this);
	}

	private void Start()
	{
		Structures_BuildSiteProject structureBrain = StructureBrain;
		structureBrain.OnBuildProgressChanged = (Action)Delegate.Combine(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
		OnBuildProgressChanged();
		Bounds = StructureBrain.Data.Bounds;
		SpawnTiles();
	}

	private void OnBrainAssigned()
	{
		Structures_BuildSiteProject structureBrain = StructureBrain;
		structureBrain.OnBuildProgressChanged = (Action)Delegate.Combine(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
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
			Structures_BuildSiteProject structureBrain = StructureBrain;
			structureBrain.OnBuildProgressChanged = (Action)Delegate.Remove(structureBrain.OnBuildProgressChanged, new Action(OnBuildProgressChanged));
		}
	}

	private void UpdateBar()
	{
		if (LetterBox.IsPlaying)
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
		if (base.gameObject.activeSelf)
		{
			cBuildSmokeRoutine = StartCoroutine(BuildSmokeRoutine());
		}
	}

	private IEnumerator BuildSmokeRoutine()
	{
		ParticleSystem[] array;
		if (SmokeObject == null)
		{
			SmokeObject = UnityEngine.Object.Instantiate(SmokePrefab, base.transform.position + new Vector3(0f, (float)Bounds.y / 2f, 0f), Quaternion.Euler(-180f, 0f, 0f), base.transform);
			SmokeObject.transform.localScale = Vector3.one * Mathf.Max(Bounds.x, Bounds.y);
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
		CentrePosition = base.transform.position + new Vector3(0f, (float)Bounds.y / 2f);
		CentrePosition = base.transform.position + new Vector3(0f, (float)Bounds.y / 2f);
		Interaction.ActivatorOffset = CentrePosition - base.transform.position;
		Interaction.ActivateDistance = Bounds.x;
	}
}
