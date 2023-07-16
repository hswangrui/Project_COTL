using System.Collections;
using UnityEngine;

namespace Spine.Unity.Examples
{
	public class SkeletonGhostRenderer : MonoBehaviour
	{
		private static readonly Color32 TransparentBlack = new Color32(0, 0, 0, 0);

		private const string colorPropertyName = "_Color";

		private float fadeSpeed = 10f;

		private Color32 startColor;

		private MeshFilter meshFilter;

		private MeshRenderer meshRenderer;

		private MaterialPropertyBlock mpb;

		private int colorId;

		private void Awake()
		{
			meshRenderer = base.gameObject.GetComponent<MeshRenderer>();
			meshFilter = base.gameObject.GetComponent<MeshFilter>();
			colorId = Shader.PropertyToID("_Color");
			mpb = new MaterialPropertyBlock();
		}

		public void Initialize(Mesh mesh, Material[] materials, Color32 color, bool additive, float speed, int sortingLayerID, int sortingOrder)
		{
			StopAllCoroutines();
			base.gameObject.SetActive(true);
			meshRenderer = base.gameObject.GetComponent<MeshRenderer>();
			meshFilter = base.gameObject.GetComponent<MeshFilter>();
			meshRenderer.sharedMaterials = materials;
			meshRenderer.sortingLayerID = sortingLayerID;
			meshRenderer.sortingOrder = sortingOrder;
			meshFilter.sharedMesh = Object.Instantiate(mesh);
			startColor = color;
			mpb.SetColor(colorId, color);
			meshRenderer.SetPropertyBlock(mpb);
			fadeSpeed = speed;
			if (additive)
			{
				StartCoroutine(FadeAdditive());
			}
			else
			{
				StartCoroutine(Fade());
			}
		}

		private IEnumerator Fade()
		{
			Color32 startColor2 = startColor;
			Color32 black = TransparentBlack;
			float t = 1f;
			for (float hardTimeLimit = 5f; hardTimeLimit > 0f; hardTimeLimit -= Time.deltaTime)
			{
				Color32 color = Color32.Lerp(black, startColor, t);
				mpb.SetColor(colorId, color);
				meshRenderer.SetPropertyBlock(mpb);
				t = Mathf.Lerp(t, 0f, Time.deltaTime * fadeSpeed);
				if (t <= 0f)
				{
					break;
				}
				yield return null;
			}
			Object.Destroy(meshFilter.sharedMesh);
			base.gameObject.SetActive(false);
		}

		private IEnumerator FadeAdditive()
		{
			Color32 startColor2 = startColor;
			Color32 black = TransparentBlack;
			float t = 1f;
			for (float hardTimeLimit = 5f; hardTimeLimit > 0f; hardTimeLimit -= Time.deltaTime)
			{
				Color32 color = Color32.Lerp(black, startColor, t);
				mpb.SetColor(colorId, color);
				meshRenderer.SetPropertyBlock(mpb);
				t = Mathf.Lerp(t, 0f, Time.deltaTime * fadeSpeed);
				if (t <= 0f)
				{
					break;
				}
				yield return null;
			}
			Object.Destroy(meshFilter.sharedMesh);
			base.gameObject.SetActive(false);
		}

		public void Cleanup()
		{
			if (meshFilter != null && meshFilter.sharedMesh != null)
			{
				Object.Destroy(meshFilter.sharedMesh);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
