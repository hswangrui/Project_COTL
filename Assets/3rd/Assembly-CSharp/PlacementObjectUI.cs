using TMPro;
using UnityEngine;

public class PlacementObjectUI : BaseMonoBehaviour
{
	[SerializeField]
	private Vector3 positionOffset;

	[SerializeField]
	private TMP_Text costText;

	private PlacementObject placementObject;

	private Camera mainCamera;

	private CanvasGroup canvasGroup;

	private bool hiding;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		mainCamera = Camera.main;
	}

	public void Play(PlacementObject placement, Structure structure)
	{
		placementObject = placement;
		base.gameObject.SetActive(true);
		UpdateText(structure.Type);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	private void Update()
	{
		if ((bool)placementObject)
		{
			Vector3 position = mainCamera.WorldToScreenPoint(placementObject.transform.position);
			position += positionOffset;
			base.transform.position = position;
		}
	}

	public void UpdateText(StructureBrain.TYPES type)
	{
		costText.text = GetCostText(type);
	}

	private string GetCostText(StructureBrain.TYPES type)
	{
		return CostFormatter.FormatCosts(StructuresData.GetCost(type));
	}
}
