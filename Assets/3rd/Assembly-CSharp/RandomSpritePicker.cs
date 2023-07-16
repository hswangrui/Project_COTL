using UnityEngine;

public class RandomSpritePicker : BaseMonoBehaviour
{
	public bool sprites;

	public Sprite[] Sprites;

	public bool Scale;

	public bool randomFlipX;

	public bool randomFlipY;

	public float scaleLow = 0.8f;

	public float scaleHigh = 1.1f;

	private float randomScaleFloat;

	private Vector3 randomScale3 = new Vector3(-40.1f, -40.1f, -40.1f);

	public bool Position;

	public float OFFSETLow = -2f;

	public float OFFSETHigh = 2f;

	public bool Rotation;

	public Vector2 MinMaxValueSlider = new Vector2(-2f, 2f);

	public bool Colour;

	public bool tintOnDistance;

	public Color[] Colours;

	[Space]
	private bool hasRandomised;

	[Space]
	public bool lockIn;

	private int randomInt;

	private float dist;

	public bool TURN_OFF_ON_LOW_QUALITY = true;

	private void Randomise()
	{
		hasRandomised = false;
		randomise_();
	}

	private void LockItIn()
	{
		lockIn = !lockIn;
	}

	private void UnlockIt()
	{
		lockIn = !lockIn;
	}

	private void Start()
	{
		bool tURN_OFF_ON_LOW_QUALITY = TURN_OFF_ON_LOW_QUALITY;
		randomise_();
	}

	private void randomise_()
	{
		if (lockIn || hasRandomised)
		{
			return;
		}
		SpriteRenderer component = base.gameObject.GetComponent<SpriteRenderer>();
		if (!(component != null))
		{
			return;
		}
		if (Sprites.Length != 0)
		{
			int num = Random.Range(0, Sprites.Length);
			component.sprite = Sprites[num];
		}
		if (randomFlipX)
		{
			randomInt = Random.Range(0, 2);
			if (randomInt == 1)
			{
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}
		if (randomFlipY)
		{
			randomInt = Random.Range(0, 2);
			if (randomInt == 1)
			{
				base.transform.localScale = new Vector3(1f, -1f, 1f);
			}
		}
		if (Rotation)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			eulerAngles.z = Random.Range(MinMaxValueSlider.x, MinMaxValueSlider.y);
			base.transform.eulerAngles = eulerAngles;
		}
		if (Scale)
		{
			randomScaleFloat = Random.Range(scaleLow, scaleHigh);
			randomScale3 = new Vector3(randomScaleFloat, randomScaleFloat, randomScaleFloat);
			base.transform.localScale = randomScale3;
		}
		if (Colour && Colours.Length != 0)
		{
			int num2 = Random.Range(0, Colours.Length);
			component.color = Colours[num2];
		}
		if (Position)
		{
			Vector3 vector = new Vector3(Random.Range(OFFSETLow, OFFSETHigh), Random.Range(OFFSETLow, OFFSETHigh), 0f);
			base.transform.position = base.transform.position + vector;
		}
		hasRandomised = true;
	}
}
