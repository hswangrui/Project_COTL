using UnityEngine;

namespace Ara
{
	[RequireComponent(typeof(AraTrail))]
	public class TireTrack : MonoBehaviour
	{
		private AraTrail trail;

		public float offset = 0.05f;

		public float maxDist = 0.1f;

		private void OnEnable()
		{
			trail = GetComponent<AraTrail>();
			trail.onUpdatePoints += ProjectToGround;
		}

		private void OnDisable()
		{
			trail.onUpdatePoints -= ProjectToGround;
		}

		private void ProjectToGround()
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(base.transform.position, -Vector3.up), out hitInfo, maxDist))
			{
				if (trail.emit && trail.points.Count > 0)
				{
					AraTrail.Point value = trail.points[trail.points.Count - 1];
					if (!value.discontinuous)
					{
						value.normal = hitInfo.normal;
						value.position = hitInfo.point + hitInfo.normal * offset;
						trail.points[trail.points.Count - 1] = value;
					}
				}
				trail.emit = true;
			}
			else if (trail.emit)
			{
				trail.emit = false;
				if (trail.points.Count > 0)
				{
					trail.points.RemoveAt(trail.points.Count - 1);
				}
			}
		}
	}
}
