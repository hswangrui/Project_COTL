using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_RedHeart : Interaction
{
	public int HP = 2;

	private float Delay = 1f;

	public string LabelName = "Heart";

	private PickUp p;

	public static List<Interaction_RedHeart> Hearts = new List<Interaction_RedHeart>();

	private void Start()
	{
		UpdateLocalisation();
		p = GetComponent<PickUp>();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		if (HP == 2)
		{
			LabelName = ScriptLocalization.Inventory.RED_HEART;
		}
		else
		{
			LabelName = ScriptLocalization.Inventory.HALF_HEART;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Hearts.Remove(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Hearts.Add(this);
		if (!(PlayerFarming.Instance != null))
		{
			return;
		}
		if (PlayerFarming.Instance.health.HP + PlayerFarming.Instance.health.SpiritHearts < PlayerFarming.Instance.health.totalHP + PlayerFarming.Instance.health.TotalSpiritHearts)
		{
			AutomaticallyInteract = true;
			Interactable = true;
			return;
		}
		if (p != null)
		{
			p.MagnetToPlayer = false;
		}
		Interactable = false;
		AutomaticallyInteract = false;
		base.Label = ScriptLocalization.Interactions.Fullhealth;
	}

	public override void GetLabel()
	{
		if (Delay < 0f)
		{
			if (PlayerFarming.Instance.health.HP + PlayerFarming.Instance.health.SpiritHearts < PlayerFarming.Instance.health.totalHP + PlayerFarming.Instance.health.TotalSpiritHearts)
			{
				AutomaticallyInteract = true;
				Interactable = true;
				base.Label = ".";
			}
			else
			{
				p.MagnetToPlayer = false;
				Interactable = false;
				AutomaticallyInteract = false;
				base.Label = ScriptLocalization.Interactions.Fullhealth;
			}
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

	public override void OnInteract(StateMachine state)
	{
		HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		base.OnInteract(state);
		CameraManager.shakeCamera(1f, Random.Range(0, 360));
		if (HP == 2)
		{
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
		}
		else
		{
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_small");
		}
		BiomeConstants.Instance.EmitBloodImpact(base.transform.position, 0f, "red", "BloodImpact_Large_0");
		AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.gameObject);
		component.Heal(HP * TrinketManager.GetHealthAmountMultiplier());
		Object.Destroy(base.gameObject);
	}
}
