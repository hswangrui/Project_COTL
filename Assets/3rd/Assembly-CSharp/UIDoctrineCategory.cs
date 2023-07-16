using TMPro;
using UnityEngine;

public class UIDoctrineCategory : BaseMonoBehaviour
{
	public TextMeshProUGUI Title;

	public Transform Container;

	public void Play(SermonCategory Category)
	{
		Title.text = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(Category);
	}
}
