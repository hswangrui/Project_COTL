using UnityEngine;

public class FlickerSpriteColor : BaseMonoBehaviour
{
	public float flickerAmt = 0.15f;

	public float lightStrength = 0.8f;

	public float flickerSpeed = 0.4f;

	public Color baseLight;

	public SpriteRenderer Light;

	private float timeOffset;

	public void Start()
	{
		Light = GetComponent<SpriteRenderer>();
		timeOffset = Random.value * 7f;
	}

	private void Update()
	{
		float num = (Time.time + timeOffset) * flickerSpeed;
		float num2 = Mathf.Sin(num * 7f) * flickerAmt + Mathf.Sin(num * 3f) * flickerAmt + lightStrength;
		Light.color = new Color(baseLight.r * num2, baseLight.g * num2, baseLight.b * num2, 1f);
	}
}
