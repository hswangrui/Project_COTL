using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StencilLighting_ExcludeSprite : BaseMonoBehaviour
{
	[Range(0f, 1f)]
	public float ExclusionAmount = 0.7f;

	private GameObject renderObject;

	private SpriteRenderer masterRenderer;

	private SpriteRenderer targetRenderer;

	private MaterialPropertyBlock m_propBlock;

	public MaterialPropertyBlock PropBlock
	{
		get
		{
			if (m_propBlock == null)
			{
				m_propBlock = new MaterialPropertyBlock();
			}
			return m_propBlock;
		}
	}

	private void Start()
	{
		masterRenderer = GetComponent<SpriteRenderer>();
		Material material = Resources.Load<Material>("Materials/StencilLighting_ExcludeSprite");
		if (masterRenderer.sharedMaterial.GetFloat("_FadeIntoWoods") == 1f)
		{
			material.EnableKeyword("_FADEINTOWOODS_ON");
		}
		renderObject = new GameObject("ExclusionRenderer");
		renderObject.layer = LayerMask.NameToLayer("Lighting_NoRender");
		renderObject.transform.parent = base.gameObject.transform;
		renderObject.transform.localPosition = Vector3.zero;
		renderObject.transform.localScale = Vector3.one;
		renderObject.transform.localRotation = Quaternion.identity;
		targetRenderer = renderObject.AddComponent<SpriteRenderer>();
		targetRenderer.sprite = masterRenderer.sprite;
		targetRenderer.flipX = masterRenderer.flipX;
		targetRenderer.flipY = masterRenderer.flipY;
		targetRenderer.sharedMaterial = material;
		targetRenderer.GetPropertyBlock(PropBlock);
		PropBlock.SetFloat("_ExcludeAmount", ExclusionAmount);
		targetRenderer.SetPropertyBlock(PropBlock);
		PropBlock.Clear();
	}
}
