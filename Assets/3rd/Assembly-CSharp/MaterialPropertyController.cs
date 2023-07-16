using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-1000)]
public abstract class MaterialPropertyController<T> : MonoBehaviour
{
	[SerializeField]
	protected T _sourceComponent;

	protected Material _material;

	private void Awake()
	{
		SetupSource();
	}

	private void OnValidate()
	{
		SetupSource();
	}

	protected abstract void SetupSource();

	protected abstract void UpdateMaterialProperties();

	private void Update()
	{
		if (_material != null)
		{
			UpdateMaterialProperties();
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			Object.Destroy(_material);
			_material = null;
		}
	}
}
