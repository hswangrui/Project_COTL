using TMPro;
using UnityEngine;

public class TextMeshProUGUIShaderAnimation : BaseMonoBehaviour
{
	public TextMeshProUGUI _tmPro;

	public Material _material;

	public float _lastFaceDilate;

	public float _faceDilate;

	private void Start()
	{
		_lastFaceDilate = -2f;
		_tmPro = GetComponent<TextMeshProUGUI>();
		_material = new Material(_tmPro.fontSharedMaterial);
		_tmPro.fontMaterial = _material;
		_material.SetFloat(ShaderUtilities.ID_FaceDilate, -1f);
	}

	private void OnEnable()
	{
		_tmPro = GetComponent<TextMeshProUGUI>();
		if (_tmPro.fontMaterial != _material)
		{
			_tmPro.fontMaterial = _material;
		}
		_lastFaceDilate = -1f;
		if (_material == null)
		{
			_material = new Material(_tmPro.fontSharedMaterial);
		}
		_material.SetFloat(ShaderUtilities.ID_FaceDilate, -1f);
		_tmPro.ForceMeshUpdate();
	}

	private void Update()
	{
		if (_tmPro.fontMaterial != _material)
		{
			_tmPro.fontMaterial = _material;
		}
		if (_material == null)
		{
			_material = new Material(_tmPro.fontSharedMaterial);
			_tmPro.ForceMeshUpdate();
			_tmPro.fontMaterial = _material;
		}
		if (!(_material == null) && _material.HasProperty(ShaderUtilities.ID_FaceDilate) && _faceDilate != _lastFaceDilate)
		{
			if (_tmPro.fontMaterial != _material)
			{
				_tmPro.fontMaterial = _material;
			}
			_lastFaceDilate = _faceDilate;
			_material.SetFloat(ShaderUtilities.ID_FaceDilate, _faceDilate);
		}
	}
}
