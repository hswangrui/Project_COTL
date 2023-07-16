using UnityEngine;
using UnityEngine.UI;

public abstract class MaterialImagePropertyController : MaterialPropertyController<Image>
{
	protected override void SetupSource()
	{
		if (!Application.isPlaying)
		{
			_material = _sourceComponent.materialForRendering;
		}
		else
		{
			_material = new Material(_sourceComponent.material);
			_sourceComponent.material = _material;
		}
		UpdateMaterialProperties();
	}
}
