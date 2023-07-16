using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction_ChaliceManager : Interaction
{
	public List<Interaction_Chalice> Chalices = new List<Interaction_Chalice>();

	private bool Activated;

	private void Start()
	{
		AutomaticallyInteract = true;
		int index = Random.Range(0, Chalices.Count);
		foreach (Interaction_Chalice chalice in Chalices)
		{
			chalice.Drink = Interaction_Chalice.DrinkType.Poison;
		}
		Chalices[index].Drink = Interaction_Chalice.DrinkType.Vitality;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Activated = true;
		StartCoroutine(MixDrinksUp());
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : "   ");
	}

	private IEnumerator MixDrinksUp()
	{
		foreach (Interaction_Chalice chalice in Chalices)
		{
			chalice.Activating = true;
		}
		yield return StartCoroutine(ShowPoison());
		int Loops = 5;
		while (Loops > 0)
		{
			yield return StartCoroutine(MixDrink());
			int num = Loops - 1;
			Loops = num;
			yield return null;
		}
		foreach (Interaction_Chalice chalice2 in Chalices)
		{
			chalice2.Activating = false;
		}
	}

	private IEnumerator ShowPoison()
	{
		yield return new WaitForSeconds(1f);
		foreach (Interaction_Chalice chalice in Chalices)
		{
			chalice.Spine.AnimationState.SetAnimation(0, (chalice.Drink == Interaction_Chalice.DrinkType.Poison) ? "show-bad" : "show-good", true);
		}
		yield return new WaitForSeconds(1.3333334f);
		foreach (Interaction_Chalice chalice2 in Chalices)
		{
			chalice2.Spine.AnimationState.SetAnimation(0, "idle", true);
		}
		yield return new WaitForSeconds(0.2f);
	}

	private IEnumerator MixDrink()
	{
		Interaction_Chalice C1 = Chalices[Random.Range(0, Chalices.Count)];
		Interaction_Chalice C2 = Chalices[Random.Range(0, Chalices.Count)];
		while (C1 == C2)
		{
			C2 = Chalices[Random.Range(0, Chalices.Count)];
		}
		float Progress = 0f;
		float Duration = 0.2f;
		Vector3 C1Position = C1.transform.position;
		Vector3 C2Position = C2.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			C1.transform.position = Vector3.Lerp(C1Position, C2Position, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			C2.transform.position = Vector3.Lerp(C2Position, C1Position, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		C1.transform.position = C2Position;
		C2.transform.position = C1Position;
		Debug.Log("Complete mixing");
		yield return new WaitForSeconds(0.2f);
	}
}
