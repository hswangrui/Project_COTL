using UnityEngine;

public class followerCountOnTemple : BaseMonoBehaviour
{
	public GameObject[] dotsFollowerCount;

	public int FollowerCount;

	public void Start()
	{
		updateCircles();
	}

	private void updateCircles()
	{
		for (int i = 0; i < dotsFollowerCount.Length; i++)
		{
			dotsFollowerCount[i].SetActive(false);
		}
		FollowerCount = DataManager.Instance.Followers.Count;
		for (int j = 0; j < Mathf.Clamp(FollowerCount, 0, dotsFollowerCount.Length); j++)
		{
			dotsFollowerCount[j].SetActive(true);
		}
	}

	public void Update()
	{
		if (FollowerCount != DataManager.Instance.Followers.Count)
		{
			updateCircles();
		}
	}
}
