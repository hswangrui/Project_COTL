using System.Collections;
using UnityEngine;

public class SubRoutine : CustomYieldInstruction
{
	private IEnumerator e;

	public override bool keepWaiting
	{
		get
		{
			return e.MoveNext();
		}
	}

	public SubRoutine(IEnumerator e)
	{
		this.e = e;
	}
}
