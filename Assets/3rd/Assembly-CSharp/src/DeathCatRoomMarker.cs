using UnityEngine;

namespace src
{
	public class DeathCatRoomMarker : MonoBehaviour
	{
		public static bool IsDeathCatRoom { get; private set; }

		private void OnEnable()
		{
			IsDeathCatRoom = true;
		}

		private void OnDisable()
		{
			IsDeathCatRoom = false;
		}
	}
}
