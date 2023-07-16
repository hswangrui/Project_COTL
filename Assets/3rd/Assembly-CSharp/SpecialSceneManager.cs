using UnityEngine;

public class SpecialSceneManager : BaseMonoBehaviour
{
	public GameObject Telescope;

	public static SpecialSceneManager Instance;

	private void Start()
	{
		Instance = this;
	}
}
