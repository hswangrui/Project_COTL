using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class LightingManagerVolume : BaseMonoBehaviour
{
	public bool isGlobal;

	private bool lastGlobalState;

	public bool TransitionOnEnableDisable;

	public float transitionDurationMultiplierAdjustment = 0.2f;

	public bool isExitTransitionOn;

	public float exitTransitionDurationMultiplierAdjustment = 0.2f;

	public BiomeLightingSettings LightingSettings;

	private BiomeLightingSettings targetLightSettings;

	public OverrideLightingProperties overrideLightingProperties;

	private bool isLeaderEncounter = true;

	public bool _isActualLeaderEncounter;

	public bool inTrigger;

	[SerializeField]
	private bool _ignoreLightingAccessibilitySetting;

	public void Start()
	{
		base.gameObject.layer = 21;
		if ((bool)LightingSettings)
		{
			LightingSettings.overrideLightingProperties = overrideLightingProperties;
		}
		if (isGlobal && LightingManager.Instance != null)
		{
			LightingManager.Instance.lerpActive = transitionDurationMultiplierAdjustment != 0f;
			LightingManager.Instance.globalOverrideSettings = LightingSettings;
			LightingManager.Instance.inGlobalOverride = true;
			LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
			LightingManager.Instance.UpdateLighting(false, _ignoreLightingAccessibilitySetting);
			LightingManager.Instance.transitionDurationMultiplier = 1f;
		}
		lastGlobalState = isGlobal;
	}

	public void OnEnable()
	{
		inTrigger = false;
		if (TransitionOnEnableDisable)
		{
			TransitionIn();
		}
	}

	public void OnDestroy()
	{
		if (!(LightingManager.Instance == null))
		{
			if (isGlobal)
			{
				LightingManager.Instance.inGlobalOverride = false;
			}
			LightingManager.Instance.lerpActive = transitionDurationMultiplierAdjustment != 0f;
			LightingManager.Instance.inOverride = false;
			LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
			LightingManager.Instance.UpdateLighting(false, _ignoreLightingAccessibilitySetting);
			LightingManager.Instance.transitionDurationMultiplier = 1f;
			if (_isActualLeaderEncounter)
			{
				LightingManager.Instance.inLeaderEncounter = false;
			}
		}
	}

	public void OnDisable()
	{
		if (!(LightingManager.Instance == null))
		{
			if (isGlobal)
			{
				LightingManager.Instance.inGlobalOverride = false;
			}
			if (TransitionOnEnableDisable)
			{
				TransitionOut();
				inTrigger = false;
			}
			if (_isActualLeaderEncounter)
			{
				LightingManager.Instance.inLeaderEncounter = false;
			}
		}
	}

	private void TransitionIn()
	{
		if (!(LightingManager.Instance == null))
		{
			LightingManager.Instance.lerpActive = transitionDurationMultiplierAdjustment != 0f;
			LightingManager.Instance.inOverride = true;
			LightingSettings.overrideLightingProperties = overrideLightingProperties;
			LightingManager.Instance.overrideSettings = LightingSettings;
			LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
			LightingManager.Instance.UpdateLighting(true, _ignoreLightingAccessibilitySetting);
			LightingManager.Instance.transitionDurationMultiplier = 1f;
		}
	}

	private void TransitionOut()
	{
		float num = (isExitTransitionOn ? exitTransitionDurationMultiplierAdjustment : transitionDurationMultiplierAdjustment);
		LightingManager.Instance.lerpActive = num != 0f;
		LightingManager.Instance.inOverride = false;
		LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
		LightingManager.Instance.UpdateLighting(true, _ignoreLightingAccessibilitySetting);
		LightingManager.Instance.transitionDurationMultiplier = 1f;
	}

	public void Update()
	{
		if (!TransitionOnEnableDisable && lastGlobalState != isGlobal && !(LightingManager.Instance == null))
		{
			if (isGlobal)
			{
				LightingManager.Instance.lerpActive = transitionDurationMultiplierAdjustment != 0f;
				LightingSettings.overrideLightingProperties = overrideLightingProperties;
				LightingManager.Instance.globalOverrideSettings = LightingSettings;
				LightingManager.Instance.inGlobalOverride = true;
				LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
				LightingManager.Instance.UpdateLighting(false, _ignoreLightingAccessibilitySetting);
				LightingManager.Instance.transitionDurationMultiplier = 1f;
			}
			else
			{
				LightingManager.Instance.lerpActive = transitionDurationMultiplierAdjustment != 0f;
				LightingManager.Instance.inGlobalOverride = false;
				LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
				LightingManager.Instance.UpdateLighting(false, _ignoreLightingAccessibilitySetting);
				LightingManager.Instance.transitionDurationMultiplier = 1f;
			}
			lastGlobalState = isGlobal;
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (TransitionOnEnableDisable || isGlobal || !(other.gameObject.tag == "Player") || inTrigger || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		LightingManager.Instance.inOverride = true;
		LightingSettings.overrideLightingProperties = overrideLightingProperties;
		LightingManager.Instance.overrideSettings = LightingSettings;
		LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplierAdjustment;
		LightingManager.Instance.lerpActive = transitionDurationMultiplierAdjustment != 0f;
		if (isLeaderEncounter)
		{
			LightingManager.Instance.isTODTransition = true;
			if (_isActualLeaderEncounter)
			{
				LightingManager.Instance.inLeaderEncounter = true;
			}
		}
		LightingManager.Instance.UpdateLighting(true, _ignoreLightingAccessibilitySetting);
		LightingManager.Instance.transitionDurationMultiplier = 1f;
		inTrigger = true;
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!TransitionOnEnableDisable && !isGlobal && other.gameObject.tag == "Player" && inTrigger)
		{
			float transitionDurationMultiplier = (isExitTransitionOn ? exitTransitionDurationMultiplierAdjustment : transitionDurationMultiplierAdjustment);
			LightingManager.Instance.inOverride = false;
			LightingManager.Instance.lerpActive = false;
			LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplier;
			if (_isActualLeaderEncounter)
			{
				LightingManager.Instance.inLeaderEncounter = false;
			}
			LightingManager.Instance.UpdateLighting(true, _ignoreLightingAccessibilitySetting);
			LightingManager.Instance.transitionDurationMultiplier = 1f;
			inTrigger = false;
		}
	}
}
