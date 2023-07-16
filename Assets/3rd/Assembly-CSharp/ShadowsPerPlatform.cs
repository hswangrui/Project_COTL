using UnityEngine;
using UnityEngine.Rendering;

public class ShadowsPerPlatform : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	private void Start()
	{
		if (_spriteRenderer == null)
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}
		if (!(_spriteRenderer == null))
		{
			_spriteRenderer.shadowCastingMode = ShadowCastingMode.On;
		}
	}
}
