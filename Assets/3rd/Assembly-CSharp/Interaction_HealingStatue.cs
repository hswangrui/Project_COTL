using I2.Loc;

public class Interaction_HealingStatue : Interaction
{
	private string sLabel;

	private bool Activated;

	public HealingWaterFade HealingWaterFade;

	private HealthPlayer _healthPlayer;

	private HealthPlayer healthPlayer
	{
		get
		{
			if (_healthPlayer == null)
			{
				healthPlayer = PlayerFarming.Instance.GetComponent<HealthPlayer>();
			}
			return _healthPlayer;
		}
		set
		{
			_healthPlayer = value;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		Activated = false;
	}

	public override void GetLabel()
	{
		if (healthPlayer.HP >= healthPlayer.totalHP && healthPlayer.SpiritHearts >= healthPlayer.TotalSpiritHearts)
		{
			Interactable = false;
		}
		else
		{
			Interactable = true;
		}
		sLabel = (Interactable ? ScriptLocalization.Interactions.HealingStatue : ScriptLocalization.Interactions.Fullhealth);
		base.Label = sLabel;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = (Interactable ? ScriptLocalization.Interactions.HealingStatue : ScriptLocalization.Interactions.Fullhealth);
	}

	public override void OnInteract(StateMachine state)
	{
		if (healthPlayer.HP < healthPlayer.totalHP || healthPlayer.SpiritHearts < healthPlayer.TotalSpiritHearts)
		{
			base.OnInteract(state);
			healthPlayer.HP = healthPlayer.totalHP;
			healthPlayer.SpiritHearts = healthPlayer.TotalSpiritHearts;
			HealingWaterFade healingWaterFade = HealingWaterFade;
			if ((object)healingWaterFade != null)
			{
				healingWaterFade.CrossFade();
			}
			BiomeConstants.Instance.EmitHeartPickUpVFX(healthPlayer.transform.position, 0f, "red", "burst_big");
			CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.transform.position);
			Activated = true;
			Interactable = false;
			base.HasChanged = true;
		}
	}
}
