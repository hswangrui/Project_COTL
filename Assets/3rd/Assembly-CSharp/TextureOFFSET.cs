using UnityEngine;

public class TextureOFFSET : BaseMonoBehaviour
{
	public float scrollSpeed = 0.5f;

	private Renderer rend;

	private void Start()
	{
		rend = GetComponent<Renderer>();
	}

	private void Update()
	{
		float x = Time.time * scrollSpeed;
		rend.material.mainTextureOffset = new Vector2(x, 0f);
	}
}
