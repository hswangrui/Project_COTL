using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineQueue
{
	private BaseMonoBehaviour m_Owner;

	private Coroutine m_InternalCoroutine;

	private Queue<IEnumerator> actions = new Queue<IEnumerator>();

	public Queue<IEnumerator> Actions
	{
		get
		{
			return actions;
		}
	}

	public CoroutineQueue(BaseMonoBehaviour aCoroutineOwner)
	{
		m_Owner = aCoroutineOwner;
	}

	public void StartLoop()
	{
		m_InternalCoroutine = m_Owner.StartCoroutine(Process());
	}

	public void StopLoop()
	{
		m_Owner.StopCoroutine(m_InternalCoroutine);
		m_InternalCoroutine = null;
	}

	public void EnqueueAction(IEnumerator aAction)
	{
		actions.Enqueue(aAction);
	}

	private IEnumerator Process()
	{
		while (true)
		{
			if (actions.Count > 0)
			{
				yield return m_Owner.StartCoroutine(actions.Dequeue());
			}
			else
			{
				yield return null;
			}
		}
	}
}
