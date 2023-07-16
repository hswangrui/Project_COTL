using UnityEngine;

namespace Map
{
	public class DottedLineRenderer : MonoBehaviour
	{
		public bool scaleInUpdate;

		private LineRenderer lR;

		private Renderer rend;

		private void Start()
		{
			ScaleMaterial();
			base.enabled = scaleInUpdate;
		}

		public void ScaleMaterial()
		{
			lR = GetComponent<LineRenderer>();
			rend = GetComponent<Renderer>();
			rend.material.mainTextureScale = new Vector2(Vector2.Distance(lR.GetPosition(0), lR.GetPosition(lR.positionCount - 1)) / lR.widthMultiplier, 1f);
		}

		private void Update()
		{
			rend.material.mainTextureScale = new Vector2(Vector2.Distance(lR.GetPosition(0), lR.GetPosition(lR.positionCount - 1)) / lR.widthMultiplier, 1f);
		}
	}
}
