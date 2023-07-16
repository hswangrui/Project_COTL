using UnityEngine;

public class FollowerDirectionChanger : BaseMonoBehaviour
{
	public StateMachine s;

	public GameObject Up;

	public GameObject Diagonal;

	public GameObject Side;

	public Vector2 LeftSideRange = new Vector2(80f, 100f);

	public Vector2 RightSideRange = new Vector2(80f, 100f);

	public Vector2 UpRange = new Vector2(80f, 100f);

	public Vector2 DownRange = new Vector2(80f, 100f);

	private void Start()
	{
	}

	public void chooseDirection()
	{
		Up.SetActive(false);
		Diagonal.SetActive(false);
		Side.SetActive(false);
		Debug.Log("ChoosingDirection");
		if (s.facingAngle >= LeftSideRange.x && s.facingAngle <= LeftSideRange.y)
		{
			Side.SetActive(true);
		}
		else if (s.facingAngle >= RightSideRange.x && s.facingAngle <= RightSideRange.y)
		{
			Side.SetActive(true);
		}
		else if (s.facingAngle >= UpRange.x && s.facingAngle <= UpRange.y)
		{
			Up.SetActive(true);
		}
		else if (s.facingAngle >= DownRange.x && s.facingAngle <= DownRange.y)
		{
			Up.SetActive(true);
		}
		else
		{
			Diagonal.SetActive(true);
		}
	}
}
