using UnityEngine;

public class copyMaterialChangeVariable : BaseMonoBehaviour
{
	public Renderer myRenderer;

	public Material material;

	public string floatToChange;

	public float floatVar;

	private void Start()
	{
		Material material = new Material(this.material);
		myRenderer.material = material;
		myRenderer.material.SetFloat(floatToChange, floatVar);
	}
}
