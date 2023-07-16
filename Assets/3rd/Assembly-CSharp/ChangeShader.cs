using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ChangeShader : BaseMonoBehaviour
{
	public Shader shader;

	public SpriteRenderer _spriteRenderer;

	private void Start()
	{
	}

	private void UpdateShader()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_spriteRenderer.sharedMaterial.shader = shader;
	}

	private void Update()
	{
	}
}
