using UnityEngine;

namespace DOGames.Scripts
{
	public class DontDestroyOnLoad : MonoBehaviour
	{
		private void Start()
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
