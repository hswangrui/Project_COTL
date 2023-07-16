using UnityEngine;

public class TranslateShaderOffset : MonoBehaviour
{
	public string propertyName = "_MainTex";

	public Vector2 velocity;

	private Vector2 offset;

	private void Start()
	{
		offset = GetComponent<Renderer>().material.GetTextureOffset(propertyName);
	}

	private void Update()
	{
		offset += velocity * Time.deltaTime;
		GetComponent<Renderer>().material.SetTextureOffset(propertyName, offset);
	}
}
