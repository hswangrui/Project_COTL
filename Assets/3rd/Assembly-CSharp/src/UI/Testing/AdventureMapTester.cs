using System.Collections;
using Map;
using UnityEngine;

namespace src.UI.Testing
{
	public class AdventureMapTester : MonoBehaviour
	{
		public IEnumerator Start()
		{
			yield return null;
			MapManager.Instance.ShowMap();
		}
	}
}
