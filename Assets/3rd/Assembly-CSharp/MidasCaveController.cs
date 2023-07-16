using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MidasCaveController : MonoBehaviour
{
	public List<GameObject> Statues = new List<GameObject>();

	public void ShakeStatues()
	{
		AudioManager.Instance.PlayOneShot("event:/dialogue/midas_statues/laugh_midas_statues", PlayerFarming.Instance.gameObject);
		foreach (GameObject statue in Statues)
		{
			statue.transform.DOShakeScale(5f, new Vector3(0f, 0.2f), 5, 5f);
		}
	}
}
