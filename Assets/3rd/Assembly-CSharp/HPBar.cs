using UnityEngine;

public class HPBar : BaseMonoBehaviour
{
	public GameObject barInstant;

	public GameObject barTween;

	public GameObject defence;

	public EnemyOrderGroupIndicator groupIndicator;

	private void Start()
	{
		defence.SetActive(false);
	}
}
