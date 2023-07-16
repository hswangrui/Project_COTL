using System.Collections.Generic;
using UnityEngine;

public class Interaction_BlueHeart : Interaction
{
	public int HP = 2;

	private float Delay = 1f;

	public string LabelName = "Blue Heart";

	public static List<Interaction_BlueHeart> Hearts = new List<Interaction_BlueHeart>();

	private void Start()
	{
		UpdateLocalisation();
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

	protected override void Update()
	{
		base.Update();
		Delay -= Time.deltaTime;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Hearts.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Hearts.Remove(this);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		CameraManager.shakeCamera(1f, Random.Range(0, 360));
		HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		if (HP == 2)
		{
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "blue", "burst_big");
		}
		else
		{
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "blue", "burst_small");
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_blue_heart", base.gameObject);
		BiomeConstants.Instance.EmitBloodImpact(base.transform.position, 0f, "blue", "BloodImpact_Large_0");
		component.BlueHearts += HP * TrinketManager.GetHealthAmountMultiplier();
		HUD_Manager.Instance.Show(0);
		Hearts.Remove(this);
		Object.Destroy(base.gameObject);
	}
}
