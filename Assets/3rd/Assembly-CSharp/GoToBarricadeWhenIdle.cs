using UnityEngine;

[RequireComponent(typeof(Health))]
public class GoToBarricadeWhenIdle : BaseMonoBehaviour
{
	private StateMachine state;

	private FollowPath followPath;

	private Barricade myBarricade;

	private Health health;

	private void Start()
	{
		state = GetComponent<StateMachine>();
		followPath = GetComponent<FollowPath>();
		health = GetComponent<Health>();
	}

	private void Update()
	{
		if (state.CURRENT_STATE != 0)
		{
			return;
		}
		if (myBarricade == null)
		{
			foreach (Barricade barricade in Barricade.barricades)
			{
				if (!barricade.occupied && Vector2.Distance(barricade.transform.position, base.transform.position) < 0.7f)
				{
					myBarricade = barricade;
					followPath.givePath(barricade.transform.position);
					followPath.NewPath += leaveBarricade;
					barricade.occupied = true;
					break;
				}
			}
			return;
		}
		if (state.CURRENT_STATE != StateMachine.State.Moving && !state.isDefending)
		{
			state.isDefending = true;
		}
	}

	private void leaveBarricade()
	{
		if (myBarricade != null)
		{
			state.isDefending = false;
			myBarricade.occupied = false;
			myBarricade = null;
			followPath.NewPath -= leaveBarricade;
		}
	}

	private void OnDestroy()
	{
		followPath.NewPath -= leaveBarricade;
	}
}
