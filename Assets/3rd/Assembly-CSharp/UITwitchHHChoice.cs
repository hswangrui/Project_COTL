using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITwitchHHChoice : MonoBehaviour
{
	[SerializeField]
	private TMP_Text choiceText;

	[SerializeField]
	private TMP_Text votesText;

	[SerializeField]
	private Image bar;

	public string ID { get; private set; }

	public void Configure(string choice, string ID)
	{
		this.ID = ID;
		choiceText.text = choice;
	}

	public void UpdateChoice(float votes, int totalVotes)
	{
		bar.fillAmount = votes / (float)totalVotes;
		votesText.text = votes.ToString();
	}

	public void SetWinner()
	{
		bar.color = Color.green;
	}
}
