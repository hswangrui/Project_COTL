using TMPro;
using UnityEngine.UI;

public class HUD_ProblemUnlockItem : BaseMonoBehaviour
{
	public Image Icon;

	public Image IconBackground;

	public TextMeshProUGUI TypeText;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI DescriptionText;

	public TextMeshProUGUI ProsText;

	public TextMeshProUGUI ConsText;

	public void Init(UnlockManager.UnlockNotificationData notification)
	{
		if (notification.SermonRitualType != 0)
		{
			Icon.sprite = SermonsAndRituals.Sprite(notification.SermonRitualType);
			IconBackground.gameObject.SetActive(false);
			TypeText.text = "Ritual";
			TitleText.text = SermonsAndRituals.LocalisedName(notification.SermonRitualType);
			DescriptionText.text = SermonsAndRituals.LocalisedDescription(notification.SermonRitualType);
			ProsText.text = SermonsAndRituals.LocalisedPros(notification.SermonRitualType);
			ConsText.text = SermonsAndRituals.LocalisedCons(notification.SermonRitualType);
		}
		else if (notification.StructureType != 0)
		{
			IconBackground.gameObject.SetActive(true);
			TypeText.text = "Building";
			TitleText.text = StructuresData.LocalizedName(notification.StructureType);
			DescriptionText.text = StructuresData.LocalizedDescription(notification.StructureType);
			ProsText.text = StructuresData.LocalizedPros(notification.StructureType);
			ConsText.text = StructuresData.LocalizedCons(notification.StructureType);
		}
	}
}
