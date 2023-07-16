using UnityEngine;

public class FollowerDirectionSetter : BaseMonoBehaviour
{
	public GameObject[] GameObjects;

	private void GetChildren()
	{
		GameObjects = new GameObject[base.transform.childCount];
		int num = -1;
		while (++num < base.transform.childCount)
		{
			GameObjects[num] = base.transform.GetChild(num).gameObject;
		}
	}

	public void SetDirection()
	{
		GameObject[] gameObjects = GameObjects;
		for (int i = 0; i < gameObjects.Length; i++)
		{
			gameObjects[i].GetComponent<FollowerDirectionChanger>().chooseDirection();
		}
	}
}
