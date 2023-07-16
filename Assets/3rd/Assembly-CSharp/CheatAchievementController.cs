using Unify;
using UnityEngine;
using UnityEngine.UI;

public class CheatAchievementController : MonoBehaviour
{
	private int C_AchievementMax;

	private int C_AchievementCurrent;

	private Achievement C_achievement;

	public Text C_achievementLabelTextBox;

	public Text C_achievementTitleTextBox;

	public Button DefaultButton;

	private ForceSelection[] ForceSelections;

	private GameObject previouslySelected;
}
