using UnityEngine;

public class PostGameSpawnable : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.SetActive(GameManager.Layer2);
	}
}
