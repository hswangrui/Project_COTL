using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CommandPoint : BaseMonoBehaviour
{
	public Health.Team team;

	private SpriteRenderer spriterenderer;

	public Sprite ImageNeutral;

	public Sprite ImageTeam1;

	public Sprite ImageTeam2;

	private void Start()
	{
		spriterenderer = GetComponent<SpriteRenderer>();
		setImage();
	}

	private void setImage()
	{
		switch (team)
		{
		case Health.Team.Neutral:
			spriterenderer.sprite = ImageNeutral;
			break;
		case Health.Team.PlayerTeam:
			spriterenderer.sprite = ImageTeam1;
			break;
		case Health.Team.Team2:
			spriterenderer.sprite = ImageTeam2;
			break;
		}
	}
}
