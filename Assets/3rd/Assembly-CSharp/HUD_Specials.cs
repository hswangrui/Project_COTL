using TMPro;
using UnityEngine.UI;

public class HUD_Specials : BaseMonoBehaviour
{
	public Image ProgressWheel;

	public TextMeshProUGUI Ammo;

	public float PLAYER_SPECIAL_CHARGE
	{
		get
		{
			return DataManager.Instance.PLAYER_SPECIAL_CHARGE;
		}
		set
		{
			if (value > DataManager.Instance.PLAYER_SPECIAL_CHARGE_TARGET)
			{
				value = DataManager.Instance.PLAYER_SPECIAL_CHARGE_TARGET;
			}
			DataManager.Instance.PLAYER_SPECIAL_CHARGE = value;
			ProgressWheel.fillAmount = PLAYER_SPECIAL_CHARGE / DataManager.Instance.PLAYER_SPECIAL_CHARGE_TARGET;
		}
	}

	private void OnEnable()
	{
		PlayerFarming.OnGetSoul += OnGetSoul;
	}

	private void OnDisable()
	{
		PlayerFarming.OnGetSoul -= OnGetSoul;
	}

	private void Start()
	{
		ProgressWheel.fillAmount = PLAYER_SPECIAL_CHARGE / DataManager.Instance.PLAYER_SPECIAL_CHARGE_TARGET;
		PlayerWeapon.OnSpecial += OnSpecial;
	}

	private void OnSpecial()
	{
		Ammo.text = DataManager.Instance.PLAYER_SPECIAL_AMMO.ToString();
		float pLAYER_SPECIAL_AMMO = DataManager.Instance.PLAYER_SPECIAL_AMMO;
		float num = 0f;
	}

	private void OnGetSoul(int DeltaValue)
	{
		if (DeltaValue > 0 && DataManager.Instance.PLAYER_SPECIAL_AMMO > 0f)
		{
			PLAYER_SPECIAL_CHARGE += DeltaValue;
		}
	}
}
