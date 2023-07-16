using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ChangeSharedShader : BaseMonoBehaviour
{
	public Shader shader;

	public SpriteRenderer _spriteRenderer;

	private void UpdateShader()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_spriteRenderer.sharedMaterial.shader = shader;
	}
}
