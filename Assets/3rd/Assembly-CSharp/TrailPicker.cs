using Ara;
using UnityEngine;

public class TrailPicker : MonoBehaviour
{
	[SerializeField]
	private AraTrail araTrail;

	[SerializeField]
	private TrailRenderer lowQualityTrail;

	private void Awake()
	{
		if (lowQualityTrail != null)
		{
			if (araTrail)
				araTrail.enabled = true;
			else
				Debug.LogWarning("araTrail IS NULL");
			lowQualityTrail.enabled = false;
		}
	}

	public void ClearTrails()
	{
        if (araTrail)
            araTrail.Clear();
	}
}
