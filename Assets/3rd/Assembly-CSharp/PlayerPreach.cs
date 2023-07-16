using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPreach : BaseMonoBehaviour
{
	private PlayerFarming playerFarming;

	private StateMachine state;

	private float Timer;

	private List<Worshipper> Worshippers;

	private void Start()
	{
		playerFarming = GetComponent<PlayerFarming>();
		state = GetComponent<StateMachine>();
	}

	private void Update()
	{
		StateMachine.State cURRENT_STATE = state.CURRENT_STATE;
		if ((uint)cURRENT_STATE > 1u)
		{
			return;
		}
		foreach (Worshipper worshipper in Worshipper.worshippers)
		{
			if (!worshipper.GivenPreachSoul && Vector3.Distance(base.transform.position, worshipper.transform.position) < 2f)
			{
				worshipper.GivenPreachSoul = true;
				worshipper.StartWorship(base.gameObject);
				CameraManager.shakeCamera(0.1f, Utils.GetAngle(base.transform.position, worshipper.transform.position));
			}
		}
	}

	private IEnumerator Preach()
	{
		Timer = 0f;
		state.CURRENT_STATE = StateMachine.State.Preach;
		Worshippers = new List<Worshipper>();
		foreach (Worshipper worshipper in Worshipper.worshippers)
		{
			if (Vector3.Distance(base.transform.position, worshipper.transform.position) < 3f)
			{
				worshipper.StartWorship(base.gameObject);
				Worshippers.Add(worshipper);
			}
		}
		yield return new WaitForSeconds(0.5f);
		if (Worshippers.Count > 0)
		{
			CameraManager.shakeCamera(0.3f, Random.Range(0, 360));
		}
		foreach (Worshipper worshipper2 in Worshippers)
		{
			if (!worshipper2.GivenPreachSoul)
			{
				SoulCustomTarget.Create(base.gameObject, worshipper2.transform.position, Color.black, null);
			}
			worshipper2.GivenPreachSoul = true;
		}
		yield return new WaitForSeconds(2f);
		foreach (Worshipper worshipper3 in Worshippers)
		{
			worshipper3.EndWorship();
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
