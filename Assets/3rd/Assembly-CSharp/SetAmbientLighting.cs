using UnityEngine;

public class SetAmbientLighting : BaseMonoBehaviour
{
	public Color color;

	private void Start()
	{
		RenderSettings.ambientLight = color;
	}

	private void Update()
	{
	}
}
