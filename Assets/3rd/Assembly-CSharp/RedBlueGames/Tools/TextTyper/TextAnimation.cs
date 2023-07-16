using TMPro;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public abstract class TextAnimation : MonoBehaviour
	{
		[Tooltip("0-based index of the first printable character that should be animated")]
		[SerializeField]
		private int firstCharToAnimate;

		[Tooltip("0-based index of the last printable character that should be animated")]
		[SerializeField]
		private int lastCharToAnimate;

		[Tooltip("If true, animation will begin playing immediately on Awake")]
		[SerializeField]
		private bool playOnAwake;

		private const float frameRate = 30f;

		private static readonly float timeBetweenAnimates = 1f / 30f;

		private float lastAnimateTime;

		private TextMeshProUGUI textComponent;

		private TMP_TextInfo textInfo;

		private TMP_MeshInfo[] cachedMeshInfo;

		protected int FirstCharToAnimate => firstCharToAnimate;

		protected int LastCharToAnimate => lastCharToAnimate;

		private TextMeshProUGUI TextComponent
		{
			get
			{
				if (textComponent == null)
				{
					textComponent = GetComponent<TextMeshProUGUI>();
				}
				return textComponent;
			}
		}

		public void SetCharsToAnimate(int firstChar, int lastChar)
		{
			firstCharToAnimate = firstChar;
			lastCharToAnimate = lastChar;
		}

		public void CacheTextMeshInfo()
		{
			textInfo = TextComponent.textInfo;
			cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
		}

		protected virtual void Awake()
		{
			base.enabled = playOnAwake;
		}

		protected virtual void Start()
		{
			TextComponent.ForceMeshUpdate();
			lastAnimateTime = float.MinValue;
		}

		protected virtual void OnEnable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTMProChanged);
		}

		protected virtual void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTMProChanged);
			TextComponent.ForceMeshUpdate();
		}

		protected virtual void Update()
		{
			if (Time.time > lastAnimateTime + timeBetweenAnimates)
			{
				AnimateAllChars();
			}
		}

		protected abstract void Animate(int characterIndex, out Vector2 translation, out float rotation, out float scale);

		public void AnimateAllChars()
		{
			lastAnimateTime = Time.time;
			int characterCount = textInfo.characterCount;
			if (characterCount == 0)
			{
				return;
			}
			for (int i = 0; i < characterCount; i++)
			{
				if (i >= firstCharToAnimate && i <= lastCharToAnimate)
				{
					TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[i];
					if (tMP_CharacterInfo.isVisible)
					{
						int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
						int vertexIndex = tMP_CharacterInfo.vertexIndex;
						Vector3[] vertices = cachedMeshInfo[materialReferenceIndex].vertices;
						Vector3 vector = (Vector2)((vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2f);
						Vector3[] vertices2 = textInfo.meshInfo[materialReferenceIndex].vertices;
						vertices2[vertexIndex] = vertices[vertexIndex] - vector;
						vertices2[vertexIndex + 1] = vertices[vertexIndex + 1] - vector;
						vertices2[vertexIndex + 2] = vertices[vertexIndex + 2] - vector;
						vertices2[vertexIndex + 3] = vertices[vertexIndex + 3] - vector;
						Animate(i, out var translation, out var rotation, out var scale);
						Matrix4x4 matrix4x = Matrix4x4.TRS(translation, Quaternion.Euler(0f, 0f, rotation), scale * Vector3.one);
						vertices2[vertexIndex] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex]);
						vertices2[vertexIndex + 1] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex + 1]);
						vertices2[vertexIndex + 2] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex + 2]);
						vertices2[vertexIndex + 3] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex + 3]);
						vertices2[vertexIndex] += vector;
						vertices2[vertexIndex + 1] += vector;
						vertices2[vertexIndex + 2] += vector;
						vertices2[vertexIndex + 3] += vector;
					}
				}
			}
			ApplyChangesToMesh();
		}

		private void ApplyChangesToMesh()
		{
			for (int i = 0; i < textInfo.meshInfo.Length; i++)
			{
				textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
				TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
			}
		}

		private void OnTMProChanged(Object obj)
		{
			if (obj == TextComponent)
			{
				CacheTextMeshInfo();
			}
		}
	}
}
