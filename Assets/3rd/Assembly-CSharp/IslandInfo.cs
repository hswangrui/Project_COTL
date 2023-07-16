using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class IslandInfo : BaseMonoBehaviour
{
	public bool NorthCollider = true;

	public bool EastCollider = true;

	public bool SouthCollider = true;

	public bool WestCollider = true;

	public float NorthColliderSpacing = 2f;

	public float EastColliderSpacing = 2f;

	public float SouthColliderSpacing = 2f;

	public float WestColliderSpacing = 2f;

	public float NorthOffset;

	public float EastOffset;

	public float SouthOffset;

	public float WestOffset;

	private Vector3 Position;

	private Vector3 Size;

	public bool TopLeft = true;

	public bool TopRight = true;

	public bool BottomLeft = true;

	public bool BottomRight = true;

	public void SetSpriteShape()
	{
		SpriteShapeController componentInChildren = GetComponentInChildren<SpriteShapeController>();
		MeshRenderer component = GetComponent<MeshRenderer>();
		float x = component.bounds.extents.x;
		float y = component.bounds.extents.y;
		Spline spline = componentInChildren.spline;
		spline.isOpenEnded = false;
		spline.Clear();
		Vector3 vector = component.bounds.center - base.transform.position;
		spline.InsertPointAt(0, vector + new Vector3(0f - x + 1f, y - 1f));
		spline.InsertPointAt(1, vector + new Vector3(x - 1f, y - 1f));
		spline.InsertPointAt(2, vector + new Vector3(x - 1f, 0f - y + 1f));
		spline.InsertPointAt(3, vector + new Vector3(0f - x + 1f, 0f - y + 1f));
	}

	public void SetColliderGapsToZero()
	{
		NorthColliderSpacing = (EastColliderSpacing = (SouthColliderSpacing = (WestColliderSpacing = 0f)));
		CreateColliders();
	}

	public void CreateColliders()
	{
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i].gameObject);
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		Vector3 extent = component.bounds.extents;
		Vector3 extent2 = component.bounds.extents;
		if (NorthCollider)
		{
			CreateNorthCollider();
		}
		if (EastCollider)
		{
			CreateEastCollider();
		}
		if (SouthCollider)
		{
			CreateSouthCollider();
		}
		if (WestCollider)
		{
			CreateWestCollider();
		}
		if (GetComponent<MeshCollider>() == null)
		{
			base.gameObject.AddComponent<MeshCollider>();
		}
	}

	public void CreateNorthCollider()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		float x = component.bounds.extents.x;
		float y = component.bounds.extents.y;
		Transform transform = base.transform.Find("Collider North");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider North Top");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider North Bottom");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		if (!NorthCollider)
		{
			return;
		}
		if (NorthColliderSpacing > 0f)
		{
			if (!TopRight)
			{
				Position = component.bounds.center + new Vector3(x / 2f + NorthColliderSpacing / 4f + NorthOffset / 4f, y + 0.5f);
				Size = new Vector2(x * 2f / 2f - NorthColliderSpacing / 2f - NorthOffset / 2f, 1f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(x / 2f + NorthColliderSpacing / 4f + 0.5f + NorthOffset / 4f, y + 0.5f);
				Size = new Vector2((x * 2f + 2f) / 2f - NorthColliderSpacing / 2f - NorthOffset / 2f, 1f);
			}
			CreateCollider("Collider North Top", Position, Size);
			if (!TopLeft)
			{
				Position = component.bounds.center + new Vector3(0f - x / 2f - NorthColliderSpacing / 4f + NorthOffset / 4f, y + 0.5f);
				Size = new Vector2(x * 2f / 2f - NorthColliderSpacing / 2f + NorthOffset / 2f, 1f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(0f - x / 2f - NorthColliderSpacing / 4f - 0.5f + NorthOffset / 4f, y + 0.5f);
				Size = new Vector2((x * 2f + 2f) / 2f - NorthColliderSpacing / 2f + NorthOffset / 2f, 1f);
			}
			CreateCollider("Collider North Bottom", Position, Size);
		}
		else if (TopLeft && TopRight)
		{
			CreateCollider("Collider North", component.bounds.center + new Vector3(0f, y + 0.5f), new Vector2(x * 2f + 2f, 1f));
		}
		else if (TopLeft)
		{
			CreateCollider("Collider North", component.bounds.center + new Vector3(-0.5f, y + 0.5f), new Vector2(x * 2f + 1f, 1f));
		}
		else if (TopRight)
		{
			CreateCollider("Collider North", component.bounds.center + new Vector3(0.5f, y + 0.5f), new Vector2(x * 2f + 1f, 1f));
		}
		else
		{
			CreateCollider("Collider North", component.bounds.center + new Vector3(0f, y + 0.5f), new Vector2(x * 2f, 1f));
		}
	}

	public void CreateEastCollider()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		float x = component.bounds.extents.x;
		float y = component.bounds.extents.y;
		Transform transform = base.transform.Find("Collider East");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider East Top");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider East Bottom");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		if (!EastCollider)
		{
			return;
		}
		if (EastColliderSpacing > 0f)
		{
			if (!TopRight)
			{
				Position = component.bounds.center + new Vector3(x + 0.5f, y / 2f + EastColliderSpacing / 4f + EastOffset / 4f);
				Size = new Vector2(1f, y * 2f / 2f - EastColliderSpacing / 2f - EastOffset / 2f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(x + 0.5f, y / 2f + EastColliderSpacing / 4f + 0.5f + EastOffset / 4f);
				Size = new Vector2(1f, (y * 2f + 2f) / 2f - EastColliderSpacing / 2f - EastOffset / 2f);
			}
			CreateCollider("Collider East Top", Position, Size);
			if (!BottomRight)
			{
				Position = component.bounds.center + new Vector3(x + 0.5f, 0f - y / 2f - EastColliderSpacing / 4f + EastOffset / 4f);
				Size = new Vector2(1f, y * 2f / 2f - EastColliderSpacing / 2f + EastOffset / 2f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(x + 0.5f, 0f - y / 2f - EastColliderSpacing / 4f - 0.5f + EastOffset / 4f);
				Size = new Vector2(1f, (y * 2f + 2f) / 2f - EastColliderSpacing / 2f + EastOffset / 2f);
			}
			CreateCollider("Collider East Bottom", Position, Size);
		}
		else if (TopRight && BottomRight)
		{
			CreateCollider("Collider East", component.bounds.center + new Vector3(x + 0.5f, 0f), new Vector2(1f, y * 2f + 2f));
		}
		else if (TopRight)
		{
			CreateCollider("Collider East", component.bounds.center + new Vector3(x + 0.5f, 0.5f), new Vector2(1f, y * 2f + 1f));
		}
		else if (BottomRight)
		{
			CreateCollider("Collider East", component.bounds.center + new Vector3(x + 0.5f, -0.5f), new Vector2(1f, y * 2f + 1f));
		}
		else
		{
			CreateCollider("Collider East", component.bounds.center + new Vector3(x + 0.5f, 0f), new Vector2(1f, y * 2f));
		}
	}

	public void CreateSouthCollider()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		float x = component.bounds.extents.x;
		float y = component.bounds.extents.y;
		Transform transform = base.transform.Find("Collider South");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider South Top");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider South Bottom");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		if (!SouthCollider)
		{
			return;
		}
		if (SouthColliderSpacing > 0f)
		{
			if (!BottomRight)
			{
				Position = component.bounds.center + new Vector3(x / 2f + SouthColliderSpacing / 4f + SouthOffset / 4f, 0f - y - 0.5f);
				Size = new Vector2(x * 2f / 2f - SouthColliderSpacing / 2f - SouthOffset / 2f, 1f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(x / 2f + SouthColliderSpacing / 4f + 0.5f + SouthOffset / 4f, 0f - y - 0.5f);
				Size = new Vector2((x * 2f + 2f) / 2f - SouthColliderSpacing / 2f - SouthOffset / 2f, 1f);
			}
			CreateCollider("Collider South Top", Position, Size);
			if (!BottomLeft)
			{
				Position = component.bounds.center + new Vector3(0f - x / 2f - SouthColliderSpacing / 4f + SouthOffset / 4f, 0f - y - 0.5f);
				Size = new Vector2(x * 2f / 2f - SouthColliderSpacing / 2f + SouthOffset / 2f, 1f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(0f - x / 2f - SouthColliderSpacing / 4f - 0.5f + SouthOffset / 4f, 0f - y - 0.5f);
				Size = new Vector2((x * 2f + 2f) / 2f - SouthColliderSpacing / 2f + SouthOffset / 2f, 1f);
			}
			CreateCollider("Collider South Bottom", Position, Size);
		}
		else if (BottomLeft && BottomRight)
		{
			CreateCollider("Collider South", component.bounds.center + new Vector3(0f, 0f - y - 0.5f), new Vector2(x * 2f + 2f, 1f));
		}
		else if (BottomLeft)
		{
			CreateCollider("Collider South", component.bounds.center + new Vector3(-0.5f, 0f - y - 0.5f), new Vector2(x * 2f + 1f, 1f));
		}
		else if (BottomRight)
		{
			CreateCollider("Collider South", component.bounds.center + new Vector3(0.5f, 0f - y - 0.5f), new Vector2(x * 2f + 1f, 1f));
		}
		else
		{
			CreateCollider("Collider South", component.bounds.center + new Vector3(0f, 0f - y - 0.5f), new Vector2(x * 2f, 1f));
		}
	}

	public void CreateWestCollider()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		float x = component.bounds.extents.x;
		float y = component.bounds.extents.y;
		Transform transform = base.transform.Find("Collider West");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider West Top");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		transform = base.transform.Find("Collider West Bottom");
		if (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		if (!WestCollider)
		{
			return;
		}
		if (WestColliderSpacing > 0f)
		{
			if (!TopLeft)
			{
				Position = component.bounds.center + new Vector3(0f - x - 0.5f, y / 2f + WestColliderSpacing / 4f + WestOffset / 4f);
				Size = new Vector2(1f, y * 2f / 2f - WestColliderSpacing / 2f - WestOffset / 2f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(0f - x - 0.5f, y / 2f + WestColliderSpacing / 4f + 0.5f + WestOffset / 4f);
				Size = new Vector2(1f, (y * 2f + 2f) / 2f - WestColliderSpacing / 2f - WestOffset / 2f);
			}
			CreateCollider("Collider West Top", Position, Size);
			if (!BottomLeft)
			{
				Position = component.bounds.center + new Vector3(0f - x - 0.5f, 0f - y / 2f - WestColliderSpacing / 4f + WestOffset / 4f);
				Size = new Vector2(1f, y * 2f / 2f - WestColliderSpacing / 2f + WestOffset / 2f);
			}
			else
			{
				Position = component.bounds.center + new Vector3(0f - x - 0.5f, 0f - y / 2f - WestColliderSpacing / 4f - 0.5f + WestOffset / 4f);
				Size = new Vector2(1f, (y * 2f + 2f) / 2f - WestColliderSpacing / 2f + WestOffset / 2f);
			}
			CreateCollider("Collider West Bottom", Position, Size);
		}
		else if (TopLeft && BottomLeft)
		{
			CreateCollider("Collider West", component.bounds.center + new Vector3(0f - x - 0.5f, 0f), new Vector2(1f, y * 2f + 2f));
		}
		else if (TopLeft)
		{
			CreateCollider("Collider West", component.bounds.center + new Vector3(0f - x - 0.5f, 0.5f), new Vector2(1f, y * 2f + 1f));
		}
		else if (BottomLeft)
		{
			CreateCollider("Collider West", component.bounds.center + new Vector3(0f - x - 0.5f, -0.5f), new Vector2(1f, y * 2f + 1f));
		}
		else
		{
			CreateCollider("Collider West", component.bounds.center + new Vector3(0f - x - 0.5f, 0f), new Vector2(1f, y * 2f));
		}
	}

	private void CreateCollider(string Name, Vector3 Position, Vector2 Size)
	{
		GameObject obj = new GameObject();
		obj.transform.parent = base.transform;
		obj.name = Name;
		obj.transform.position = Position;
		obj.AddComponent<BoxCollider2D>().size = Size;
		obj.layer = LayerMask.NameToLayer("Island");
	}
}
