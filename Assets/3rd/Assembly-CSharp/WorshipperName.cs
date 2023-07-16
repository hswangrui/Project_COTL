using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorshipperName : BaseMonoBehaviour
{
	public Follower Follower;

	public TextMeshProUGUI Text;

	public Gradient ColorGradient;

	public Image DevotionRing;

	public TextMeshProUGUI DevotionText;

	private Color color;

	private float Alpha;

	private float Distance;

	private void Update()
	{
		if (Follower.Brain != null)
		{
			Text.text = Follower.Brain.Info.Name;
			color = ColorGradient.Evaluate(Follower.Brain.Stats.Happiness / 100f);
			Text.color = color;
		}
	}
}
