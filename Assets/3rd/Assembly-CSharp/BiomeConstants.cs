using System;
using System.Collections;
using System.Collections.Generic;
using AmplifyColor;
using DG.Tweening;
using MMBiomeGeneration;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BiomeConstants : BaseMonoBehaviour
{
	public enum TypeOfParticle
	{
		blank,
		wood,
		clay,
		grass,
		stone,
		bone,
		glass,
		grass_2,
		grass_3,
		grass_4,
		grass_5
	}

	[SerializeField]
	private GoopFade goopFade;

	[SerializeField]
	private PsychedelicFade psychedelicFade;

	[SerializeField]
	private ImpactFrame impactFrame;

	public List<ObjectPoolObject> ObjectPoolObjects = new List<ObjectPoolObject>();

	private static GameObject _ObjectPoolParent;

	public static BiomeConstants Instance;

	[FormerlySerializedAs("_uiProgressIndicator")]
	[FormerlySerializedAs("ProgressIndicator")]
	public UIProgressIndicator ProgressIndicatorTemplate;

	public Shader _uberShaderNoDepth;

	public Shader _uberShader;

	public Material grassNormal;

	public Material grassNoDepth;

	public GameObject ShopKeeperChef;

	public EnableRecieveShadowsSpriteRenderer enableRecieve;

	[SerializeField]
	private ParticleSystem dustCloudParticles;

	private ParticleSystem.MinMaxCurve dustCloudParticlesInitSize;

	private ParticleSystem.MinMaxCurve dustCloudParticlesInitLifetime;

	public ParticleSystem footprintParticles;

	private ParticleSystem.MinMaxCurve footprintParticlesInitSize;

	private ParticleSystem.MinMaxCurve footprintParticlesInitLifetime;

	private PostProcessLayer _postProcessLayer;

	private bool swapShaders;

	public Desaturate_StencilRTMaskPPSSettings desaturateStencil;

	public Vignette vignette;

	public float VignetteDefaultValue = 0.3f;

	public ChromaticAberration chromaticAbberration;

	public float ChromaticAberrationDefaultValue = 0.072f;

	public Bloom bloom;

	public DepthOfField depthoffield;

	public float DepthOfFieldDefaultLength;

	private Coroutine depthOfFieldRoutine;

	public AmplifyPostEffect amplifyPostEffect;

	public const float FlashTick = 0.12f;

	public List<Texture2D> ColorBlindLUT = new List<Texture2D>();

	private AmplifyColorBase amp;

	private bool TrackDepthOfField;

	public ParticleSystem steppingInWaterParticles;

	private ParticleSystem.EmitParams DustCloudParticlesParams;

	private ParticleSystem.EmitParams emitParams;

	public GameObject HitImpactPrefab;

	public SimpleParticleVFX PlayerHitImpactPrefab_Ice;

	public SimpleParticleVFX PlayerHitImpactPrefab_Poison;

	public SimpleParticleVFX PlayerHitImpactPrefabDown;

	public SimpleParticleVFX PlayerHitImpactPrefabDownRight;

	public SimpleParticleVFX PlayerHitImpactPrefabUp;

	public SimpleVFX PlayerHitCritical;

	public GameObject blackSoulPrefab;

	[SerializeField]
	public ParticleSystem BloodSplatterGround;

	private ParticleSystem.MinMaxCurve BloodSplatterGroundParticlesInitSize;

	public SimpleVFX BloodImpactPrefab;

	public GameObject HitFX_BlockedRedSmall;

	public Vector3 defaultScale = Vector3.one;

	public GameObject HeartPickUpVFX;

	public GameObject BlockImpactShield;

	public GameObject BlockImpact;

	public GameObject ProtectorImpactIcon;

	public GameObject HitFXSoulPrefab;

	public SimpleVFX HitFXPrefab;

	[SerializeField]
	public ParticleSystem EmitRubbleHitFX;

	[SerializeField]
	public ParticleSystem EmitGroundSmashVFX;

	private ParticleSystem.MinMaxCurve EmitGroundSmashVFXInitSize;

	[SerializeField]
	public ParticleSystem DeadBodyExplodeVFX;

	[SerializeField]
	private Particle_Chunk particleChunkPrefab;

	public GameObject GrenadeBulletImpact_A;

	public GameObject SpawnInWhite;

	public GameObject GroundSmash_Medium;

	public GameObject HitFX_Blocked;

	public SimpleVFX GhostTwirlAttack;

	public SimpleVFX SlamVFX;

	public SimpleVFX craterVFX;

	public SimpleVFX groundParticles;

	public SetSpriteshapeMaterial setSpriteshapeMaterial;

	public GameObject PickUpVFX;

	public GameObject SmokeExplosionVFX;

	public ParticleSystem interactionSmokeVFX;

	public GameObject ConfettiVFX;

	public ParticleSystem ParticleChunkSystem_Grass;

	public ParticleSystem ParticleChunkSystem_Wood;

	public ParticleSystem ParticleChunkSystem_Clay;

	public ParticleSystem ParticleChunkSystem_Bones;

	public ParticleSystem ParticleChunkSystem_Grass_2;

	public ParticleSystem ParticleChunkSystem_Grass_3;

	public ParticleSystem ParticleChunkSystem_Grass_4;

	public ParticleSystem ParticleChunkSystem_Grass_5;

	public ParticleSystem ParticleChunkSystem_Stone;

	public ParticleSystem.TextureSheetAnimationModule ParticleChunkTextures;

	private float RandomVariation = 0.5f;

	public PostProcessVolume ppv;

	public Image timeOfDayOverlay;

	public ParticleSystem fireflies;

	public Color dawnColor;

	public Color dayColor;

	public Color duskColor;

	public Color nightColor;

	public float overlayAmount = 0.9f;

	private float overlayAmountDay = 1f;

	public float lerpTime = 10f;

	private float speed = 1f;

	public GameObject DungeonChallengeRatoo;

	public GameObject BlackHeartDamageIcon;

	public GameObject TarotCardDamageIcon;

	public GameObject DamageTextIcon;

	public static GameObject ObjectPoolParent
	{
		get
		{
			if (_ObjectPoolParent == null)
			{
				_ObjectPoolParent = new GameObject();
				_ObjectPoolParent.name = "ObjectPool Parent";
			}
			return _ObjectPoolParent;
		}
	}

	public bool IsFlashLightsActive
	{
		get
		{
			return SettingsManager.Settings.Accessibility.FlashingLights;
		}
	}

	public GameObject BloodSplatterPrefab
	{
		get
		{
			if (BiomeGenerator.Instance == null)
			{
				return null;
			}
			return BiomeGenerator.Instance.CurrentRoom.generateRoom.BloodSplatterPrefab;
		}
	}

	public void GoopFadeIn(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true)
	{
		goopFade.gameObject.SetActive(true);
		goopFade.FadeIn(_Duration, _Delay, UseDeltaTime);
	}

	public void GoopFadeOut(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true)
	{
		goopFade.gameObject.SetActive(true);
		goopFade.FadeOut(_Duration, _Delay, UseDeltaTime);
	}

	public void PsychedelicFadeIn(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true, Action onComplete = null)
	{
		psychedelicFade.gameObject.SetActive(true);
		psychedelicFade.FadeIn(_Duration, _Delay, UseDeltaTime, onComplete);
	}

	public void PsychedelicFadeOut(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true, Action onComplete = null)
	{
		psychedelicFade.gameObject.SetActive(true);
		psychedelicFade.FadeOut(_Duration, _Delay, UseDeltaTime, onComplete);
	}

	public void ImpactFrameForDuration(float _Duration = 0.2f, float _Delay = 0f)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			DeviceLightingManager.TransitionLighting(Color.white, Color.black, 0.5f);
			impactFrame.gameObject.SetActive(true);
			impactFrame.ShowForDuration(_Duration, _Delay);
		}
	}

	public void ImpactFrameForIn()
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			impactFrame.gameObject.SetActive(true);
			impactFrame.Show();
		}
	}

	public void ImpactFrameForOut()
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			impactFrame.gameObject.SetActive(true);
			impactFrame.Hide();
		}
	}

	public void EnableRecieveShadows()
	{
		enableRecieve.UpdateSpriteRenderers();
	}

	private void OnEnable()
	{
		Instance = this;
		ParticleSystem particleSystem = dustCloudParticles;
		if ((object)particleSystem != null)
		{
			particleSystem.gameObject.SetActive(true);
		}
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(newTimeOfDay));
		ObjectiveManager.OnObjectiveCompleted += ObjectiveManager_OnObjectiveCompleted;
		StartCoroutine(SwapShaders());
		Camera main = Camera.main;
		_postProcessLayer = main.GetComponent<PostProcessLayer>();
		main.allowHDR = true;
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnColorblindModeChanged = (Action)Delegate.Combine(instance.OnColorblindModeChanged, new Action(OnColourblindModeUpdated));
		GraphicsSettingsUtilities.OnEnvironmentSettingsChanged = (Action)Delegate.Combine(GraphicsSettingsUtilities.OnEnvironmentSettingsChanged, new Action(SwapShadersStart));
	}

	private void SwapShadersStart()
	{
		StartCoroutine(SwapShaders());
	}

	private IEnumerator SwapShaders()
	{
		swapShaders = false;
		while (SettingsManager.Settings == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (SettingsManager.Settings.Graphics.EnvironmentDetail == 0)
		{
			swapShaders = true;
		}
		if (swapShaders)
		{
			grassNormal.shader = _uberShaderNoDepth;
		}
		else
		{
			grassNormal.shader = _uberShader;
		}
	}

	private void OnDisable()
	{
		if (Instance != null)
		{
			if (Instance.grassNormal != null)
			{
				Instance.grassNormal.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
			}
			if (Instance.grassNoDepth != null)
			{
				Instance.grassNoDepth.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
			}
		}
		if (Instance == this)
		{
			Instance = null;
		}
		GraphicsSettingsUtilities.OnEnvironmentSettingsChanged = (Action)Delegate.Remove(GraphicsSettingsUtilities.OnEnvironmentSettingsChanged, new Action(SwapShadersStart));
		ParticleSystem particleSystem = footprintParticles;
		if ((object)particleSystem != null)
		{
			particleSystem.Clear();
		}
		ParticleSystem particleSystem2 = dustCloudParticles;
		if ((object)particleSystem2 != null)
		{
			particleSystem2.Clear();
		}
		ParticleSystem particleSystem3 = dustCloudParticles;
		if ((object)particleSystem3 != null)
		{
			particleSystem3.gameObject.SetActive(false);
		}
		ParticleSystem particleSystem4 = steppingInWaterParticles;
		if ((object)particleSystem4 != null)
		{
			particleSystem4.Clear();
		}
		ParticleSystem bloodSplatterGround = BloodSplatterGround;
		if ((object)bloodSplatterGround != null)
		{
			bloodSplatterGround.Clear();
		}
		ParticleSystem emitGroundSmashVFX = EmitGroundSmashVFX;
		if ((object)emitGroundSmashVFX != null)
		{
			emitGroundSmashVFX.Clear();
		}
		ParticleSystem emitRubbleHitFX = EmitRubbleHitFX;
		if ((object)emitRubbleHitFX != null)
		{
			emitRubbleHitFX.Clear();
		}
		ParticleSystem particleChunkSystem_Grass = ParticleChunkSystem_Grass;
		if ((object)particleChunkSystem_Grass != null)
		{
			particleChunkSystem_Grass.Clear();
		}
		ParticleSystem particleChunkSystem_Wood = ParticleChunkSystem_Wood;
		if ((object)particleChunkSystem_Wood != null)
		{
			particleChunkSystem_Wood.Clear();
		}
		ParticleSystem particleChunkSystem_Clay = ParticleChunkSystem_Clay;
		if ((object)particleChunkSystem_Clay != null)
		{
			particleChunkSystem_Clay.Clear();
		}
		ParticleSystem particleSystem5 = fireflies;
		if ((object)particleSystem5 != null)
		{
			particleSystem5.Clear();
		}
		ParticleSystem particleChunkSystem_Grass_ = ParticleChunkSystem_Grass_2;
		if ((object)particleChunkSystem_Grass_ != null)
		{
			particleChunkSystem_Grass_.Clear();
		}
		ParticleSystem particleChunkSystem_Grass_2 = ParticleChunkSystem_Grass_3;
		if ((object)particleChunkSystem_Grass_2 != null)
		{
			particleChunkSystem_Grass_2.Clear();
		}
		ParticleSystem particleChunkSystem_Grass_3 = ParticleChunkSystem_Grass_4;
		if ((object)particleChunkSystem_Grass_3 != null)
		{
			particleChunkSystem_Grass_3.Clear();
		}
		ParticleSystem particleChunkSystem_Grass_4 = ParticleChunkSystem_Grass_5;
		if ((object)particleChunkSystem_Grass_4 != null)
		{
			particleChunkSystem_Grass_4.Clear();
		}
		ParticleSystem particleChunkSystem_Bones = ParticleChunkSystem_Bones;
		if ((object)particleChunkSystem_Bones != null)
		{
			particleChunkSystem_Bones.Clear();
		}
		ParticleSystem particleChunkSystem_Stone = ParticleChunkSystem_Stone;
		if ((object)particleChunkSystem_Stone != null)
		{
			particleChunkSystem_Stone.Clear();
		}
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(newTimeOfDay));
		if (Singleton<AccessibilityManager>.Instance != null)
		{
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnColorblindModeChanged = (Action)Delegate.Remove(instance.OnColorblindModeChanged, new Action(OnColourblindModeUpdated));
		}
		ObjectiveManager.OnObjectiveCompleted -= ObjectiveManager_OnObjectiveCompleted;
		GraphicsSettingsUtilities.OnEnvironmentSettingsChanged = (Action)Delegate.Remove(GraphicsSettingsUtilities.OnEnvironmentSettingsChanged, new Action(SwapShadersStart));
		ObjectPoolObjects.Clear();
		_ObjectPoolParent = null;
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnDestroy()
	{
		ParticleSystem particleSystem = dustCloudParticles;
		if ((object)particleSystem != null)
		{
			particleSystem.gameObject.SetActive(false);
		}
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(newTimeOfDay));
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		footprintParticles.Clear();
		ParticleChunkSystem_Grass.Clear();
		ParticleChunkSystem_Wood.Clear();
		ParticleChunkSystem_Clay.Clear();
		EmitGroundSmashVFX.Clear();
		EmitRubbleHitFX.Clear();
		ParticleChunkSystem_Grass_2.Clear();
		ParticleChunkSystem_Grass_3.Clear();
		ParticleChunkSystem_Grass_4.Clear();
		ParticleChunkSystem_Grass_5.Clear();
		ParticleChunkSystem_Bones.Clear();
		ParticleChunkSystem_Stone.Clear();
	}

	private void Start()
	{
		goopFade.gameObject.SetActive(false);
		psychedelicFade.gameObject.SetActive(false);
		impactFrame.gameObject.SetActive(false);
		if (dustCloudParticles != null)
		{
			dustCloudParticlesInitSize = dustCloudParticles.main.startSize;
			dustCloudParticlesInitLifetime = dustCloudParticles.main.startLifetime;
		}
		if (footprintParticles != null)
		{
			footprintParticlesInitSize = footprintParticles.main.startSize;
			footprintParticlesInitLifetime = footprintParticles.main.startLifetime;
		}
		CreatePools();
		getPostProcessingSettings();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(newTimeOfDay));
		newTimeOfDay();
	}

	public void CreatePools()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		foreach (ObjectPoolObject objectPoolObject in ObjectPoolObjects)
		{
			if ((objectPoolObject.PoolingLocation == ObjectPoolObject.PoolLocation.Base && activeScene.name == "Base Biome 1") || objectPoolObject.PoolingLocation == ObjectPoolObject.PoolLocation.Both)
			{
				objectPoolObject.gameObject.CreatePool(objectPoolObject.AmountToPool);
			}
			else if ((objectPoolObject.PoolingLocation == ObjectPoolObject.PoolLocation.Dungeon && activeScene.name != "Base Biome 1") || objectPoolObject.PoolingLocation == ObjectPoolObject.PoolLocation.Both)
			{
				objectPoolObject.gameObject.CreatePool(objectPoolObject.AmountToPool);
			}
		}
		if (activeScene.name != "Base Biome 1" && activeScene.name != "Midas Cave" && activeScene.name != "Sozo Cave Location" && activeScene.name != "Mushroom Research Site" && activeScene.name != "Hub-Shore" && activeScene.name != "Dungeon Ratau Home")
		{
			BloodImpactPrefab.CreatePool(128);
			PlayerHitImpactPrefabDown.CreatePool(6);
			PlayerHitImpactPrefabDownRight.CreatePool(6);
			PlayerHitImpactPrefabUp.CreatePool(6);
			HitImpactPrefab.CreatePool(12);
			PlayerHitImpactPrefab_Poison.CreatePool(3);
			PlayerHitImpactPrefab_Ice.CreatePool(3);
			SmokeExplosionVFX.CreatePool(6);
			HitFXPrefab.CreatePool(16);
			particleChunkPrefab.CreatePool(32);
			PickUpVFX.CreatePool(48);
			HeartPickUpVFX.CreatePool(1);
			HitFX_BlockedRedSmall.CreatePool(128);
			ProgressIndicatorTemplate.CreatePool(3);
		}
		if (activeScene.name == "Base Biome 1")
		{
			HitFXSoulPrefab.CreatePool(12);
			ProgressIndicatorTemplate.CreatePool(20);
		}
	}

	public void getPostProcessingSettings()
	{
		ppv.profile.TryGetSettings<Desaturate_StencilRTMaskPPSSettings>(out desaturateStencil);
		ppv.profile.TryGetSettings<Vignette>(out vignette);
		ppv.profile.TryGetSettings<ChromaticAberration>(out chromaticAbberration);
		ppv.profile.TryGetSettings<Bloom>(out bloom);
		ppv.profile.TryGetSettings<DepthOfField>(out depthoffield);
		ppv.profile.TryGetSettings<AmplifyPostEffect>(out amplifyPostEffect);
		updatePostProcessingSettings();
	}

	public void DepthOfFieldTween(float Duration, float FocusDistance, float aperture, float FocusLengthStart, float FocusLengthEnd)
	{
		if (depthoffield == null)
		{
			getPostProcessingItems();
		}
		if (!(depthoffield == null))
		{
			depthoffield.enabled.value = SettingsManager.Settings.Graphics.DepthOfField;
			depthoffield.focusDistance.value = FocusDistance;
			depthoffield.aperture.value = aperture;
			if (depthOfFieldRoutine != null)
			{
				StopCoroutine(depthOfFieldRoutine);
			}
			depthOfFieldRoutine = StartCoroutine(DepthOfFieldRoutine(Duration, FocusLengthStart, FocusLengthEnd));
		}
	}

	private IEnumerator DepthOfFieldRoutine(float Duration, float FocusLengthStart, float FocusLengthEnd)
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			depthoffield.focalLength.value = Mathf.SmoothStep(FocusLengthStart, FocusLengthEnd, Progress / Duration);
			yield return null;
		}
		depthoffield.focalLength.value = FocusLengthEnd;
	}

	public void ChromaticAbberationTween(float Duration, float StartValue, float EndValue)
	{
		if (chromaticAbberration == null)
		{
			getPostProcessingItems();
		}
		if (!(chromaticAbberration == null))
		{
			chromaticAbberration.enabled.value = SettingsManager.Settings.Graphics.ChromaticAberration;
			StartCoroutine(ChromaticAbberationRoutine(Duration, StartValue, EndValue));
		}
	}

	private IEnumerator ChromaticAbberationRoutine(float Duration, float StartValue, float EndValue)
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			chromaticAbberration.intensity.value = Mathf.SmoothStep(StartValue, EndValue, Progress / Duration);
			yield return null;
		}
		chromaticAbberration.intensity.value = EndValue;
	}

	public void VignetteTween(float Duration, float StartValue, float EndValue)
	{
		if (vignette == null)
		{
			getPostProcessingItems();
		}
		if (!(vignette == null))
		{
			vignette.enabled.value = SettingsManager.Settings.Graphics.Vignette;
			StartCoroutine(VignetteRoutine(Duration, StartValue, EndValue));
		}
	}

	private IEnumerator VignetteRoutine(float Duration, float StartValue, float EndValue)
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			vignette.intensity.value = Mathf.SmoothStep(StartValue, EndValue, Progress / Duration);
			yield return null;
		}
		vignette.intensity.value = EndValue;
	}

	public void SetAmplifyPostEffect(bool enable)
	{
		amplifyPostEffect.enabled.value = enable;
	}

	public void DesaturationStencilTween(float Duration, float StartValue, float EndValue, float BrightnessStart, float BrightnessEnd)
	{
		if (desaturateStencil == null)
		{
			getPostProcessingItems();
		}
		if (!(desaturateStencil == null))
		{
			StopCoroutine(DesaturationStencilRoutine(Duration, StartValue, EndValue, BrightnessStart, BrightnessEnd));
			desaturateStencil.enabled.value = true;
			StartCoroutine(DesaturationStencilRoutine(Duration, StartValue, EndValue, BrightnessStart, BrightnessEnd));
		}
	}

	private IEnumerator DesaturationStencilRoutine(float Duration, float StartValue, float EndValue, float BrightnessStart, float BrightnessEnd)
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			desaturateStencil._Intensity.value = Mathf.SmoothStep(StartValue, EndValue, Progress / Duration);
			desaturateStencil._Brightness.value = Mathf.SmoothStep(BrightnessStart, BrightnessEnd, Progress / Duration);
			yield return null;
		}
		desaturateStencil._Intensity.value = EndValue;
		desaturateStencil._Brightness.value = BrightnessEnd;
		if (desaturateStencil._Intensity.value == 0f)
		{
			desaturateStencil.enabled.value = false;
		}
	}

	public void DesaturationStencil(float norm, float StartValue, float EndValue, float BrightnessStart, float BrightnessEnd)
	{
		if (desaturateStencil == null)
		{
			getPostProcessingItems();
		}
		if (!(desaturateStencil == null))
		{
			desaturateStencil._Intensity.value = Mathf.SmoothStep(StartValue, EndValue, norm);
			desaturateStencil._Brightness.value = Mathf.SmoothStep(BrightnessStart, BrightnessEnd, norm);
			desaturateStencil.enabled.value = desaturateStencil._Intensity.value != 0f;
		}
	}

	public void DisableIndicators()
	{
		foreach (UIProgressIndicator progressIndicator in UIProgressIndicator.ProgressIndicators)
		{
			progressIndicator.Hide(0f, 0f);
		}
	}

	public void getColorBlind()
	{
		if (Singleton<AccessibilityManager>.Instance.ColorblindMode != 0 && Singleton<AccessibilityManager>.Instance.ColorblindMode - 1 < ColorBlindLUT.Count)
		{
			if (amp == null)
			{
				amp = Camera.main.gameObject.AddComponent<AmplifyColorBase>();
			}
			amp.LutTexture = ColorBlindLUT[Singleton<AccessibilityManager>.Instance.ColorblindMode - 1];
			amp.QualityLevel = Quality.Mobile;
		}
		else if (amp != null)
		{
			UnityEngine.Object.Destroy(amp);
		}
	}

	private void OnColourblindModeUpdated()
	{
		getColorBlind();
		updatePostProcessing();
	}

	private void getPostProcessingItems()
	{
		ppv.profile.TryGetSettings<Vignette>(out vignette);
		ppv.profile.TryGetSettings<ChromaticAberration>(out chromaticAbberration);
		ppv.profile.TryGetSettings<Bloom>(out bloom);
		ppv.profile.TryGetSettings<DepthOfField>(out depthoffield);
		ppv.profile.TryGetSettings<AmplifyPostEffect>(out amplifyPostEffect);
		
		ppv.profile.TryGetSettings<Desaturate_StencilRTMaskPPSSettings>(out desaturateStencil);
		updatePostProcessing();
	}

	private void updatePostProcessingSettings()
	{
		if (vignette == null || chromaticAbberration == null || bloom == null || depthoffield == null)
		{
			getPostProcessingItems();
		}
		if (!(vignette == null) && !(chromaticAbberration == null) && !(bloom == null) && !(depthoffield == null))
		{
			Debug.Log("PostProcessingUpdate");
			bloom.enabled.value = SettingsManager.Settings.Graphics.Bloom;
			chromaticAbberration.enabled.value = SettingsManager.Settings.Graphics.ChromaticAberration;
			vignette.enabled.value = SettingsManager.Settings.Graphics.Vignette;
			depthoffield.enabled.value = SettingsManager.Settings.Graphics.DepthOfField;
			_postProcessLayer.antialiasingMode = GraphicsSettingsUtilities.AntiAliasingModeFromBool(SettingsManager.Settings.Graphics.AntiAliasing);
			TrackDepthOfField = false;
		}
		if (amplifyPostEffect != null)
        {
			Debug.Log("AmplifyPostEffect 有问题 关闭");
			amplifyPostEffect.active = false;
        }
	}

	private IEnumerator TrackDepthOfFieldDistance()
	{
		while (depthoffield != null && TrackDepthOfField)
		{
			float value = Vector3.Distance(Camera.main.transform.position, PlayerFarming.Instance.gameObject.transform.position);
			depthoffield.focusDistance.value = value;
			yield return null;
		}
	}

	public void updatePostProcessing()
	{
		updatePostProcessingSettings();
	}

	public void EmitSteppingInWaterParticles(Vector3 worldPos, int numParticles = 3)
	{
		if (steppingInWaterParticles == null)
		{
			if (base.gameObject != null)
			{
				Debug.LogWarning("steppingInWaterParticles property is missing!", base.gameObject);
			}
		}
		else
		{
			emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = worldPos;
			emitParams.applyShapeToPosition = true;
			steppingInWaterParticles.Emit(emitParams, numParticles);
		}
	}

	public void EmitDustCloudParticles(Vector3 worldPos, int numParticles = 3, float multiplier = 1f, bool ignoreTimescale = false)
	{
		if (!dustCloudParticles.gameObject.activeSelf || !(PlayerFarming.Instance != null))
		{
			return;
		}
		if (dustCloudParticles == null)
		{
			if (base.gameObject != null)
			{
				Debug.LogWarning("dustCloudPFX property is missing!", base.gameObject);
			}
			return;
		}
		DustCloudParticlesParams = default(ParticleSystem.EmitParams);
		DustCloudParticlesParams.position = worldPos;
		DustCloudParticlesParams.applyShapeToPosition = true;
		if (multiplier != 1f)
		{
			DustCloudParticlesParams.startSize = UnityEngine.Random.Range(dustCloudParticlesInitSize.constantMin, dustCloudParticlesInitSize.constantMax) * multiplier;
			DustCloudParticlesParams.startLifetime = UnityEngine.Random.Range(dustCloudParticlesInitLifetime.constantMin, dustCloudParticlesInitLifetime.constantMax) * multiplier;
		}
		ParticleSystem.MainModule main = dustCloudParticles.main;
		main.useUnscaledTime = ignoreTimescale;
		dustCloudParticles.Emit(DustCloudParticlesParams, numParticles);
	}

	public void EmitFootprintsParticles(Vector3 worldPos, Color color, float multiplier = 1f)
	{
		if (footprintParticles == null)
		{
			Debug.LogWarning("footprintParticles property is missing!", base.gameObject);
			return;
		}
		emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = new Vector3(worldPos.x, worldPos.y, -0.001f);
		emitParams.applyShapeToPosition = true;
		emitParams.startColor = color;
		if (multiplier != 1f)
		{
			emitParams.startSize = footprintParticlesInitSize.constant * multiplier;
		}
		footprintParticles.Emit(emitParams, 1);
	}

	public void EmitHitImpactEffect(Vector3 Position, float Angle, bool useDeltaTime = true)
	{
		SimpleVFX component = HitImpactPrefab.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		component.gameObject.SetActive(true);
		if (component.Spine != null)
		{
			component.Spine.UseDeltaTime = useDeltaTime;
			SkeletonRenderer component2 = component.Spine.GetComponent<SkeletonRenderer>();
			Slot slot = component2.Skeleton.FindSlot("SlashPlayer_0");
			Slot slot2 = component2.Skeleton.FindSlot("Swipe_1");
			Slot slot3 = component2.Skeleton.FindSlot("Swipe_2");
			Slot slot4 = component2.Skeleton.FindSlot("Swipe 3");
			slot.SetColor(StaticColors.GreenColor);
			slot2.SetColor(StaticColors.GreenColor);
			slot3.SetColor(StaticColors.GreenColor);
			slot4.SetColor(StaticColors.GreenColor);
		}
		component.Play(Position, Angle);
	}

	public void PlayerEmitHitImpactEffect(Vector3 Position, float Angle, bool useDeltaTime = true, Color color = default(Color), float scale = 1f, bool crit = false)
	{
		SimpleParticleVFX simpleParticleVFX = null;
		if (EquipmentManager.IsPoisonWeapon(PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.EquipmentType))
		{
			simpleParticleVFX = PlayerHitImpactPrefab_Poison.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity);
		}
		else if (DataManager.Instance.CurrentCurse == EquipmentType.Tentacles_Ice || DataManager.Instance.CurrentCurse == EquipmentType.EnemyBlast_Ice || DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Ice)
		{
			simpleParticleVFX = PlayerHitImpactPrefab_Ice.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity);
		}
		else
		{
			switch (PlayerWeapon.AttackSwipeDirection)
			{
			case PlayerWeapon.AttackSwipeDirections.Down:
				if (PlayerHitImpactPrefabDown != null)
				{
					simpleParticleVFX = PlayerHitImpactPrefabDown.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity);
				}
				break;
			case PlayerWeapon.AttackSwipeDirections.DownRight:
				if (PlayerHitImpactPrefabDownRight != null)
				{
					simpleParticleVFX = PlayerHitImpactPrefabDownRight.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity);
				}
				break;
			case PlayerWeapon.AttackSwipeDirections.Up:
				if (PlayerHitImpactPrefabUp != null)
				{
					simpleParticleVFX = PlayerHitImpactPrefabUp.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity);
				}
				break;
			}
		}
		if (simpleParticleVFX == null)
		{
			Debug.Log("Oh no! vfx was not set correctly! " + PlayerWeapon.AttackSwipeDirection);
			return;
		}
		simpleParticleVFX.transform.localScale = Vector3.one * scale;
		if (simpleParticleVFX.particlesToPlay[0] != null)
		{
			ParticleSystem.MainModule main = simpleParticleVFX.particlesToPlay[0].main;
			main.useUnscaledTime = useDeltaTime;
		}
		Color red = Color.red;
		simpleParticleVFX.gameObject.SetActive(true);
		simpleParticleVFX.Play(Position, Angle);
		if (crit)
		{
			PlayerEmitHitCriticalImpactEffect(Position, Angle, useDeltaTime, color, 0.5f);
			AudioManager.Instance.PlayOneShot("event:/weapon/crit_hit", Position);
		}
	}

	public void PlayerEmitHitCriticalImpactEffect(Vector3 Position, float Angle, bool useDeltaTime = true, Color color = default(Color), float scale = 1f)
	{
		SimpleVFX simpleVFX = null;
		simpleVFX = PlayerHitCritical.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity);
		if (simpleVFX == null)
		{
			Debug.Log("Oh no! vfx was not set correctly! " + PlayerWeapon.AttackSwipeDirection);
			return;
		}
		simpleVFX.transform.localScale = Vector3.one * scale;
		if (simpleVFX.particlesToPlay[0] != null)
		{
			ParticleSystem.MainModule main = simpleVFX.particlesToPlay[0].main;
			main.useUnscaledTime = useDeltaTime;
		}
		simpleVFX.gameObject.SetActive(true);
		simpleVFX.Play(Position, Angle);
	}

	public BlackSoul SpawnBlackSouls(Vector3 Position, Transform Layer, float Angle, bool simulated = false)
	{
		BlackSoul component = blackSoulPrefab.Spawn(Layer, Position, Quaternion.identity).GetComponent<BlackSoul>();
		component.Simulated = simulated;
		component.gameObject.SetActive(true);
		component.SetAngle(Angle);
		return component;
	}

	public void EmitBloodSplatter(Vector3 hitPos, Vector3 direction, Color color)
	{
	}

	public void EmitBloodDieEffect(Vector3 hitPos, Vector3 direction, Color color)
	{
	}

	public void EmitBloodSplatterGroundParticles(Vector3 worldPos, Vector3 Velocity, Color color, int numParticles = 1, float multiplier = 1f)
	{
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = worldPos;
		emitParams.applyShapeToPosition = true;
		emitParams.startColor = color;
		int count = UnityEngine.Random.Range(1, 3);
		if (BloodSplatterPrefab != null)
		{
			BloodSplatterPrefab.GetComponent<ParticleSystem>().Emit(emitParams, count);
			BloodSplatterPrefab.GetComponent<BloodSplatterPrefab>().NewParticle();
		}
	}

	public void EmitBloodImpact(Vector3 Position, float Angle, string skin, string animation = null, bool useDeltaTime = true)
	{
		SimpleVFX simpleVFX = BloodImpactPrefab.Spawn(null, Position, Quaternion.identity);
		if (simpleVFX.Spine != null)
		{
			simpleVFX.Spine.UseDeltaTime = useDeltaTime;
			
		}
		if (animation == null)
		{
			simpleVFX.Play(Position, Angle);
		}
		else
		{
			simpleVFX.Play(Position, Angle, animation);
		}
		if (skin == null)
		{
			skin = "black";
		}
		simpleVFX.Spine.skeleton.SetSkin(skin);
	}

	public void EmitHitFX_BlockedRedSmall(Vector3 Position, Quaternion Angle, Vector3 scale)
	{
		GameObject gameObject = HitFX_BlockedRedSmall.Spawn(ObjectPoolParent.transform, Position, Angle);
		if (scale != Vector3.one)
		{
			gameObject.transform.localScale = scale;
		}
	}

	public void EmitHeartPickUpVFX(Vector3 Position, float Angle, string skin, string animationName, bool useDeltaTime = true)
	{
		SimpleVFX component = HeartPickUpVFX.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		component.gameObject.SetActive(true);
		component.Play(Position, Angle, animationName);
		component.Spine.skeleton.SetSkin(skin);
		component.Spine.UseDeltaTime = useDeltaTime;
	}

	public void EmitBlockImpact(Vector3 Position, float Angle, Transform parentTransform = null, string animation = null)
	{
		SimpleVFX component = BlockImpactShield.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		SimpleVFX component2 = BlockImpact.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		if (animation != null)
		{
			component2.Play(Position, Angle);
			component.Play(Position, 0f, animation);
		}
		else
		{
			component2.Play(Position, Angle);
			component.Play(Position);
		}
		if (parentTransform != null)
		{
			component.gameObject.transform.SetParent(parentTransform);
			component2.gameObject.transform.SetParent(parentTransform);
		}
	}

	public void EmitProtectorImpact(Vector3 Position, float Angle)
	{
		SimpleVFX component = ProtectorImpactIcon.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		BlockImpact.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>().Play(Position, Angle);
		component.Play(Position);
	}

	public void EmitHitVFXSoul(Vector3 Position, Quaternion Angle)
	{
		HitFXSoulPrefab.Spawn(ObjectPoolParent.transform, Position, Angle).GetComponent<ParticleSystem>().Play();
	}

	public void EmitHitVFX(Vector3 Position, float Angle, string animationName)
	{
		HitFXPrefab.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).Play(Position, Angle, animationName);
	}

	public void EmitRubbleHitFXParticles(Vector3 worldPos, float multiplier = 1f)
	{
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = worldPos;
		EmitRubbleHitFX.Emit(emitParams, 1);
	}

	public void EmitGroundSmashVFXParticles(Vector3 worldPos, float multiplier = 1f)
	{
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		if (multiplier != 1f)
		{
			emitParams.startSize = UnityEngine.Random.Range(EmitGroundSmashVFXInitSize.constantMin, EmitGroundSmashVFXInitSize.constantMax) * multiplier;
		}
		emitParams.position = worldPos;
		emitParams.applyShapeToPosition = true;
		EmitGroundSmashVFX.Emit(emitParams, 1);
	}

	public void EmitDeadBodyExplodeVFX(Vector3 worldPos)
	{
		if (!(DeadBodyExplodeVFX == null))
		{
			DeadBodyExplodeVFX.transform.position = worldPos;
			DeadBodyExplodeVFX.Play();
		}
	}

	public Particle_Chunk SpawnParticleChunk(Vector3 position)
	{
		return particleChunkPrefab.Spawn(ObjectPoolParent.transform, position, Quaternion.identity);
	}

	public void EmitGhostTwirlAttack(Vector3 Position, float Angle)
	{
		Debug.Log("EmitGhostTwirlAttack");
		GhostTwirlAttack.gameObject.SetActive(true);
		Vector3 vector = new Vector3(1.5f * Mathf.Cos(Angle * ((float)Math.PI / 180f)), 1.5f * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
		GhostTwirlAttack.Play(Position + vector);
		SlamVFX.gameObject.SetActive(true);
		SlamVFX.Play(Position + vector);
		craterVFX.Play(Position + vector);
		groundParticles.Play(Position + vector);
		CameraManager.shakeCamera(2f);
	}

	public void EmitHammerEffects(Vector3 Position, float Angle)
	{
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.3f);
		Vector3 vector = new Vector3(1.5f * Mathf.Cos(Angle * ((float)Math.PI / 180f)), 1.5f * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
		SlamVFX.gameObject.SetActive(true);
		SlamVFX.Play(Position + vector);
		craterVFX.Play(Position + vector);
		groundParticles.Play(Position + vector);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_burst_out_of_ground", Position + vector);
	}

	public void ShakeCamera()
	{
		CameraManager.shakeCamera(2f);
	}

	public void EmitSlamVFX(Vector3 Position)
	{
		SlamVFX.Play(Position);
	}

	public void EmitPickUpVFX(Vector3 Position, string animation = null)
	{
		SimpleVFX component = PickUpVFX.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		component.gameObject.SetActive(true);
		if (animation == null)
		{
			component.Play(Position);
		}
		else
		{
			component.Play(Position, 0f, animation);
		}
	}

	public void EmitSmokeExplosionVFX(Vector3 Position)
	{
		SimpleVFX component = SmokeExplosionVFX.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		component.gameObject.SetActive(true);
		component.Play(Position);
	}

	public void EmitSmokeExplosionVFX(Vector3 Position, Color color)
	{
		SimpleVFX component = SmokeExplosionVFX.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		component.gameObject.SetActive(true);
		foreach (ParticleSystem item in component.particlesToPlay)
		{
			ParticleSystem.MainModule main = item.main;
			main.startColor = color;
		}
		component.Play(Position);
	}

	public void EmitSmokeInteractionVFX(Vector3 position, Vector3 scale)
	{
		Debug.DrawLine(position - scale * 0.5f, position + scale * 0.5f, Color.magenta, 5f);
		interactionSmokeVFX.transform.localScale = scale;
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = position;
		emitParams.applyShapeToPosition = true;
		interactionSmokeVFX.Emit(emitParams, (int)(Mathf.Max(scale.x, scale.y) * 5f));
	}

	public void EmitConfettiVFX(Vector3 Position)
	{
		SimpleVFX component = ConfettiVFX.Spawn(ObjectPoolParent.transform, Position, Quaternion.identity).GetComponent<SimpleVFX>();
		component.gameObject.SetActive(true);
		component.Play(Position);
	}

	public void EmitParticleChunk(TypeOfParticle type, Vector3 worldPos, Vector3 Velocity, int numParticles = 3, float multiplier = 1f)
	{
		Vector3 velocity = new Vector3(Velocity.x / 4f, Velocity.y / 4f, -2f) + new Vector3(UnityEngine.Random.Range(0f - RandomVariation, RandomVariation), UnityEngine.Random.Range(0f - RandomVariation, RandomVariation), UnityEngine.Random.Range(0f - RandomVariation, RandomVariation) * 2f);
		emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = worldPos;
		emitParams.applyShapeToPosition = true;
		emitParams.velocity = velocity;
		switch (type)
		{
		case TypeOfParticle.wood:
			ParticleChunkSystem_Wood.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.clay:
			ParticleChunkSystem_Clay.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.grass:
			ParticleChunkSystem_Grass.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.grass_2:
			ParticleChunkSystem_Grass_2.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.grass_3:
			ParticleChunkSystem_Grass_3.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.grass_4:
			ParticleChunkSystem_Grass_4.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.grass_5:
			ParticleChunkSystem_Grass_5.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.bone:
			ParticleChunkSystem_Bones.Emit(emitParams, numParticles);
			break;
		case TypeOfParticle.stone:
			ParticleChunkSystem_Stone.Emit(emitParams, numParticles);
			break;
		}
	}

	public void newTimeOfDay()
	{
		fireflies.gameObject.SetActive(false);
		DayPhase currentPhase = TimeManager.CurrentPhase;
		if (currentPhase == DayPhase.Night)
		{
			fireflies.gameObject.SetActive(true);
		}
	}

	private IEnumerator LerpTimeOfDay(Color StartColor, Color endColor, float Intensity)
	{
		endColor.a = Intensity;
		for (float timer = 0f; timer <= lerpTime; timer += 0.005f)
		{
			Shader.SetGlobalColor("_TimeOfDayColor", Color.Lerp(StartColor, endColor, timer));
			yield return null;
		}
	}

	private void ObjectiveManager_OnObjectiveCompleted(ObjectivesData objective)
	{
		if (objective.Type == Objectives.TYPES.NO_CURSES || objective.Type == Objectives.TYPES.NO_DAMAGE || objective.Type == Objectives.TYPES.NO_DODGE)
		{
			UnityEngine.Object.Instantiate(DungeonChallengeRatoo, (UnityEngine.Random.value > 0.5f) ? (Vector3.right * 1.5f) : (Vector3.right * -1.5f), Quaternion.identity, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		}
	}

	public void ShowBlackHeartDamage(Transform parent, Vector3 offset)
	{
		GameObject icon = UnityEngine.Object.Instantiate(Instance.BlackHeartDamageIcon, base.transform.position, Quaternion.identity);
		icon.transform.parent = parent;
		icon.transform.localPosition = Vector3.back + offset;
		icon.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f);
		GameManager.GetInstance().StartCoroutine(DelayedCallback(0.75f, delegate
		{
			if ((bool)icon)
			{
				icon.transform.DOLocalMoveZ(0f, 0.25f);
				icon.transform.DOScale(0f, 0.25f);
			}
			GameManager.GetInstance().StartCoroutine(DelayedCallback(0.25f, delegate
			{
				if ((bool)icon)
				{
					UnityEngine.Object.Destroy(icon.gameObject);
				}
			}));
		}));
	}

	public void ShowTarotCardDamage(Transform parent, Vector3 offset)
	{
		GameObject icon = UnityEngine.Object.Instantiate(Instance.TarotCardDamageIcon, base.transform.position, Quaternion.identity);
		icon.transform.parent = parent;
		icon.transform.localPosition = Vector3.back + offset;
		icon.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f);
		GameManager.GetInstance().StartCoroutine(DelayedCallback(0.75f, delegate
		{
			if ((bool)icon)
			{
				icon.transform.DOLocalMoveZ(0f, 0.25f);
				icon.transform.DOScale(0f, 0.25f);
			}
			GameManager.GetInstance().StartCoroutine(DelayedCallback(0.25f, delegate
			{
				if ((bool)icon)
				{
					UnityEngine.Object.Destroy(icon.gameObject);
				}
			}));
		}));
	}

	public void ShowDamageTextIcon(Transform parent, Vector3 offset, float damage)
	{
		GameObject icon = UnityEngine.Object.Instantiate(Instance.DamageTextIcon, base.transform.position, Quaternion.identity);
		icon.transform.parent = parent;
		icon.transform.localPosition = Vector3.back + offset;
		icon.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f);
		icon.GetComponentInChildren<TMP_Text>().text = damage.ToString();
		GameManager.GetInstance().StartCoroutine(DelayedCallback(0.75f, delegate
		{
			if ((bool)icon)
			{
				icon.transform.DOLocalMoveZ(0f, 0.25f);
				icon.transform.DOScale(0f, 0.25f);
			}
			GameManager.GetInstance().StartCoroutine(DelayedCallback(0.25f, delegate
			{
				if ((bool)icon)
				{
					UnityEngine.Object.Destroy(icon.gameObject);
				}
			}));
		}));
	}

	private IEnumerator DelayedCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}
}
