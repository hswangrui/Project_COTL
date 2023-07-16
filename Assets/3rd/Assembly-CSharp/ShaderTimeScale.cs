using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ShaderTimeScale : BaseMonoBehaviour
{
	public float TimeStep = 1f;

	private float timer;

	private MeshRenderer _rend
	{
		get
		{
			return GetComponent<MeshRenderer>();
		}
	}

	private void Update()
	{
		timer += TimeStep * Time.deltaTime;
		Material[] materials = _rend.materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetFloat("_shaderTime", timer);
		}
	}
}
