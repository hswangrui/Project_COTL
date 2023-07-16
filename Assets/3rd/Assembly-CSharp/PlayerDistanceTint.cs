using UnityEngine;

public class PlayerDistanceTint : BaseMonoBehaviour
{
	public GameObject tintObjectLocation;

	public float distanceToTint;

	public float fillAmount = 0.5f;

	public Color tintColor;

	public Material playerMaterial;

	private int colorID;

	private int floatID;

	private MaterialPropertyBlock block;

	public float dist;

	public Color lerpedColor;

	public float distPercent;

	public bool ResetToZero;

	[SerializeField]
	private bool useItemInWoodsColor;

	private bool wasInRange;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		floatID = Shader.PropertyToID("_FillAlpha");
		SetDefaults();
	}

	private void SetDefaults()
	{
		if (playerMaterial != null)
		{
			playerMaterial.SetFloat(floatID, 0f);
		}
	}

	private void OnDisable()
	{
		SetDefaults();
	}

	private void OnDestroy()
	{
		SetDefaults();
	}

	private void OnEnable()
	{
		playerMaterial.SetFloat(floatID, 0f);
	}

	private void Update()
	{
		if (PlayerFarming.Instance != null)
		{
			dist = Vector3.Distance(PlayerFarming.Instance.gameObject.transform.position, tintObjectLocation.transform.position);
			if (dist <= distanceToTint)
			{
				distPercent = Mathf.Abs(dist / distanceToTint - 1f);
				playerMaterial.SetFloat(floatID, distPercent);
				wasInRange = true;
			}
			else if (wasInRange)
			{
				playerMaterial.SetFloat(floatID, 0f);
				wasInRange = false;
			}
		}
	}
}
