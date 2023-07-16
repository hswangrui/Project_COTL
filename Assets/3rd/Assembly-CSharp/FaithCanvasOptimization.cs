using UnityEngine;

public class FaithCanvasOptimization : MonoBehaviour
{
	[SerializeField]
	private Canvas canvasToOptimize;

	[SerializeField]
	private GameObject[] gameObjectsToTrack;

	[SerializeField]
	private MonoBehaviour[] callUpdateScriptsByForce;

	private IUpdateManually[] callUpdateScriptsByForceArray;

	public void ActivateCanvas()
	{
		if (canvasToOptimize != null)
		{
			canvasToOptimize.gameObject.SetActive(true);
		}
	}
}
