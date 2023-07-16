using System;
using LeTai.Asset.TranslucentImage;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PerformanceTest : MonoBehaviour
{
	public static bool ReduceGUI;

	public static bool ReduceCPU;

	public static bool ReduceOnRender;

	public PostProcessVolume CurrentPostProcessVolume;

	private Vignette vignette;

	private ChromaticAberration chromaticAbberration;

	private Bloom bloom;

	private ColorGrading colorGrading;

	private VFXImpactFramePPSSettings vFXImpactFramePPSSettings;

	private ImpactFrameBlackPPSSettings impactFrameBlackPPSSettings;

	private Grain grain;

	private AmplifyPostEffect amp;

	public GameObject OptionTemplate;

	public Button DefaultSelect;

	private UINavigator _uiNavigator;

	private void AddToggle(string name, Action<TMP_Text> onToggle, Action<TMP_Text> onUpdate)
	{
		GameObject obj = UnityEngine.Object.Instantiate(OptionTemplate, OptionTemplate.transform.parent);
		obj.transform.SetAsLastSibling();
		obj.transform.GetChild(0).GetComponent<Text>().text = name;
		obj.name = name;
		Button componentInChildren = obj.transform.GetComponentInChildren<Button>();
		TMP_Text label = componentInChildren.transform.GetChild(0).GetComponent<TMP_Text>();
		onUpdate(label);
		Buttons buttons2 = new Buttons();
		buttons2.Button = componentInChildren.gameObject;
		buttons2.Index = 1;
		_uiNavigator.Buttons.Add(buttons2);
		componentInChildren.onClick.AddListener(delegate
		{
			onToggle(label);
			onUpdate(label);
		});
	}

	private void OnEnable()
	{
		_uiNavigator = OptionTemplate.transform.parent.GetComponent<UINavigator>();
		CurrentPostProcessVolume = (PostProcessVolume)UnityEngine.Object.FindObjectOfType(typeof(PostProcessVolume));
		PostProcessLayer ppLayer = (PostProcessLayer)UnityEngine.Object.FindObjectOfType(typeof(PostProcessLayer));
		AddToggle("Shadows Enabled", delegate
		{
			QualitySettings.shadows = ((QualitySettings.shadows == ShadowQuality.Disable) ? ShadowQuality.All : ShadowQuality.Disable);
		}, delegate(TMP_Text label)
		{
			label.text = ((QualitySettings.shadows == ShadowQuality.Disable) ? "O" : "I");
		});
		AddToggle("Depth Texture", delegate
		{
			Camera.main.depthTextureMode = ((Camera.main.depthTextureMode == DepthTextureMode.None) ? DepthTextureMode.Depth : DepthTextureMode.None);
		}, delegate(TMP_Text label)
		{
			label.text = ((Camera.main.depthTextureMode == DepthTextureMode.None) ? "O" : "I");
		});
		AddToggle("Reduce CPU", delegate
		{
			ReduceCPU = !ReduceCPU;
		//	SkeletonAnimation.StopUpdates = (SkeletonGraphic.StopUpdates = ReduceCPU);
		}, delegate(TMP_Text label)
		{
			label.text = (ReduceCPU ? "I" : "O");
		});
		AddToggle("Reduce GUI", delegate
		{
			ReduceGUI = !ReduceGUI;
		}, delegate(TMP_Text label)
		{
			label.text = (ReduceGUI ? "I" : "O");
		});
		AddToggle("Reduce Render", delegate
		{
			StencilLighting_MaskEffect.DisableRender = (TranslucentImageSource.DisableRender = !StencilLighting_MaskEffect.DisableRender);
		}, delegate(TMP_Text label)
		{
			label.text = (StencilLighting_MaskEffect.DisableRender ? "I" : "O");
		});
		AddToggle("Post Proc", delegate
		{
			ppLayer.enabled = !ppLayer.enabled;
		}, delegate(TMP_Text label)
		{
			label.text = (ppLayer.isActiveAndEnabled ? "I" : "O");
		});
		AddToggle("Vsync", delegate
		{
			QualitySettings.vSyncCount ^= 1;
		}, delegate(TMP_Text label)
		{
			label.text = ((QualitySettings.vSyncCount == 0) ? "O" : "I");
		});
		AddToggle("No Lights", delegate
		{
			Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
			foreach (Light light in array)
			{
				if (light.tag != null && !light.tag.StartsWith("Main"))
				{
					light.enabled = false;
				}
			}
		}, delegate(TMP_Text label)
		{
			label.text = " ";
		});
		StencilLighting_DecalSprite[] StencilLighting_DecalSprites = UnityEngine.Object.FindObjectsOfType<StencilLighting_DecalSprite>();
		GameObject obj = UnityEngine.Object.Instantiate(OptionTemplate, OptionTemplate.transform.parent);
		obj.transform.SetAsLastSibling();
		obj.transform.GetChild(0).GetComponent<Text>().text = "StencilLighting_DecalSprite";
		obj.name = "StencilLighting_DecalSprite";
		Button componentInChildren = obj.transform.GetComponentInChildren<Button>();
		TMP_Text StencilLighting_DecalSpriteButtonlabel = componentInChildren.transform.GetChild(0).GetComponent<TMP_Text>();
		if (StencilLighting_DecalSprites[0].enabled)
		{
			StencilLighting_DecalSpriteButtonlabel.text = "I";
		}
		else
		{
			StencilLighting_DecalSpriteButtonlabel.text = "O";
		}
		Buttons buttons2 = new Buttons();
		buttons2.Button = componentInChildren.gameObject;
		buttons2.Index = 2;
		_uiNavigator.Buttons.Add(buttons2);
		componentInChildren.onClick.AddListener(delegate
		{
			for (int k = 0; k < StencilLighting_DecalSprites.Length; k++)
			{
				StencilLighting_DecalSprites[k].enabled = !StencilLighting_DecalSprites[k].enabled;
				StencilLighting_DecalSprites[k].gameObject.SetActive(StencilLighting_DecalSprites[k].enabled);
			}
			if (StencilLighting_DecalSprites[0].enabled)
			{
				StencilLighting_DecalSpriteButtonlabel.text = "I";
			}
			else
			{
				StencilLighting_DecalSpriteButtonlabel.text = "O";
			}
		});
		SpriteShapeController[] spriteShapeControllers = UnityEngine.Object.FindObjectsOfType<SpriteShapeController>();
		GameObject obj2 = UnityEngine.Object.Instantiate(OptionTemplate, OptionTemplate.transform.parent);
		obj2.transform.SetAsLastSibling();
		obj2.transform.GetChild(0).GetComponent<Text>().text = "spriteShapeController";
		obj2.name = "spriteShapeController";
		componentInChildren = obj2.transform.GetComponentInChildren<Button>();
		TMP_Text spriteShapeControllerButtonlabel = componentInChildren.transform.GetChild(0).GetComponent<TMP_Text>();
		if (spriteShapeControllers[0].enabled)
		{
			spriteShapeControllerButtonlabel.text = "I";
		}
		else
		{
			spriteShapeControllerButtonlabel.text = "O";
		}
		Buttons buttons3 = new Buttons();
		buttons3.Button = componentInChildren.gameObject;
		buttons3.Index = 3;
		_uiNavigator.Buttons.Add(buttons3);
		componentInChildren.onClick.AddListener(delegate
		{
			for (int j = 0; j < spriteShapeControllers.Length; j++)
			{
				spriteShapeControllers[j].enabled = !spriteShapeControllers[j].enabled;
				spriteShapeControllers[j].gameObject.SetActive(spriteShapeControllers[j].enabled);
			}
			if (StencilLighting_DecalSprites[0].enabled)
			{
				spriteShapeControllerButtonlabel.text = "I";
			}
			else
			{
				spriteShapeControllerButtonlabel.text = "O";
			}
		});
		getPostProcessingItems();
		for (int i = 0; i < CurrentPostProcessVolume.profile.settings.Count; i++)
		{
			GameObject obj3 = UnityEngine.Object.Instantiate(OptionTemplate, OptionTemplate.transform.parent);
			obj3.transform.SetAsLastSibling();
			obj3.name = CurrentPostProcessVolume.profile.settings[i].GetType().ToString();
			obj3.transform.GetChild(0).GetComponent<Text>().text = CurrentPostProcessVolume.profile.settings[i].GetType().ToString().Replace("UnityEngine.Rendering.PostProcessing.", "");
			componentInChildren = obj3.transform.GetComponentInChildren<Button>();
			int index = i;
			Buttons buttons4 = new Buttons();
			buttons4.Button = componentInChildren.gameObject;
			buttons4.Index = 3 + i;
			_uiNavigator.Buttons.Add(buttons4);
			Type t = CurrentPostProcessVolume.profile.settings[index].GetType();
			TMP_Text buttonlabel = componentInChildren.transform.GetChild(0).GetComponent<TMP_Text>();
			if (CurrentPostProcessVolume.profile.settings[i].active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			Debug.Log(t.ToString());
			componentInChildren.onClick.AddListener(delegate
			{
				postprocessingpress(t, buttonlabel);
			});
		}
		DefaultSelect.Select();
		OptionTemplate.SetActive(false);
	}

	private void postprocessingpress(Type t, TMP_Text buttonlabel)
	{
		if (t == typeof(Vignette))
		{
			if (vignette.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			vignette.active = !vignette.active;
		}
		else if (t == typeof(ChromaticAberration))
		{
			if (chromaticAbberration.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			chromaticAbberration.active = !chromaticAbberration.active;
		}
		else if (t == typeof(Bloom))
		{
			if (bloom.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			bloom.active = !bloom.active;
		}
		else if (t == typeof(ColorGrading))
		{
			if (colorGrading.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			colorGrading.active = !colorGrading.active;
		}
		else if (t == typeof(VFXImpactFramePPSSettings))
		{
			if (vFXImpactFramePPSSettings.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			vFXImpactFramePPSSettings.active = !vFXImpactFramePPSSettings.active;
		}
		else if (t == typeof(ImpactFrameBlackPPSSettings))
		{
			if (impactFrameBlackPPSSettings.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			impactFrameBlackPPSSettings.active = !impactFrameBlackPPSSettings.active;
		}
		else if (t == typeof(Grain))
		{
			if (grain.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			grain.active = !grain.active;
		}
		else if (t == typeof(AmplifyPostEffect))
		{
			if (amp.active)
			{
				buttonlabel.text = "I";
			}
			else
			{
				buttonlabel.text = "O";
			}
			amp.active = !amp.active;
		}
	}

	private void getPostProcessingItems()
	{
		CurrentPostProcessVolume.profile.TryGetSettings<Vignette>(out vignette);
		CurrentPostProcessVolume.profile.TryGetSettings<ChromaticAberration>(out chromaticAbberration);
		CurrentPostProcessVolume.profile.TryGetSettings<Bloom>(out bloom);
		CurrentPostProcessVolume.profile.TryGetSettings<Grain>(out grain);
		CurrentPostProcessVolume.profile.TryGetSettings<ImpactFrameBlackPPSSettings>(out impactFrameBlackPPSSettings);
		CurrentPostProcessVolume.profile.TryGetSettings<VFXImpactFramePPSSettings>(out vFXImpactFramePPSSettings);
		CurrentPostProcessVolume.profile.TryGetSettings<AmplifyPostEffect>(out amp);
	}

	public void Close()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
