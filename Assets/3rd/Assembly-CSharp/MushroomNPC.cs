using UnityEngine;

public class MushroomNPC : MonoBehaviour
{
	private void Awake()
	{
		if (DataManager.Instance.SozoDead)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
