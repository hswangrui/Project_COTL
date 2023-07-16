using UnityEngine;

public class HasPet : BaseMonoBehaviour
{
	private void Start()
	{
		GameObject obj = Object.Instantiate(Resources.Load("Prefabs/Units/Cat") as GameObject, base.transform.parent, true);
		obj.transform.position = base.transform.position - new Vector3(0f, -0.5f, 0f);
		obj.GetComponent<Pet>().Owner = base.gameObject;
	}
}
