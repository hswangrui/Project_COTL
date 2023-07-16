using UnityEngine;

public class RandomEnable : BaseMonoBehaviour
{
	private void OnEnable()
	{
		if (Random.Range(0, 100) < 50)
		{
			base.gameObject.SetActive(false);
		}
	}
}
