using I2.Loc;
using UnityEngine;

public class Interaction_BlackHeart : Interaction
{
	private float Delay = 1f;

	public string LabelName = "Black Heart";

	private void Awake()
	{
		if (PlayerFleeceManager.FleecePreventsHealthPickups())
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		LabelName = ScriptLocalization.Inventory.BLACK_HEART;
	}

	protected override void Update()
	{
		base.Update();
		Delay -= Time.deltaTime;
	}

	public override void GetLabel()
	{
		if (Delay < 0f)
		{
			base.Label = ".";
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		CameraManager.shakeCamera(1f, Random.Range(0, 360));
		HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "black", "burst_big");
		BiomeConstants.Instance.EmitBloodImpact(base.transform.position, 0f, "black", "BloodImpact_Large_0");
		component.BlackHearts += 2 * TrinketManager.GetHealthAmountMultiplier();
		HUD_Manager.Instance.Show(0);
		Object.Destroy(base.gameObject);
	}
}
