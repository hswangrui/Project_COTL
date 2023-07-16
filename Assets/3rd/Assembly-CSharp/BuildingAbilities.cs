using System.Collections.Generic;
using I2.Loc;
using TMPro;

public class BuildingAbilities : BaseMonoBehaviour
{
	public enum BuildingAbility
	{
		NONE,
		Shrine_Buff_0,
		Shrine_Buff_1
	}

	public TextMeshProUGUI BuildingNameTxt;

	public TextMeshProUGUI BuildingDescriptionTxt;

	public StructureBrain.TYPES BuildingType;

	public List<BuildingAbility> BuildingAbilityList = new List<BuildingAbility>();

	public bool HasChanged { get; protected set; }

	public virtual void GetBuildingName()
	{
		BuildingNameTxt.text = StructuresData.GetLocalizedNameStatic(BuildingType);
	}

	public virtual void GetBuildingDescription()
	{
		BuildingDescriptionTxt.text = StructuresData.GetLocalizedNameStatic(BuildingType);
	}

	private void OnEnable()
	{
		OnEnableInteraction();
		LocalizationManager.OnLocalizeEvent += UpdateLocalisation;
	}

	public virtual void UpdateLocalisation()
	{
		GetBuildingName();
		GetBuildingDescription();
	}

	public virtual void OnEnableInteraction()
	{
	}

	private void OnDisable()
	{
		OnDisableInteraction();
		LocalizationManager.OnLocalizeEvent -= UpdateLocalisation;
	}

	public virtual void OnDisableInteraction()
	{
	}
}
