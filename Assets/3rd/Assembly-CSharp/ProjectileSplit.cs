using System.Collections.Generic;
using UnityEngine;

public class ProjectileSplit : Projectile
{
	public GameObject Arrow;

	public float NumSplit = 10f;

	public float ChildSpeed = 4f;

	[SerializeField]
	private List<GameObject> children = new List<GameObject>();

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!string.IsNullOrEmpty(OnSpawnSoundPath))
		{
			AudioManager.Instance.PlayOneShot(OnSpawnSoundPath, base.gameObject);
		}
		if ((bool)Trail)
		{
			Trail.Clear();
			Trail.emit = true;
		}
		foreach (GameObject child in children)
		{
			child.SetActive(true);
		}
		Speed = 8f;
	}

	public override void EndOfLife()
	{
		AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
		Debug.Log("Speed " + Speed);
		base.EndOfLife();
		Angle = 45f;
		int num = -1;
		while ((float)(++num) < NumSplit)
		{
			Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = (Angle += 360f / NumSplit);
			component.team = base.team;
			component.Speed = ChildSpeed;
			component.Owner = Owner;
		}
		foreach (GameObject child in children)
		{
			child.SetActive(false);
		}
	}
}
