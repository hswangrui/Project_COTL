using System;
using System.Collections;
using MMBiomeGeneration;
using src;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class LightingManager : BaseMonoBehaviour
{
	private enum FinalizedState
	{
		StateA,
		StateB
	}

	public struct LightRotationRange
	{
		public Vector3 startRot;

		public Vector3 endRot;
	}

	public BiomeLightingSettings accessibleLightingSetting;

	[Space]
	public bool debugGUI;

	private CoroutineQueue queue;

	private bool isTransitionActive;

	public bool isTODTransition;

	public bool lerpActive;

	private float timer;

	private float progress;

	private float deltaTimeMult = 1f;

	private static LightingManager _instance;

	public BiomeLightingSettings dawnSettings;

	public BiomeLightingSettings morningSettings;

	public BiomeLightingSettings afternoonSettings;

	public BiomeLightingSettings duskSettings;

	public BiomeLightingSettings nightSettings;

	public float transitionDuration = 5f;

	public AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public float transitionDurationMultiplier = 1f;

	private BiomeLightingSettings targetSettings;

	[HideInInspector]
	public BiomeLightingSettings globalOverrideSettings;

	[HideInInspector]
	public BiomeLightingSettings overrideSettings;

	public bool inLeaderEncounter;

	public bool inOverride;

	public bool inGlobalOverride;

	private bool hideShadowsInTransition;

	public float StencilInfluence;

	private Light mainDirLight;

	private AmplifyColorEffect amplifyColor;

	private ScreenSpaceOverlay screenSpaceOverlay;

	public bool overrideNaturalLightRot;

	private const string mainDirLightTag = "MainDirLight";

	public static readonly int overrideInfluenceID = Shader.PropertyToID("_StencilInfluence");

	private float globalStencilInfluence = 0.5f;

	private Texture currentBlendShadowLUT;

	private Texture currentBlendHighlightLUT;

	public PostProcessVolume ppv;

	private AmplifyPostEffect amplifyPostEffect;

	private AnimationCurve nightTransitionCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0f, 0f));

	private FinalizedState finalizedState;

	private static readonly int TimeOfDayColor = Shader.PropertyToID("_TimeOfDayColor");

	private static readonly int ItemInWoodsColor = Shader.PropertyToID("_ItemInWoodsColor");

	private static readonly int GlobalFogDist = Shader.PropertyToID("_GlobalFogDist");

	private static readonly int VerticalFogZOffset = Shader.PropertyToID("_VerticalFog_ZOffset");

	private static readonly int VerticalFogGradientSpread = Shader.PropertyToID("_VerticalFog_GradientSpread");

	private static readonly int GlobalHCol = Shader.PropertyToID("_GlobalHCol");

	private static readonly int GlobalSCol = Shader.PropertyToID("_GlobalSCol");

	private bool removeLightingEffectsEnabled;

	public static LightingManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public BiomeLightingSettings currentSettings { get; set; }

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
		Init();
	}

	public void Init()
	{
		ppv.profile.TryGetSettings<AmplifyPostEffect>(out amplifyPostEffect);
		if (!amplifyPostEffect)
		{
			amplifyPostEffect = ppv.profile.AddSettings<AmplifyPostEffect>();
		}
		mainDirLight = GameObject.FindGameObjectWithTag("MainDirLight").GetComponent<Light>();
		amplifyColor = Camera.main.GetComponent<AmplifyColorEffect>();
		screenSpaceOverlay = base.transform.parent.GetComponentInChildren<ScreenSpaceOverlay>();
		screenSpaceOverlay.Init();
	}

	private void Start()
	{
		currentSettings = SetCurrentLightingSettings();
		targetSettings = new BiomeLightingSettings();
		queue = new CoroutineQueue(this);
		queue.StartLoop();
		SetUpTimeOfDay();
	}

	private void OnEnable()
	{
		BiomeGenerator.OnBiomeGenerated += SetUpTimeOfDay;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(TransitionTimeOfDay));
	}

	private void OnDisable()
	{
		inOverride = false;
		BiomeGenerator.OnBiomeGenerated -= SetUpTimeOfDay;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(TransitionTimeOfDay));
	}

	private void OnDestroy()
	{
		inOverride = false;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(TransitionTimeOfDay));
		if (this == _instance)
		{
			_instance = null;
		}
	}

	public BiomeLightingSettings SetCurrentLightingSettings()
	{
		BiomeLightingSettings biomeLightingSettings = ScriptableObject.CreateInstance<BiomeLightingSettings>();
		biomeLightingSettings.AmbientColour = RenderSettings.ambientLight;
		biomeLightingSettings.DirectionalLightColour = mainDirLight.color;
		biomeLightingSettings.DirectionalLightIntensity = mainDirLight.intensity;
		biomeLightingSettings.ShadowStrength = mainDirLight.shadowStrength;
		biomeLightingSettings.LightRotation = mainDirLight.transform.rotation.eulerAngles;
		biomeLightingSettings.GlobalHighlightColor = Shader.GetGlobalColor(GlobalHCol);
		biomeLightingSettings.GlobalShadowColor = Shader.GetGlobalColor(GlobalSCol);
		biomeLightingSettings.LutTexture_Shadow = amplifyPostEffect.LutTexture.value;
		biomeLightingSettings.LutTexture_Lit = amplifyPostEffect.LutHighlightTexture.value;
		biomeLightingSettings.StencilInfluence = Shader.GetGlobalFloat(overrideInfluenceID);
		biomeLightingSettings.Exposure = amplifyPostEffect.Exposure.value;
		biomeLightingSettings.FogColor = Shader.GetGlobalColor(ItemInWoodsColor);
		biomeLightingSettings.FogDist = Shader.GetGlobalVector(GlobalFogDist);
		biomeLightingSettings.FogHeight = Shader.GetGlobalFloat(VerticalFogZOffset);
		biomeLightingSettings.FogSpread = Shader.GetGlobalFloat(VerticalFogGradientSpread);
		biomeLightingSettings.GodRayColor = Shader.GetGlobalColor(TimeOfDayColor);
		biomeLightingSettings.ScreenSpaceOverlayMat = screenSpaceOverlay.GetRenderer().sharedMaterials[0];
		biomeLightingSettings.overrideLightingProperties = new OverrideLightingProperties();
		currentBlendShadowLUT = ((amplifyPostEffect.LutBlendTexture.value != null) ? amplifyPostEffect.LutBlendTexture.value : amplifyPostEffect.LutTexture.value);
		currentBlendHighlightLUT = ((amplifyPostEffect.LutHighlightBlendTexture.value != null) ? amplifyPostEffect.LutHighlightBlendTexture.value : amplifyPostEffect.LutHighlightTexture.value);
		return biomeLightingSettings;
	}

	public Vector3 GetLightRotation(DayPhase dayPhase)
	{
		LightRotationRange lightRotationRange = default(LightRotationRange);
		switch (dayPhase)
		{
		case DayPhase.Dawn:
			lightRotationRange.startRot = new Vector3(-20f, -60f, 0f);
			lightRotationRange.endRot = new Vector3(-40f, -30f, 0f);
			break;
		case DayPhase.Morning:
			lightRotationRange.startRot = new Vector3(-40f, -30f, 0f);
			lightRotationRange.endRot = new Vector3(-50f, 0f, 0f);
			break;
		case DayPhase.Afternoon:
			lightRotationRange.startRot = new Vector3(-50f, 0f, 0f);
			lightRotationRange.endRot = new Vector3(-40f, 30f, 0f);
			break;
		case DayPhase.Dusk:
			lightRotationRange.startRot = new Vector3(-40f, 30f, 0f);
			lightRotationRange.endRot = new Vector3(-20f, 60f, 0f);
			break;
		case DayPhase.Night:
			lightRotationRange.startRot = new Vector3(-25f, -60f, 0f);
			lightRotationRange.endRot = new Vector3(-25f, 60f, 0f);
			break;
		default:
			Debug.Log("Oh uh day phase not right in lighting manager");
			lightRotationRange.startRot = Vector3.zero;
			lightRotationRange.endRot = Vector3.zero;
			break;
		}
		return Vector3.Lerp(lightRotationRange.startRot, lightRotationRange.endRot, TimeManager.CurrentPhaseProgress);
	}

	private void LateUpdate()
	{
		if (mainDirLight != null && !overrideNaturalLightRot)
		{
			mainDirLight.transform.rotation = Quaternion.Euler(GetLightRotation(TimeManager.CurrentPhase));
			if (TimeManager.CurrentPhase == DayPhase.Dusk)
			{
				float t = Mathf.InverseLerp(0.9f, 1f, TimeManager.CurrentPhaseProgress);
				mainDirLight.shadowStrength = Mathf.Lerp(targetSettings.ShadowStrength, 0f, t);
			}
			else if (TimeManager.CurrentPhase == DayPhase.Night)
			{
				float t = Mathf.InverseLerp(0f, 0.1f, TimeManager.CurrentPhaseProgress) * (1f - Mathf.InverseLerp(0.9f, 1f, TimeManager.CurrentPhaseProgress));
				mainDirLight.shadowStrength = Mathf.Lerp(0f, targetSettings.ShadowStrength, t);
			}
			else if (TimeManager.CurrentPhase == DayPhase.Dawn)
			{
				float t = Mathf.InverseLerp(0f, 0.1f, TimeManager.CurrentPhaseProgress);
				mainDirLight.shadowStrength = Mathf.Lerp(0f, targetSettings.ShadowStrength, t);
			}
		}
	}

	public void TransitionTimeOfDay()
	{
		UpdateLighting(false);
	}

	public void SetUpTimeOfDay()
	{
		if (!DeathCatRoomMarker.IsDeathCatRoom)
		{
			transitionDurationMultiplier = 0f;
			UpdateLighting(false);
			transitionDurationMultiplier = 1f;
		}
	}

	public void PrepareLightingSettings(bool ignoreAccessibilitySetting = false)
	{
		if (SettingsManager.Settings.Accessibility.RemoveLightingEffects && !ignoreAccessibilitySetting)
		{
			targetSettings = accessibleLightingSetting;
			Debug.Log("Remove Lighting Effects On");
			removeLightingEffectsEnabled = true;
			return;
		}
		if (removeLightingEffectsEnabled)
		{
			targetSettings = null;
			removeLightingEffectsEnabled = false;
		}
		if (targetSettings == null)
		{
			targetSettings = ScriptableObject.CreateInstance<BiomeLightingSettings>();
		}
		switch (TimeManager.CurrentPhase)
		{
		case DayPhase.Dawn:
			targetSettings = dawnSettings;
			break;
		case DayPhase.Morning:
			targetSettings = morningSettings;
			break;
		case DayPhase.Afternoon:
			targetSettings = afternoonSettings;
			break;
		case DayPhase.Dusk:
			targetSettings = duskSettings;
			break;
		case DayPhase.Night:
			targetSettings = nightSettings;
			break;
		}
		if (inGlobalOverride && globalOverrideSettings.overrideLightingProperties != null)
		{
			if (globalOverrideSettings.overrideLightingProperties.Enabled)
			{
				targetSettings = MergeLightingSettings(targetSettings, globalOverrideSettings, globalOverrideSettings.overrideLightingProperties);
			}
			else
			{
				targetSettings = globalOverrideSettings;
			}
		}
		if (inOverride && overrideSettings.overrideLightingProperties != null)
		{
			if (overrideSettings.overrideLightingProperties.Enabled)
			{
				targetSettings = MergeLightingSettings(targetSettings, overrideSettings, overrideSettings.overrideLightingProperties);
			}
			else
			{
				targetSettings = overrideSettings;
			}
		}
	}

	public void UpdateLighting(bool allowInterupt, bool ignoreAccessibilitySetting = false)
	{
		Debug.Log(string.Format("Update Lighting {0}", ignoreAccessibilitySetting));
		if (allowInterupt)
		{
			if (lerpActive)
			{
				Debug.Log("Lerp Active");
				if (!isTODTransition)
				{
					deltaTimeMult *= -1f;
				}
				else
				{
					queue.EnqueueAction(TransitionLighting(transitionDuration * transitionDurationMultiplier, true, ignoreAccessibilitySetting));
				}
				return;
			}
			Debug.Log("Lerp Not Active");
			if (queue == null)
			{
				queue = new CoroutineQueue(this);
			}
			queue.EnqueueAction(TransitionLighting(transitionDuration * transitionDurationMultiplier, true, ignoreAccessibilitySetting));
		}
		else if (queue != null)
		{
			queue.EnqueueAction(TransitionLighting(transitionDuration * transitionDurationMultiplier, false, ignoreAccessibilitySetting));
		}
	}

	public BiomeLightingSettings MergeLightingSettings(BiomeLightingSettings currSettings, BiomeLightingSettings LightingSettings, OverrideLightingProperties overrideLightingProperties)
	{
		BiomeLightingSettings biomeLightingSettings = ScriptableObject.CreateInstance<BiomeLightingSettings>();
		biomeLightingSettings.UnscaledTime = (overrideLightingProperties.UnscaledTime ? LightingSettings.UnscaledTime : currSettings.UnscaledTime);
		biomeLightingSettings.AmbientColour = (overrideLightingProperties.AmbientColor ? LightingSettings.AmbientColour : currSettings.AmbientColour);
		biomeLightingSettings.DirectionalLightColour = (overrideLightingProperties.DirectionalLightColor ? LightingSettings.DirectionalLightColour : currSettings.DirectionalLightColour);
		biomeLightingSettings.DirectionalLightIntensity = (overrideLightingProperties.DirectionalLightIntensity ? LightingSettings.DirectionalLightIntensity : currSettings.DirectionalLightIntensity);
		biomeLightingSettings.ShadowStrength = (overrideLightingProperties.ShadowStrength ? LightingSettings.ShadowStrength : currSettings.ShadowStrength);
		biomeLightingSettings.LightRotation = (overrideLightingProperties.LightRotation ? LightingSettings.LightRotation : currSettings.LightRotation);
		biomeLightingSettings.GlobalHighlightColor = (overrideLightingProperties.GlobalHighlightColor ? LightingSettings.GlobalHighlightColor : currSettings.GlobalHighlightColor);
		biomeLightingSettings.GlobalShadowColor = (overrideLightingProperties.GlobalShadowColor ? LightingSettings.GlobalShadowColor : currSettings.GlobalShadowColor);
		biomeLightingSettings.LutTexture_Shadow = (overrideLightingProperties.LUTTextureShadow ? LightingSettings.LutTexture_Shadow : currSettings.LutTexture_Shadow);
		biomeLightingSettings.LutTexture_Lit = (overrideLightingProperties.LUTTextureLit ? LightingSettings.LutTexture_Lit : currSettings.LutTexture_Lit);
		biomeLightingSettings.StencilInfluence = (overrideLightingProperties.StencilInfluence ? LightingSettings.StencilInfluence : currSettings.StencilInfluence);
		biomeLightingSettings.Exposure = (overrideLightingProperties.Exposure ? LightingSettings.Exposure : currSettings.Exposure);
		biomeLightingSettings.FogColor = (overrideLightingProperties.FogColor ? LightingSettings.FogColor : currSettings.FogColor);
		biomeLightingSettings.FogDist = (overrideLightingProperties.FogDist ? LightingSettings.FogDist : currSettings.FogDist);
		biomeLightingSettings.FogHeight = (overrideLightingProperties.FogHeight ? LightingSettings.FogHeight : currSettings.FogHeight);
		biomeLightingSettings.FogSpread = (overrideLightingProperties.FogSpread ? LightingSettings.FogSpread : currSettings.FogSpread);
		biomeLightingSettings.GodRayColor = (overrideLightingProperties.GodRayColor ? LightingSettings.GodRayColor : currSettings.GodRayColor);
		biomeLightingSettings.ScreenSpaceOverlayMat = (overrideLightingProperties.ScreenSpaceOverlayMat ? LightingSettings.ScreenSpaceOverlayMat : currSettings.ScreenSpaceOverlayMat);
		biomeLightingSettings.overrideLightingProperties = overrideLightingProperties;
		return biomeLightingSettings;
	}

	public IEnumerator TransitionLighting(float transitionDuration, bool allowInterupt, bool ignoreAccessibilitySetting = false)
	{
		isTransitionActive = true;
		PrepareLightingSettings(ignoreAccessibilitySetting);
		BiomeLightingSettings currSettings = currentSettings;
		BiomeLightingSettings newSettings = targetSettings;
		if (currSettings.IsEquivalent(newSettings))
		{
			isTransitionActive = false;
			yield break;
		}
		if (SettingsManager.Settings.Accessibility.RemoveLightingEffects && !ignoreAccessibilitySetting)
		{
			Debug.Log("Remove Lighting Effects On");
			currSettings = accessibleLightingSetting;
			newSettings = accessibleLightingSetting;
		}
		if (allowInterupt)
		{
			isTODTransition = false;
		}
		else
		{
			isTODTransition = true;
		}
		if (newSettings.overrideLightingProperties.LightRotation)
		{
			overrideNaturalLightRot = true;
		}
		amplifyPostEffect.LutBlendTexture.value = newSettings.LutTexture_Shadow;
		amplifyPostEffect.LutHighlightBlendTexture.value = newSettings.LutTexture_Lit;
		bool transitionScreenSpaceOverlay = false;
		if (newSettings.ScreenSpaceOverlayMat != currSettings.ScreenSpaceOverlayMat)
		{
			screenSpaceOverlay.SetMaterials(currSettings.ScreenSpaceOverlayMat, newSettings.ScreenSpaceOverlayMat);
			transitionScreenSpaceOverlay = true;
		}
		Quaternion initLightRot = mainDirLight.transform.rotation;
		timer = 0f;
		lerpActive = true;
		while (lerpActive)
		{
			if (newSettings.UnscaledTime || currSettings.UnscaledTime)
			{
				Debug.Log("Using Unscaled Time");
				timer += Time.unscaledDeltaTime * deltaTimeMult;
			}
			else if (allowInterupt)
			{
				timer += Time.deltaTime * deltaTimeMult;
			}
			else
			{
				deltaTimeMult = 1f;
				timer += Time.deltaTime * deltaTimeMult;
			}
			float num = ((transitionDuration != 0f) ? transitionCurve.Evaluate(Mathf.Clamp01(timer / transitionDuration)) : 1f);
			RenderSettings.ambientLight = Color.Lerp(currSettings.AmbientColour, newSettings.AmbientColour, num);
			Shader.SetGlobalColor(TimeOfDayColor, Color.Lerp(currSettings.GodRayColor, newSettings.GodRayColor, num));
			Shader.SetGlobalColor(ItemInWoodsColor, Color.Lerp(currSettings.FogColor, newSettings.FogColor, num));
			Shader.SetGlobalVector(GlobalFogDist, Vector2.Lerp(currSettings.FogDist, newSettings.FogDist, num));
			Shader.SetGlobalFloat(VerticalFogZOffset, Mathf.Lerp(currSettings.FogHeight, newSettings.FogHeight, num));
			Shader.SetGlobalFloat(VerticalFogGradientSpread, Mathf.Lerp(currSettings.FogSpread, newSettings.FogSpread, num));
			Shader.SetGlobalColor(GlobalHCol, Color.Lerp(currSettings.GlobalHighlightColor, newSettings.GlobalHighlightColor, num));
			Shader.SetGlobalColor(GlobalSCol, Color.Lerp(currSettings.GlobalShadowColor, newSettings.GlobalShadowColor, num));
			if (Camera.main != null)
			{
				Camera.main.backgroundColor = Color.Lerp(currSettings.FogColor, newSettings.FogColor, num);
			}
			mainDirLight.color = Color.Lerp(currSettings.DirectionalLightColour, newSettings.DirectionalLightColour, num);
			mainDirLight.intensity = Mathf.Lerp(currSettings.DirectionalLightIntensity, newSettings.DirectionalLightIntensity, num);
			mainDirLight.shadowStrength = Mathf.Lerp(currSettings.ShadowStrength, newSettings.ShadowStrength, num);
			if (TimeManager.CurrentPhase == DayPhase.Dusk)
			{
				float num2 = Mathf.InverseLerp(0.9f, 1f, TimeManager.CurrentPhaseProgress);
				mainDirLight.shadowStrength *= 1f - num2;
			}
			else if (TimeManager.CurrentPhase == DayPhase.Night)
			{
				float num2 = Mathf.InverseLerp(0f, 0.1f, TimeManager.CurrentPhaseProgress) * (1f - Mathf.InverseLerp(0.9f, 1f, TimeManager.CurrentPhaseProgress));
				mainDirLight.shadowStrength *= num2;
			}
			else if (TimeManager.CurrentPhase == DayPhase.Dawn)
			{
				float num2 = Mathf.InverseLerp(0f, 0.1f, TimeManager.CurrentPhaseProgress);
				mainDirLight.shadowStrength *= num2;
			}
			if (!currSettings.overrideLightingProperties.LightRotation)
			{
				initLightRot = Quaternion.Euler(GetLightRotation(TimeManager.CurrentPhase));
			}
			Quaternion b = (newSettings.overrideLightingProperties.LightRotation ? Quaternion.Euler(newSettings.LightRotation) : Quaternion.Euler(GetLightRotation(TimeManager.CurrentPhase)));
			mainDirLight.transform.rotation = Quaternion.Lerp(initLightRot, b, num);
			amplifyPostEffect.BlendAmount.value = num;
			globalStencilInfluence = Mathf.Lerp(currSettings.StencilInfluence, newSettings.StencilInfluence, num);
			Shader.SetGlobalFloat(overrideInfluenceID, globalStencilInfluence);
			float value = Mathf.Lerp(currSettings.Exposure, newSettings.Exposure, num);
			amplifyPostEffect.Exposure.value = value;
			if (transitionScreenSpaceOverlay)
			{
				screenSpaceOverlay.TransitionMaterial(num);
			}
			if (timer < 0f)
			{
				finalizedState = FinalizedState.StateA;
				lerpActive = false;
			}
			else if (timer > transitionDuration)
			{
				finalizedState = FinalizedState.StateB;
				lerpActive = false;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForEndOfFrame();
		currentSettings = ((finalizedState == FinalizedState.StateA && allowInterupt) ? currSettings : newSettings);
		timer = 0f;
		deltaTimeMult = 1f;
		transitionDurationMultiplier = 1f;
		overrideNaturalLightRot = currentSettings.overrideLightingProperties.LightRotation;
		amplifyPostEffect.BlendAmount.value = 0f;
		amplifyPostEffect.LutTexture.value = currentSettings.LutTexture_Shadow;
		amplifyPostEffect.LutBlendTexture.value = null;
		amplifyPostEffect.LutHighlightTexture.value = currentSettings.LutTexture_Lit;
		amplifyPostEffect.LutHighlightBlendTexture.value = null;
		if (transitionScreenSpaceOverlay)
		{
			screenSpaceOverlay.SetMaterials(currentSettings.ScreenSpaceOverlayMat, null);
		}
		timer = 0f;
		deltaTimeMult = 1f;
		if (isTODTransition)
		{
			isTODTransition = false;
		}
		isTransitionActive = false;
	}

	private void OnGUI()
	{
		if (!debugGUI)
		{
			return;
		}
		GUI.skin.label.fontSize = 20;
		GUI.color = Color.yellow;
		GUI.HorizontalSlider(new Rect(25f, 325f, 500f, 50f), amplifyPostEffect.BlendAmount.value, 0f, 1f);
		GUI.Label(new Rect(25f, 350f, 500f, 25f), "Is transition active: " + isTransitionActive);
		GUI.Label(new Rect(25f, 375f, 500f, 25f), "Is transition dir: " + deltaTimeMult);
		int num = 400;
		GUI.Label(new Rect(25f, 400f, 500f, 25f), "QUEUE:");
		foreach (IEnumerator action in queue.Actions)
		{
			num += 25;
			GUI.Label(new Rect(25f, num, 500f, 25f), action.ToString());
		}
		if (!overrideNaturalLightRot && !isTransitionActive)
		{
			GUI.Label(new Rect(25f, 500f, 500f, 25f), "Natural Light Rotation: " + mainDirLight.transform.rotation);
		}
		else
		{
			GUI.Label(new Rect(25f, 500f, 500f, 25f), "Overwriting Light Rotation: " + mainDirLight.transform.rotation);
			GUI.Label(new Rect(25f, 525f, 500f, 25f), "Target Natural Rotation: " + Quaternion.Euler(GetLightRotation(TimeManager.CurrentPhase)));
		}
		GUI.Label(new Rect(25f, 575f, 500f, 25f), "is TOD Transition?: " + isTODTransition);
	}
}
