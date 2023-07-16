using UnityEngine;
using UnityEngine.U2D;

namespace src.Rendering
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class MMCombinedMeshRenderer : MonoBehaviour
	{
		[Header("Input")]
		[SerializeField]
		private SpriteAtlas _atlas;

		[SerializeField]
		private Material _srcMaterial;

		[SerializeField]
		private Shader _newShaderSrc;

		[Header("Output")]
		[SerializeField]
		private MeshFilter _meshFilter;

		[SerializeField]
		private MeshRenderer _meshRenderer;

		[SerializeField]
		private Mesh _mesh;

		[SerializeField]
		private Material _material;

		private void Reset()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();
		}

		private void Clear()
		{
			if (_mesh != null)
			{
				Object.DestroyImmediate(_mesh);
			}
			if (_material != null)
			{
				Object.DestroyImmediate(_material);
			}
		}
	}
}
