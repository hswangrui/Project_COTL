using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class VFXMaterialOverride
{
	public string _materialPropertyName = "_MainColor";

	[SerializeField]
	private ShaderPropertyType _type;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color _color;

	[SerializeField]
	private Texture _texture;

	[SerializeField]
	private float _float;

	[SerializeField]
	private Vector4 _vector;

	[SerializeField]
	private List<Renderer> _renderers;

	public VFXMaterialOverride()
	{
	}

	public VFXMaterialOverride(string materialPropertyName, List<Renderer> renderers, Color color)
	{
		_materialPropertyName = materialPropertyName;
		_renderers = renderers;
		_type = ShaderPropertyType.Color;
		_color = color;
	}

	public VFXMaterialOverride(string materialPropertyName, List<Renderer> renderers, Texture texture)
	{
		_materialPropertyName = materialPropertyName;
		_renderers = renderers;
		_type = ShaderPropertyType.Texture;
		_texture = texture;
	}

	public VFXMaterialOverride(string materialPropertyName, List<Renderer> renderers, float f)
	{
		_materialPropertyName = materialPropertyName;
		_renderers = renderers;
		_type = ShaderPropertyType.Float;
		_float = f;
	}

	public void Apply(ref MaterialPropertyBlock propertyBlock)
	{
		switch (_type)
		{
		case ShaderPropertyType.Color:
			propertyBlock.SetColor(_materialPropertyName, _color);
			break;
		case ShaderPropertyType.Texture:
			propertyBlock.SetTexture(_materialPropertyName, _texture);
			break;
		case ShaderPropertyType.Float:
		case ShaderPropertyType.Range:
			propertyBlock.SetFloat(_materialPropertyName, _float);
			break;
		case ShaderPropertyType.Vector:
			propertyBlock.SetVector(_materialPropertyName, _vector);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		foreach (Renderer renderer in _renderers)
		{
			renderer.SetPropertyBlock(propertyBlock);
		}
	}
}
