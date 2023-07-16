using System.Collections.Generic;
using UnityEngine;

public class MoonImage : BaseMonoBehaviour
{
	public List<Sprite> Images;

	private void Start()
	{
		GetComponent<SpriteRenderer>().sprite = Images[DataManager.Instance.CurrentDay.MoonPhase];
	}
}
