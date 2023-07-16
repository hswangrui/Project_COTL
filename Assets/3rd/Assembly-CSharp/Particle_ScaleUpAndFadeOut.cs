using UnityEngine;

public class Particle_ScaleUpAndFadeOut : BaseMonoBehaviour
{
	private float scaleSpeed;

	private float scale;

	private float timer;

	public float InitScale;

	private void Start()
	{
		base.transform.localScale = new Vector3(1f, InitScale, 1f);
	}

	private void Update()
	{
		if ((timer += Time.deltaTime) > 0.2f)
		{
			scale -= 0.1f;
			base.transform.localScale = new Vector3(scale, scale, 1f);
			if (scale <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			scaleSpeed += (1f - scale) * 0.3f;
			scale += (scaleSpeed *= 0.7f);
			base.transform.localScale = new Vector3(1f, scale, 1f);
		}
		base.transform.eulerAngles = new Vector3(-60f, 0f, 0f);
	}
}
