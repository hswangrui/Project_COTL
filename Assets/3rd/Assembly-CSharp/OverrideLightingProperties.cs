using System;
using UnityEngine;

[Serializable]
public class OverrideLightingProperties
{
	public bool UnscaledTime;

	public bool Enabled;

	[Header("Ambient")]
	public bool AmbientColor;

	[Header("Directional Light")]
	public bool DirectionalLightColor;

	public bool DirectionalLightIntensity;

	public bool ShadowStrength;

	public bool LightRotation;

	[Header("Lighting Ramp Colours")]
	public bool GlobalHighlightColor;

	public bool GlobalShadowColor;

	[Header("StencilLighting LUT Effects")]
	public bool LUTTextureShadow;

	public bool LUTTextureLit;

	public bool StencilInfluence;

	public bool Exposure;

	[Header("Fog")]
	public bool FogColor;

	public bool FogDist;

	public bool FogHeight;

	public bool FogSpread;

	[Header("God Rays Color")]
	public bool GodRayColor;

	[Header("Screenspace Shadows")]
	public bool ScreenSpaceOverlayMat;
}
