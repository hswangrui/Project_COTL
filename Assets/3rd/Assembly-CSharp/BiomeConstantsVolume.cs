using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class BiomeConstantsVolume : MonoBehaviour
{
	private enum ShaderTypes
	{
		Float,
		Color,
		Texture
	}

	public List<BiomeVolume> biomeVolume = new List<BiomeVolume>();

	public float BlendTime = 2f;

	public bool ShowInSceneView = true;

	public bool activateObject;

	public GameObject objectToActivate;

	private ShaderTypes type;

	public List<BiomeVolume> MyList;

	public bool inTrigger;

	private void Update()
	{
		if (Application.isPlaying || !Application.isEditor)
		{
			return;
		}
		foreach (BiomeVolume item in biomeVolume)
		{
			item.shaderName = item.getName(item._ShaderNames);
		}
	}

	private void Start()
	{
		if (objectToActivate != null)
		{
			objectToActivate.SetActive(false);
		}
		MyList = new List<BiomeVolume>();
		for (int i = 0; i < biomeVolume.Count; i++)
		{
			MyList.Add(biomeVolume[i]);
		}
	}

	public void activate()
	{
		if (objectToActivate != null)
		{
			objectToActivate.SetActive(true);
		}
		GameManager.startCoroutineAdjustGlobalShaders(MyList, BlendTime, 0f, 1f);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		inTrigger = true;
		if (objectToActivate != null)
		{
			objectToActivate.SetActive(true);
		}
		GameManager.startCoroutineAdjustGlobalShaders(MyList, BlendTime, 0f, 1f);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		inTrigger = false;
		GameManager.startCoroutineAdjustGlobalShaders(MyList, BlendTime, 1f, 0f);
		if (objectToActivate != null)
		{
			objectToActivate.SetActive(false);
		}
	}

	public void manualExitAndDeactive()
	{
		inTrigger = false;
		if (GameManager.GetInstance() != null)
		{
			GameManager.startCoroutineAdjustGlobalShaders(MyList, BlendTime, 1f, 0f);
		}
		if (objectToActivate != null)
		{
			objectToActivate.SetActive(false);
		}
		base.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		manualExitAndDeactive();
	}

	private void OnDrawGizmos()
	{
		if (!ShowInSceneView)
		{
			return;
		}
		BoxCollider component = GetComponent<BoxCollider>();
		BoxCollider2D component2 = GetComponent<BoxCollider2D>();
		if (component != null || component2 != null)
		{
			Vector3 center;
			Vector3 size;
			if (component != null)
			{
				center = component.center;
				size = component.size;
			}
			else
			{
				center = component2.offset;
				size = component2.size;
			}
			Gizmos.color = Color.green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(center, size);
		}
	}

	private void OnDrawGizmosSelected()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		BoxCollider2D component2 = GetComponent<BoxCollider2D>();
		if (component != null || component2 != null)
		{
			Color green = Color.green;
			green.a = 0.2f;
			Gizmos.color = green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Vector3 center;
			Vector3 size;
			if (component != null)
			{
				center = component.center;
				size = component.size;
			}
			else
			{
				center = component2.offset;
				size = component2.size;
			}
			Gizmos.DrawCube(center, size);
		}
	}
}
