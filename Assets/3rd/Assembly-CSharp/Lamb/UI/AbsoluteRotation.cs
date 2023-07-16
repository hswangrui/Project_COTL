using UnityEngine;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class AbsoluteRotation : MonoBehaviour
	{
		[SerializeField]
		private Vector3 _rotation;

		public void Update()
		{
			base.transform.rotation = Quaternion.Euler(_rotation);
		}
	}
}
