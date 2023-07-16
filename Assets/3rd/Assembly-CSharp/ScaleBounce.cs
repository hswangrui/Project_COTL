using UnityEngine;

public class ScaleBounce : BaseMonoBehaviour
{
	public float TargetX = 1f;

	public float TargetY = 1f;

	public float DampingX = 0.3f;

	public float ElasticX = 0.7f;

	public float DampingY = 0.3f;

	public float ElasticY = 0.7f;

	private float scaleSpeedX;

	private float scaleSpeedY;

	private float scaleX = 1f;

	private float scaleY = 1f;

	public float StartScaleX = 1f;

	public float StartScaleY = 1f;

	private void OnEnable()
	{
		scaleX = StartScaleX;
		scaleY = StartScaleY;
		base.gameObject.transform.localScale = new Vector3(scaleX, scaleY);
	}

	public void SquishMe(float _scaleSpeedX, float _scaleSpeedY)
	{
		scaleSpeedX = _scaleSpeedX;
		scaleSpeedY = _scaleSpeedY;
	}

	private void Update()
	{
		if (Time.timeScale != 0f)
		{
			scaleX = base.gameObject.transform.localScale.x;
			scaleY = base.gameObject.transform.localScale.y;
			scaleSpeedX += (TargetX - scaleX) * DampingX / Time.deltaTime;
			scaleX += (scaleSpeedX *= ElasticX) * Time.deltaTime;
			scaleSpeedY += (TargetY - scaleY) * DampingY / Time.deltaTime;
			scaleY += (scaleSpeedY *= ElasticY) * Time.deltaTime;
			base.gameObject.transform.localScale = new Vector3(scaleX, scaleY);
		}
	}
}
