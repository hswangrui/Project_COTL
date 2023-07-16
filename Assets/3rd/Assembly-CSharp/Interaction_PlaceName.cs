using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_PlaceName : BaseMonoBehaviour
{
	[TermsPopup("")]
	public string PlaceName;

	public float Radius = 3f;

	private GameObject Player;

	private void Start()
	{
		StartCoroutine(UpdateRoutine());
	}

	private IEnumerator UpdateRoutine()
	{
		while ((Player = GameObject.FindGameObjectWithTag("Player")) == null)
		{
			yield return null;
		}
		while (Player != null && Vector3.Distance(base.transform.position, Player.transform.position) > Radius)
		{
			yield return null;
		}
		if (!(Player == null))
		{
			HUD_DisplayName.Play(PlaceName);
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, Radius, Color.white);
	}
}
